using System;
using System.Collections.Generic;
using System.Text;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class OrganisationEvent : EntityBase, IAggregateRoot
    {
        protected OrganisationEvent() : base (null) { }

        public OrganisationEvent(User createdBy, string eventName)
            : base (createdBy)
        {
            EventName = eventName;
        }

        public virtual string EventName { get; set; }

        public virtual Guid OrganisationId { get; set; }

        public virtual Guid OldClientProgrammeId { get; set; }

        public virtual Guid NewClientProgrammeId { get; set; }

        public virtual DateTime EventDate { get; set; }

    }
}
