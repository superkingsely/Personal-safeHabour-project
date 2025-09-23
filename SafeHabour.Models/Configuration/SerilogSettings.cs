namespace SafeHabour.Models.Configuration;

public class SerilogSettings
{
    public const string SectionName = "Serilog";
    
    public DatabaseSinkSettings? Database { get; set; }
    public FileSinkSettings? File { get; set; }
    public ConsoleSinkSettings? Console { get; set; }
    public string MinimumLevel { get; set; } = "Information";
    public Dictionary<string, string> MinimumLevelOverrides { get; set; } = new();
}

public class DatabaseSinkSettings
{
    public bool Enabled { get; set; } = true;
    public string ConnectionStringName { get; set; } = "DefaultConnection";
    public string TableName { get; set; } = "Logs";
    public string SchemaName { get; set; } = "dbo";
    public bool AutoCreateSqlTable { get; set; } = true;
    public string RestrictedToMinimumLevel { get; set; } = "Information";
}

public class FileSinkSettings
{
    public bool Enabled { get; set; } = true;
    public string Path { get; set; } = "wwwroot/logs/SafeHabour-.txt";
    public string RollingInterval { get; set; } = "Day";
    public string RestrictedToMinimumLevel { get; set; } = "Information";
    public int? RetainedFileCountLimit { get; set; } = 30;
    public long? FileSizeLimitBytes { get; set; } = 1073741824; // 1GB
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
}

public class ConsoleSinkSettings
{
    public bool Enabled { get; set; } = true;
    public string RestrictedToMinimumLevel { get; set; } = "Information";
    public string OutputTemplate { get; set; } = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
}
