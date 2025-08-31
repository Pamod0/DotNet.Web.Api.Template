using DotNet.Web.Api.Template.Models;
using DotNet.Web.Api.Template.Models.Audit;
using DotNet.Web.Api.Template.Models.Auth;
using DotNet.Web.Api.Template.Models.FileUploads;
using DotNet.Web.Api.Template.Models.Notification;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System.Security.Claims;

namespace DotNet.Web.Api.Template.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<AuditEntry> AuditEntries { get; set; }
        public DbSet<UserActionLog> UserActionLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<SupportDocument> SupportDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        ));
                    }
                }
            }

            builder.Entity<SupportDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            });

            // Seed roles (remain here as they are part of Identity setup)
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "Admin", NormalizedName = "ADMIN" },
                new ApplicationRole { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "User", NormalizedName = "USER" }
            );

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Get current user ID
            var userId = GetCurrentUserId();

            // Track changes for auditing
            await AuditChanges(userId);

            // Set CreatedAt, CreatedUserId, UpdatedAt, UpdatedUserId, IsDeleted for BaseEntity
            ApplyBaseEntityTracking(userId);

            return await base.SaveChangesAsync(cancellationToken);
        }
        public override int SaveChanges()
        {
            var userId = GetCurrentUserId();
            AuditChanges(userId).Wait(); // Blocking call for synchronous SaveChanges
            ApplyBaseEntityTracking(userId);
            return base.SaveChanges();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return Guid.Empty; // Or handle as appropriate if user is not logged in
        }

        private void ApplyBaseEntityTracking(Guid userId)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

            foreach (var entry in entries)
            {
                var baseEntity = (BaseEntity)entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                        baseEntity.CreatedAt = DateTime.UtcNow;
                        baseEntity.CreatedUserId = userId;
                        baseEntity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        baseEntity.UpdatedAt = DateTime.UtcNow;
                        baseEntity.UpdatedUserId = userId;
                        // For soft deletes, you might have specific logic here
                        if (entry.OriginalValues.GetValue<bool>(nameof(BaseEntity.IsDeleted)) == false &&
                            baseEntity.IsDeleted == true)
                        {
                            baseEntity.DeletedAt = DateTime.UtcNow;
                            baseEntity.DeletedUserId = userId;
                        }
                        break;
                    case EntityState.Deleted:
                        // If you're doing soft deletes, you'd change the state to Modified and set IsDeleted = true
                        // entry.State = EntityState.Modified;
                        // baseEntity.IsDeleted = true;
                        // baseEntity.DeletedAt = DateTime.UtcNow;
                        // baseEntity.DeletedUserId = userId;
                        // If doing hard deletes, no additional BaseEntity tracking needed here,
                        // but the AuditEntry will still record the "Deleted" action.
                        break;
                }
            }
        }

        private Dictionary<string, object> ConvertPropertyValuesToDictionary(PropertyValues propertyValues)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var property in propertyValues.Properties)
            {
                dictionary[property.Name] = propertyValues[property];
            }
            return dictionary;
        }

        private async System.Threading.Tasks.Task AuditChanges(Guid userId)
        {
            ChangeTracker.DetectChanges(); // Ensure all changes are detected

            var auditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                // Skip entries that are not BaseEntity or are in states that do not require auditing
                //if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged || !(entry.Entity is BaseEntity))
                //    continue;

                // Skip entries that are not tracked or are in states that do not require auditing
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry
                {
                    UserId = userId,
                    EntityName = entry.Metadata.GetTableName(),
                    Timestamp = DateTime.UtcNow,
                    EntityId = GetEntityId(entry)
                };

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.ActionType = "Created";
                        auditEntry.Changes = JsonConvert.SerializeObject(ConvertPropertyValuesToDictionary(entry.CurrentValues));
                        break;
                    case EntityState.Modified:
                        auditEntry.ActionType = "Updated";
                        var changes = new Dictionary<string, object>();
                        foreach (var property in entry.OriginalValues.Properties)
                        {
                            var originalValue = entry.OriginalValues[property];
                            var currentValue = entry.CurrentValues[property];

                            if (!object.Equals(originalValue, currentValue))
                            {
                                changes[$"{property.Name} (Old)"] = originalValue;
                                changes[$"{property.Name} (New)"] = currentValue;
                            }
                        }
                        auditEntry.Changes = JsonConvert.SerializeObject(changes);
                        break;
                    case EntityState.Deleted:
                        auditEntry.ActionType = "Deleted";
                        auditEntry.Changes = JsonConvert.SerializeObject(ConvertPropertyValuesToDictionary(entry.OriginalValues));
                        break;
                }

                auditEntries.Add(auditEntry);
            }

            foreach (var auditEntry in auditEntries)
            {
                await AuditEntries.AddAsync(auditEntry);
            }
        }

        private string GetEntityId(EntityEntry entry)
        {
            var primaryKey = entry.Metadata.FindPrimaryKey();
            if (primaryKey != null)
            {
                var keyValues = primaryKey.Properties
                    .Select(p => entry.Property(p.Name).CurrentValue)
                    .Where(v => v != null)
                    .Select(v => v.ToString())
                    .ToList();
                return string.Join(",", keyValues);
            }
            return null;
        }
    }
}