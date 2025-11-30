using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ISession = NHibernate.ISession;

namespace DealEngine.Services.Interfaces
{
    public interface ILocalTaskRepository
    {
        Task SaveRangeAsync(IEnumerable<LocalTask> tasks, CancellationToken ct = default);
        Task<IList<LocalTask>> GetPendingByProjectAsync(int projectId, CancellationToken ct = default);
        Task UpdateRangeAsync(IEnumerable<LocalTask> tasks, CancellationToken ct = default);
    }

    public sealed class LocalTaskRepository : ILocalTaskRepository
    {
        private readonly ISession _session;
        public LocalTaskRepository(ISession session) => _session = session;

        public async Task SaveRangeAsync(IEnumerable<LocalTask> tasks, CancellationToken ct = default)
        {
            using var tx = _session.BeginTransaction();
            foreach (var t in tasks) await _session.SaveAsync(t, ct);
            await tx.CommitAsync(ct);
        }

        public async Task<IList<LocalTask>> GetPendingByProjectAsync(int projectId, CancellationToken ct = default)
        {
            return await _session.Query<LocalTask>()
                .Where(x => x.ProjectId == projectId && x.SyncStatus == TaskSyncStatus.Pending)
                .OrderBy(x => x.CreatedAtUtc)
                .ToListAsync(ct);
        }

        public async Task UpdateRangeAsync(IEnumerable<LocalTask> tasks, CancellationToken ct = default)
        {
            using var tx = _session.BeginTransaction();
            foreach (var t in tasks) await _session.UpdateAsync(t, ct);
            await tx.CommitAsync(ct);
        }
    }
}
