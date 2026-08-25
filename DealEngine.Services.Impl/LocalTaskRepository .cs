using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NHibernate;


namespace DealEngine.Services.Impl
{
    public sealed class LocalTaskRepository : ILocalTaskRepository
    {
        private readonly ISession _session;

        public LocalTaskRepository(ISession session) => _session = session;

        public async Task SaveAsync(LocalTask task, CancellationToken ct = default)
        {
            using var tx = _session.BeginTransaction();
            await _session.SaveAsync(task, ct);
            await tx.CommitAsync(ct);
        }

        public async Task SaveRangeAsync(IEnumerable<LocalTask> tasks, CancellationToken ct = default)
        {
            using var tx = _session.BeginTransaction();
            foreach (var t in tasks)
                await _session.SaveAsync(t, ct);

            await tx.CommitAsync(ct);
        }

        public async Task<IList<LocalTask>> GetPendingByProjectAsync(int projectId, CancellationToken ct = default)
        {
            return await _session.Query<LocalTask>()
                .Where(x => x.ProjectId == projectId && x.SyncStatus == TaskSyncStatus.Pending)
                .OrderBy(x => x.CreatedAtUtc)
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(LocalTask task, CancellationToken ct = default)
        {
            using var tx = _session.BeginTransaction();
            await _session.UpdateAsync(task, ct);
            await tx.CommitAsync(ct);
        }

        public async Task UpdateRangeAsync(IEnumerable<LocalTask> tasks, CancellationToken ct = default)
        {
            using var tx = _session.BeginTransaction();
            foreach (var t in tasks)
                await _session.UpdateAsync(t, ct);

            await tx.CommitAsync(ct);
        }

        public async Task<LocalTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _session.GetAsync<LocalTask>(id, ct);
        }

        public async Task<LocalTask?> GetByCorrelationKeyAsync(string correlationKey, CancellationToken ct = default)
        {
            return await _session.Query<LocalTask>()
                .Where(x => x.CorrelationKey == correlationKey)
                .SingleOrDefaultAsync(ct);
        }
    }


}

