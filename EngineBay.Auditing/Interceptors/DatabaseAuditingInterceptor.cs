namespace EngineBay.Auditing
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    public class DatabaseAuditingInterceptor : SaveChangesInterceptor
    {
        private readonly HttpContextWrapper httpContextWrapper;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        private List<AuditEntry>? auditEntries;

        public DatabaseAuditingInterceptor(HttpContextWrapper httpContextWrapper)
        {
            this.httpContextWrapper = httpContextWrapper;
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            AuditChangesToEntity(eventData);

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            AuditChangesToEntity(eventData);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override void SaveChangesCanceled(DbContextEventData eventData)
        {
            auditEntries = null;

            base.SaveChangesCanceled(eventData);
        }

        public override Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
        {
            auditEntries = null;

            return base.SaveChangesCanceledAsync(eventData, cancellationToken);
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            auditEntries = null;

            base.SaveChangesFailed(eventData);
        }

        public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            auditEntries = null;

            return base.SaveChangesFailedAsync(eventData, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            SaveAuditChanges();

            return base.SavedChanges(eventData, result);
        }

        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            SaveAuditChanges();

            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }



        private void AuditChangesToEntity(DbContextEventData eventData)
        {
            var changeTrackerEntries = eventData.Context?.ChangeTracker.Entries();

            if (changeTrackerEntries == null)
            {
                throw new ArgumentNullException(nameof(changeTrackerEntries));
            }

            var entries = new List<AuditEntry>();

            foreach (var entry in changeTrackerEntries)
            {
                // Do not audit entities that are not tracked, not changed, or not of type AuditableModel
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged || entry.Entity is not AuditableModel)
                {
                    continue;
                }

                // Do not audit our audits (should never be true, but this is here explicitly to safeguard against future accidents)
                if (entry.Entity is AuditEntry)
                {
                    continue;
                }

                if (entry.Properties is null)
                {
                    throw new ArgumentException("Auditing change tracker entry properties were null");
                }

                var entityId = entry.Properties.Single(p => p.Metadata.IsPrimaryKey());

                if (entityId is null)
                {
                    throw new ArgumentException("Auditing change tracker entry entityId was null");
                }

                if (entityId.CurrentValue is null)
                {
                    throw new ArgumentException("Auditing change tracker entry current value was null");
                }

                var changes = entry.Properties.Select(p => new { p.Metadata.Name, p.CurrentValue });

                if (changes is null)
                {
                    throw new ArgumentException("Auditing change tracker entry changes were null");
                }

                var auditEntry = new AuditEntry
                {
                    ActionType = entry.State == EntityState.Added ? DatabaseOperationConstants.INSERT : entry.State == EntityState.Deleted ? DatabaseOperationConstants.DELETE : DatabaseOperationConstants.UPDATE,
                    EntityId = entityId.CurrentValue.ToString(),
                    EntityName = entry.Metadata.ClrType.Name,
                    ApplicationUserId = httpContextWrapper.UserId,
                    ApplicationUserName = httpContextWrapper.Username,
                    TempChanges = changes.ToDictionary(i => i.Name, i => i.CurrentValue),
                    TempProperties = entry.Properties.Where(p => p.IsTemporary).ToList(),
                };

                entries.Add(auditEntry);
            }

            auditEntries = entries;
        }

        private void SaveAuditChanges()
        {
            if (auditEntries != null && auditEntries.Count > 0)
            {
                Parallel.ForEach(auditEntries, entry =>
                {
                    if (entry.TempProperties is null)
                    {
                        throw new ArgumentException("Auditing temporary properties collection was null");
                    }

                    if (entry.TempChanges is null)
                    {
                        throw new ArgumentException("Auditing temporary changes collection was null");
                    }

                    foreach (var prop in entry.TempProperties)
                    {
                        if (prop.CurrentValue is not null)
                        {
                            var currentValue = prop.CurrentValue.ToString();
                            if (prop.Metadata.IsPrimaryKey())
                            {
                                entry.EntityId = prop.CurrentValue.ToString();
                                entry.TempChanges[prop.Metadata.Name] = currentValue;
                            }
                            else
                            {
                                entry.TempChanges[prop.Metadata.Name] = currentValue;
                            }
                        }
                    }

                    entry.Changes = JsonConvert.SerializeObject(entry.TempChanges, this.jsonSerializerSettings);
                });
            }

            auditEntries = null;
        }
    }
}
