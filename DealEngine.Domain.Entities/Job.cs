using System;
using System.Collections.Generic;
using System.Text;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class Job : EntityBase
    {
        public Job() : base(null)
        {

        }

        public Job(User createdBy) : base(createdBy)
        {

        }
        public virtual string JobDescription { get; set; }
        public virtual string ContractorEmail { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string JobNumber { get; set; }
        public virtual DateTime IssueDate { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual DateTime CertRequiredBy { get; set; }
        public virtual string RACLientNamedParty { get; set; }
        public virtual bool Removed { get; set; }
    }

}
