using System;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class ClubActivities : EntityBase, IAggregateRoot
    {
        public virtual string Name { get; set; }
       
        public virtual Programme Programme { get; set; }

        protected ClubActivities() : this (null) { }

		public ClubActivities(User createdBy)
			: base (createdBy)
		{
		}
	}
}

