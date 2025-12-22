using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities.Abstracts;


namespace DealEngine.Domain.Entities
{
    public class OrganisationAttribute : EntityBase, IAggregateRoot
    {
        protected OrganisationAttribute() : base(null) { }

        public OrganisationAttribute(User createdBy)
            : base(createdBy)
        {
        }

        // -------------------------------
        // CLUB MEMBER FIELDS
        // -------------------------------
        public virtual int? ActiveFeePaying { get; set; }
        public virtual int? Honorary { get; set; }
        public virtual int? Associate { get; set; }
        public virtual int? Family { get; set; }
        public virtual int? Community { get; set; }
        public virtual int? Volunteer { get; set; }
        public virtual int? Corporate { get; set; }        // (×3 logic happens in UI)
        public virtual int? Alumni { get; set; }
        public virtual int? Trustees { get; set; }
        public virtual int? OtherMembers { get; set; }
        public virtual int? ClubTotal { get; set; }

        // -------------------------------
        // DISTRICT FIELDS
        // -------------------------------
        public virtual int? Dist_Rotary { get; set; }
        public virtual int? Dist_Rotaract { get; set; }
        public virtual int? Dist_Interact { get; set; }
        public virtual int? Dist_RotaKids { get; set; }
        public virtual int? Dist_CommunityCore { get; set; }
        public virtual int? DistrictTotal { get; set; }

        // -------------------------------
        // SPECIAL PURPOSE TRUST FIELDS
        // -------------------------------
        public virtual int? SPT_Companies { get; set; }
        public virtual int? SPT_TradingTrusts { get; set; }
        public virtual string SPT_RevenueOver1m { get; set; }   // Yes / No
        public virtual int? SPT_Revenue { get; set; }           // Only if Yes
        public virtual int? SPT_Total { get; set; }
    }
}

