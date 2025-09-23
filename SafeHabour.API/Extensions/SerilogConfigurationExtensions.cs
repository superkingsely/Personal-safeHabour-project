using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using SafeHabour.Models.Configuration;
using System.Collections.ObjectModel;
using System.Data;

namespace SafeHabour.API.Extensions;

public static class SerilogConfigurationExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        
        // Configure Serilog settings
        builder.Services.Configure<SerilogSettings>(
            configuration.GetSection(SerilogSettings.SectionName));
        
        // Build a temporary service provider to get the settings
        var tempServiceProvider = builder.Services.BuildServiceProvider();
        var serilogSettings = tempServiceProvider.GetRequiredService<IOptions<SerilogSettings>>().Value;
        
        // Get connection string
        var connectionString = configuration.GetConnectionString(serilogSettings.Database?.ConnectionStringName ?? "DefaultConnection");
        
        // Create logger configuration
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(ParseLogLevel(serilogSettings.MinimumLevel))
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithClientIp();
        
        // Register HttpContextAccessor for the enricher
        builder.Services.AddHttpContextAccessor();
        
        // Add minimum level overrides
        foreach (var (source, level) in serilogSettings.MinimumLevelOverrides)
        {
            loggerConfiguration.MinimumLevel.Override(source, ParseLogLevel(level));
        }
        
        // Configure Console sink
        if (serilogSettings.Console?.Enabled == true)
        {
            loggerConfiguration.WriteTo.Console(
                restrictedToMinimumLevel: ParseLogLevel(serilogSettings.Console.RestrictedToMinimumLevel),
                outputTemplate: serilogSettings.Console.OutputTemplate);
        }
        
        // Configure File sink
        if (serilogSettings.File?.Enabled == true)
        {
            var fileSettings = serilogSettings.File;
            loggerConfiguration.WriteTo.File(
                path: fileSettings.Path,
                restrictedToMinimumLevel: ParseLogLevel(fileSettings.RestrictedToMinimumLevel),
                rollingInterval: ParseRollingInterval(fileSettings.RollingInterval),
                retainedFileCountLimit: fileSettings.RetainedFileCountLimit,
                fileSizeLimitBytes: fileSettings.FileSizeLimitBytes,
                outputTemplate: fileSettings.OutputTemplate);
        }
        
        // Configure Database sink
        if (serilogSettings.Database?.Enabled == true && !string.IsNullOrEmpty(connectionString))
        {
            var dbSettings = serilogSettings.Database;
            var columnOptions = GetColumnOptions();
            
            loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = dbSettings.TableName,
                    SchemaName = dbSettings.SchemaName,
                    AutoCreateSqlTable = dbSettings.AutoCreateSqlTable
                },
                columnOptions: columnOptions,
                restrictedToMinimumLevel: ParseLogLevel(dbSettings.RestrictedToMinimumLevel));
        }
      
        // Clean up temporary service provider
        tempServiceProvider.Dispose();
    }
    
    private static LogEventLevel ParseLogLevel(string level)
    {
        return level?.ToLowerInvariant() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
    
    private static RollingInterval ParseRollingInterval(string interval)
    {
        return interval?.ToLowerInvariant() switch
        {
            "infinite" => RollingInterval.Infinite,
            "year" => RollingInterval.Year,
            "month" => RollingInterval.Month,
            "day" => RollingInterval.Day,
            "hour" => RollingInterval.Hour,
            "minute" => RollingInterval.Minute,
            _ => RollingInterval.Day
        };
    }
    
    private static ColumnOptions GetColumnOptions()
    {
        var columnOptions = new ColumnOptions();
        
        // Remove default columns we don't want
        columnOptions.Store.Remove(StandardColumn.MessageTemplate);
        columnOptions.Store.Remove(StandardColumn.Properties);
        
        // Customize existing columns
        columnOptions.TimeStamp.ConvertToUtc = true;
        columnOptions.TimeStamp.ColumnName = "Timestamp";
        
        // Add custom columns
        columnOptions.AdditionalColumns = new Collection<SqlColumn>
        {
            new SqlColumn { ColumnName = "UserName", DataType = SqlDbType.NVarChar, DataLength = 100 },
            new SqlColumn { ColumnName = "MachineName", DataType = SqlDbType.NVarChar, DataLength = 50 },
            new SqlColumn { ColumnName = "RequestId", DataType = SqlDbType.NVarChar, DataLength = 100 },
            new SqlColumn { ColumnName = "RequestPath", DataType = SqlDbType.NVarChar, DataLength = 500 },
            new SqlColumn { ColumnName = "SourceContext", DataType = SqlDbType.NVarChar, DataLength = 200 },
            new SqlColumn { ColumnName = "ActionName", DataType = SqlDbType.NVarChar, DataLength = 100 },
            new SqlColumn { ColumnName = "ApplicationName", DataType = SqlDbType.NVarChar, DataLength = 50 }
        };
        
        return columnOptions;
    }
}
