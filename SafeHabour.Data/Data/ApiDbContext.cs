using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SafeHabour.Data.Entities;

namespace SafeHabour.Data.Data;

public class ApiDbContext : IdentityDbContext<User, UserRole, Guid>
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    // Entity DbSets
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Transfer> Transfers { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Dispute> Disputes { get; set; }
    public DbSet<ClientUser> ClientUsers { get; set; }
    public DbSet<ServiceWorkerUser> ServiceWorkerUsers { get; set; }
    public DbSet<SuperAdmin> SuperAdmins { get; set; }
    public DbSet<UserNotificationSetting> UserNotificationSettings { get; set; }
    public DbSet<TwoFactorAuthCode> TwoFactorAuthCodes { get; set; }
    public DbSet<PushNotification> PushNotifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Identity tables
        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        });

        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
        });

        // Configure Job entity
        builder.Entity<Job>(entity =>
        {
            entity.ToTable("Jobs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Budget).HasColumnType("decimal(18,2)");

            // Configure relationship with User (Client)
            entity.HasOne(e => e.Client)
                  .WithMany(u => u.PostedJobs)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Application entity
        builder.Entity<Application>(entity =>
        {
            entity.ToTable("Applications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ProposedRate).HasColumnType("decimal(18,2)");

            // Configure relationships
            entity.HasOne(e => e.Job)
                  .WithMany(j => j.Applications)
                  .HasForeignKey(e => e.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceWorker)
                  .WithMany(u => u.Applications)
                  .HasForeignKey(e => e.ServiceWorkerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Payment entity
        builder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(18,2)");

            // Configure relationships
            entity.HasOne(e => e.Job)
                  .WithMany(j => j.Payments)
                  .HasForeignKey(e => e.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Client)
                  .WithMany(u => u.ClientPayments)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ServiceWorker)
                  .WithMany(u => u.ServiceWorkerPayments)
                  .HasForeignKey(e => e.ServiceWorkerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Transfer entity
        builder.Entity<Transfer>(entity =>
        {
            entity.ToTable("Transfers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

            // Configure relationships
            entity.HasOne(e => e.Payment)
                  .WithOne()
                  .HasForeignKey<Transfer>(e => e.PaymentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceWorker)
                  .WithMany(u => u.Transfers)
                  .HasForeignKey(e => e.ServiceWorkerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Schedule entity
        builder.Entity<Schedule>(entity =>
        {
            entity.ToTable("Schedules");
            entity.HasKey(e => e.Id);

            // Configure relationship with ServiceWorker
            entity.HasOne(e => e.ServiceWorker)
                  .WithMany(u => u.Schedules)
                  .HasForeignKey(e => e.ServiceWorkerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Review entity
        builder.Entity<Review>(entity =>
        {
            entity.ToTable("Reviews");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(1000);

            // Configure relationships
            entity.HasOne(e => e.Job)
                  .WithMany()
                  .HasForeignKey(e => e.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Reviewer)
                  .WithMany(u => u.ReviewsGiven)
                  .HasForeignKey(e => e.ReviewerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reviewee)
                  .WithMany(u => u.ReviewsReceived)
                  .HasForeignKey(e => e.RevieweeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Dispute entity
        builder.Entity<Dispute>(entity =>
        {
            entity.ToTable("Disputes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(18,2)");

            // Configure relationships
            entity.HasOne(e => e.Job)
                  .WithMany()
                  .HasForeignKey(e => e.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Initiator)
                  .WithMany(u => u.InitiatedDisputes)
                  .HasForeignKey(e => e.InitiatorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Respondent)
                  .WithMany(u => u.RespondentDisputes)
                  .HasForeignKey(e => e.RespondentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedAdmin)
                  .WithMany(u => u.AssignedDisputes)
                  .HasForeignKey(e => e.AssignedAdminId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ClientUser entity
        builder.Entity<ClientUser>(entity =>
        {
            entity.ToTable("ClientUsers");
            entity.HasKey(e => e.Id);

            // Configure relationship with User
            entity.HasOne(e => e.User)
                  .WithOne(u => u.ClientProfile)
                  .HasForeignKey<ClientUser>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ServiceWorkerUser entity
        builder.Entity<ServiceWorkerUser>(entity =>
        {
            entity.ToTable("ServiceWorkerUsers");
            entity.HasKey(e => e.Id);

            // Configure relationship with User
            entity.HasOne(e => e.User)
                  .WithOne(u => u.ServiceWorkerProfile)
                  .HasForeignKey<ServiceWorkerUser>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SuperAdmin entity
        builder.Entity<SuperAdmin>(entity =>
        {
            entity.ToTable("SuperAdmins");
            entity.HasKey(e => e.Id);

            // Configure relationship with User
            entity.HasOne(e => e.User)
                  .WithOne(u => u.SuperAdminProfile)
                  .HasForeignKey<SuperAdmin>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserNotificationSetting entity
        builder.Entity<UserNotificationSetting>(entity =>
        {
            entity.ToTable("UserNotificationSettings");
            entity.HasKey(e => e.Id);

            // Create unique constraint on UserId and NotificationType to prevent duplicates
            entity.HasIndex(e => new { e.UserId, e.NotificationType }).IsUnique();

            // Configure relationship with User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.NotificationSettings)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TwoFactorAuthCode entity
        builder.Entity<TwoFactorAuthCode>(entity =>
        {
            entity.ToTable("TwoFactorAuthCodes");
            entity.HasKey(e => e.Id);

            // Configure relationship with User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.TwoFactorAuthCodes)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Add index for faster lookups
            entity.HasIndex(e => new { e.UserId, e.Code });
            entity.HasIndex(e => e.ExpiresAt);
        });

        // Configure PushNotification entity
        builder.Entity<PushNotification>(entity =>
        {
            entity.ToTable("PushNotifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);

            // Configure relationship with User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.PushNotifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Add indexes for efficient querying
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ExpiresAt);
        });
    }

}
