using DealEngine.Domain.Entities.Abstracts;
using System;
using System.Collections.Generic;

namespace DealEngine.Domain.Entities
{
    public enum TaskSyncStatus { Pending = 0, Synced = 1, Failed = 2 }

    public class LocalTask : EntityBase, IAggregateRoot
    {
        protected LocalTask() : base(null) { } // for NH

        public LocalTask(
            User createdBy,
            string title,
            int projectId,
            string? notes = null,
            DateTime? deadline = null,
            int? assigneeUserId = null,
            string? tagListCsv = null)
            : base(createdBy)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));

            Title = title;
            ProjectId = projectId;
            Notes = notes;
            Deadline = deadline;
            AssigneeUserId = assigneeUserId;
            TagListCsv = tagListCsv;

            SyncStatus = TaskSyncStatus.Pending;
            CreatedAtUtc = DateTime.UtcNow;
            CorrelationKey = Guid.NewGuid().ToString("N");
        }

        public virtual string Title { get; protected set; }
        public virtual string? Notes { get; protected set; }
        public virtual int ProjectId { get; protected set; }
        public virtual DateTime? Deadline { get; protected set; }
        public virtual int? AssigneeUserId { get; protected set; }
        public virtual string? TagListCsv { get; protected set; }

        public virtual int? OdooTaskId { get; protected set; }
        public virtual TaskSyncStatus SyncStatus { get; protected set; }
        public virtual DateTime CreatedAtUtc { get; protected set; }
        public virtual DateTime? SyncedAtUtc { get; protected set; }
        public virtual string CorrelationKey { get; protected set; }


        // domain behaviors
        public virtual void MarkSynced(int odooTaskId)
        {
            OdooTaskId = odooTaskId;
            SyncStatus = TaskSyncStatus.Synced;
            SyncedAtUtc = DateTime.UtcNow;
        }

        public virtual void MarkFailed() => SyncStatus = TaskSyncStatus.Failed;

        public virtual void UpdateNotes(string? notes) => Notes = notes;
        public virtual void UpdateDeadline(DateTime? deadline) => Deadline = deadline;
        public virtual void UpdateAssignee(int? userId) => AssigneeUserId = userId;
        public virtual void UpdateTagsCsv(string? csv) => TagListCsv = csv;
    }
}
