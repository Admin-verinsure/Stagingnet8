using DealEngine.Domain.Entities.Abstracts;
using System;
using System.Linq;

namespace DealEngine.Domain.Entities
{
    public class PreRenewOrRefData : EntityBase, IAggregateRoot
    {
        protected PreRenewOrRefData() : base(null) { }

        public PreRenewOrRefData(User createdBy, string dataType, string refField)
            : base(createdBy)
        {
            DataType = dataType;
            RefField = refField;

        }
        public virtual string DataType { get; set; }
        public virtual string RefField { get; set; }
        public virtual string PIBoundLimit { get; set; }
        public virtual string PIBoundPremium { get; set; }
        public virtual string PIRetro { get; set; }
        public virtual string GLRetro { get; set; }
        public virtual string DORetro { get; set; }
        public virtual string ELRetro { get; set; }
        public virtual string EDRetro { get; set; }
        public virtual string SLRetro { get; set; }
        public virtual string CLRetro { get; set; }
        public virtual string OTRetro { get; set; }
        public virtual string LPDRetro { get; set; }
        public virtual string FIDRetro { get; set; }
        public virtual string EndorsementTitle { get; set; }
        public virtual string EndorsementProduct { get; set; }
        public virtual string EndorsementText { get; set; }
        public virtual string PIBoundExcess { get; set; }
    }
}