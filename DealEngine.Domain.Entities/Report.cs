using System;
using System.Collections.Generic;
using System.Linq;
using DealEngine.Domain.Entities.Abstracts;


namespace DealEngine.Domain.Entities
{
    public class PIReport : EntityBase
    {
        public PIReport() : this(null) { }
        public PIReport(User createdBy) : base(createdBy)
        {
        }
        public virtual string ReferenceID { get; set; }
        public virtual string IndividualName { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string Inceptiondate { get; set; }
        public virtual string selectedlimit { get; set; }
        public virtual string Premium { get; set; }

    }



    public class EDReport : EntityBase
    {
        public EDReport() : this(null) { }

        public EDReport(User createdBy) : base(createdBy)
        {
        }
        public virtual string ReferenceID { get; set; }
        public virtual string IndividualName { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string Inceptiondate { get; set; }
        public virtual string selectedlimit { get; set; }
        public virtual string Premium { get; set; }

    }

    public class FAPReport : EntityBase
    {
        public FAPReport() : this(null) { }

        public FAPReport(User createdBy) : base(createdBy)
        {
        }
        public virtual string Email { get; set; }
        public virtual string MemberName { get; set; }
        public virtual string Status { get; set; }
        public virtual string AdvisorName { get; set; }
        public virtual string Hastraditionallicence { get; set; }
        public virtual string Hasadvisers { get; set; }
        public virtual string Hasadditionaltraditionallicence { get; set; }

    }
}



