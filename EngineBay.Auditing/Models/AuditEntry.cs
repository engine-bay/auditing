namespace EngineBay.Auditing
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using EngineBay.Core;
    using Humanizer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class AuditEntry : BaseModel
    {
        public string? EntityName { get; set; }

        public string? ActionType { get; set; }

        public Guid? ApplicationUserId { get; set; }

        public string? ApplicationUserName { get; set; }

        public string? EntityId { get; set; }

        public string? Changes { get; set; }

#pragma warning disable CA2227 // These properties are not read only as they are set during the auditing process.
        [NotMapped]
        public Dictionary<string, object?>? TempChanges { get; set; }

        [NotMapped]
        public ICollection<PropertyEntry>? TempProperties { get; set; } // TempProperties are used for properties that are only generated on save, e.g. ID's

#pragma warning restore CA2227
        public static new void CreateDataAnnotations(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<AuditEntry>().ToTable(typeof(AuditEntry).Name.Pluralize());

            modelBuilder.Entity<AuditEntry>().HasKey(x => x.Id);

            modelBuilder.Entity<AuditEntry>().Property(x => x.CreatedAt).IsRequired();

            modelBuilder.Entity<AuditEntry>().Property(x => x.LastUpdatedAt).IsRequired();

            modelBuilder.Entity<AuditEntry>().Property(x => x.EntityName).IsRequired();

            modelBuilder.Entity<AuditEntry>().Property(x => x.ActionType).IsRequired();

            modelBuilder.Entity<AuditEntry>().Property(x => x.EntityId).IsRequired();

            modelBuilder.Entity<AuditEntry>().Property(x => x.Changes).IsRequired();

            modelBuilder.Entity<AuditEntry>().Property(x => x.ApplicationUserId).IsRequired();

            modelBuilder.Entity<AuditEntry>().Property(x => x.ApplicationUserName).IsRequired();

            modelBuilder.Entity<AuditEntry>().Ignore(x => x.TempProperties);

            modelBuilder.Entity<AuditEntry>().Ignore(x => x.TempChanges);
        }
    }
}