using DealEngine.Domain.Entities.Abstracts;
using System;
using System.Collections.Generic;

namespace DealEngine.Domain.Entities
{
    public class OdooTaskSpec : EntityBase, IAggregateRoot
    {
        protected OdooTaskSpec() : base(null) { }

        public OdooTaskSpec(User createdBy) : base(createdBy) { }

        public OdooTaskSpec(
            User createdBy,
            string title,
            int projectId,
            Product product,
            string? notes = null
        )
        : base(createdBy)
        {
            Title = title;
            ProjectId = projectId;
            Notes = notes;     
            Product = product;

        }
        public virtual string Title { get; set; }
        public virtual int ProjectId { get; set; }
        public virtual string? Notes { get; set; }
        public virtual string Value { get; set; }

        public virtual Product Product { get; set; }
        public virtual DateTime Deadline { get; set; }
        public virtual int AssigneeUserId { get; set; }
       // public object TagIds { get; set; }
        //public IEnumerable<int>? TagIds { get; set; }

    }

}
