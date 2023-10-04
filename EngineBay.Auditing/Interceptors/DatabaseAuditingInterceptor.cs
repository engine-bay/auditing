﻿namespace EngineBay.Auditing
{
    using EngineBay.Core;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Newtonsoft.Json;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class DatabaseAuditingInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentIdentity currentIdentity;
        private readonly AuditingWriteDbContext auditingWriteDbContext;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        private List<AuditEntry>? auditEntries;

        public DatabaseAuditingInterceptor(ICurrentIdentity currentIdentity, AuditingWriteDbContext auditingWriteDbContext)
        {
            this.currentIdentity = currentIdentity;
            this.auditingWriteDbContext = auditingWriteDbContext;
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

            AuditChangesToEntity(eventData);

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

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
            CollateAuditChanges();

            auditingWriteDbContext.SaveChanges();

            auditEntries = null;

            return base.SavedChanges(eventData, result);
        }

        public async override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            CollateAuditChanges();

            await auditingWriteDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            auditEntries = null;

            return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
        }



        private void AuditChangesToEntity(DbContextEventData eventData)
        {
            var changeTrackerEntries = eventData.Context?.ChangeTracker.Entries();

            ArgumentNullException.ThrowIfNull(changeTrackerEntries, nameof(changeTrackerEntries));

            var auditEntries = new List<AuditEntry>();

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
                    ApplicationUserId = currentIdentity.UserId,
                    ApplicationUserName = currentIdentity.Username,
                    TempChanges = changes.ToDictionary(i => i.Name, i => i.CurrentValue),
                    TempProperties = entry.Properties.Where(p => p.IsTemporary).ToList(),
                };

                auditEntries.Add(auditEntry);
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

            if (auditEntries != null && auditEntries.Count > 0)
            {
                auditingWriteDbContext.AddRange(auditEntries);
            }
        }
    }
}
