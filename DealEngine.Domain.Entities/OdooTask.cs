using DealEngine.Domain.Entities.Abstracts;
using System;

namespace DealEngine.Domain.Entities
{
    public class OdooTask : EntityBase, IAggregateRoot
    {       
        public OdooTask() : this(null) { }
        public OdooTask(User createdBy) : base(createdBy) { }
        public virtual string Title { get; set; } = "";
        public virtual string? Notes { get; set; }
      //  public DateTime? Due { get; set; }
      //  public bool ReferralSelected { get; set; }
        public virtual int? OdooTaskId { get; set; }

        public virtual string EndPoint { get; set; }
        public virtual string OdooDB { get; set; }
        public virtual string LoginID { get; set; }
        public virtual string LoginKey { get; set; }

    }
}