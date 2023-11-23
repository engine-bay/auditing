namespace EngineBay.Auditing
{
    using EngineBay.Core;
    using EngineBay.Persistence;
    using LinqKit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Newtonsoft.Json;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class AuditingInterceptor : SaveChangesInterceptor, IAuditingInterceptor
    {
        private readonly ICurrentIdentity currentIdentity;
        private readonly AuditingWriteDbContext auditingWriteDbContext;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly bool auditingEnabled;

        private List<AuditEntry>? auditEntries;

        public AuditingInterceptor(ICurrentIdentity currentIdentity, AuditingWriteDbContext auditingWriteDbContext)
        {
            this.currentIdentity = currentIdentity;
            this.auditingWriteDbContext = auditingWriteDbContext;
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            this.auditingEnabled = AuditingConfiguration.IsAuditingEnabled();
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            ArgumentNullException.ThrowIfNull(eventData);

            if (auditingEnabled)
                AuditChangesToEntity(eventData);

            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(eventData);

            if (auditingEnabled)
                await AuditChangesToEntityAsync(eventData, cancellationToken);

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
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
            if (auditingEnabled)
            {
                CollateAuditChanges();

                auditingWriteDbContext.SaveChanges();

                auditEntries = null;
            }

            return base.SavedChanges(eventData, result);
        }

        public async override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (auditingEnabled)
            {
                CollateAuditChanges();

                await auditingWriteDbContext.SaveChangesAsync(cancellationToken);

                auditEntries = null;
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void AuditChangesToEntity(DbContextEventData eventData)
        {
            this.AuditChangesToEntityAsync(eventData, CancellationToken.None).Wait();
        }

        private async Task AuditChangesToEntityAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
        {
            var changeTrackerEntries = eventData.Context?.ChangeTracker.Entries();

            if (changeTrackerEntries is null)
            {
                throw new ArgumentException(nameof(eventData) + " does not contain change tracket entries");
            }

            auditEntries = new List<AuditEntry>();

            foreach (var entry in changeTrackerEntries)
            {
                // Do not audit entities that are not tracked, not changed, or not of type AuditableModel
                if (entry.State != EntityState.Detached && entry.State != EntityState.Unchanged && entry.Entity is AuditableModel model)
                {
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

                    model.LastUpdatedById = this.currentIdentity.UserId;
                    if (entry.State == EntityState.Added)
                    {
                        model.CreatedById = this.currentIdentity.UserId;
                    }

                    var changes = entry.Properties.Select(p => new { p.Metadata.Name, p.CurrentValue });

                    var auditEntry = new AuditEntry
                    {
                        ActionType = entry.State == EntityState.Added ? DatabaseOperationConstants.INSERT : entry.State == EntityState.Deleted ? DatabaseOperationConstants.DELETE : DatabaseOperationConstants.UPDATE,
                        EntityId = entityId.CurrentValue.ToString(),
                        EntityName = entry.Metadata.ClrType.Name,
                        ApplicationUserId = await currentIdentity.GetUserIdAsync(cancellationToken),
                        ApplicationUserName = await currentIdentity.GetUsernameAsync(cancellationToken),
                        TempChanges = changes.ToDictionary(i => i.Name, i => i.CurrentValue),
                        TempProperties = entry.Properties.Where(p => p.IsTemporary).ToList(),
                    };

                    auditEntries.Add(auditEntry);
                }
            }
        }

        private void CollateAuditChanges()
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

                    entry.TempProperties.ForEach(prop =>
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
                    });

                    entry.Changes = JsonConvert.SerializeObject(entry.TempChanges, this.jsonSerializerSettings);
                });
            }

            if (auditEntries != null && auditEntries.Count > 0)
            {
                auditingWriteDbContext.AddRange(auditEntries);
            }
        }
    }
}
