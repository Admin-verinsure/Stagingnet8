using DealEngine.Domain.Entities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealEngine.Domain.Entities
{
    public class ProductTerm : EntityBase
    {
        protected ProductTerm() : base (null) { }

        public ProductTerm(User createdBy, int Limit)
			: base (createdBy)
        {

        }

        public virtual int Limit
        {
            get;
            protected set;
        }

        public virtual int AggregateLimit
        {
            get;
            protected set;
        }

        public virtual int Excess
        {
            get;
            protected set;
        }

        public virtual int HigherExcess
        {
            get;
            protected set;
        }

        public virtual decimal Premium
        {
            get;
            protected set;
        }

        public virtual string Reference
        {
            get;
            protected set;
        }

        public virtual bool DefaultTerm
        {
            get;
            protected set;
        }

        public virtual int OrderNumber
        {
            get;
            protected set;
        }

        public virtual bool Bound
        {
            get;
            protected set;
        }

        public virtual decimal BrokerageRate
        {
            get;
            protected set;
        }

        public virtual decimal Brokerage
        {
            get;
            protected set;
        }

        public virtual decimal NDBrokerageRate
        {
            get;
            protected set;
        }

        public virtual decimal NDBrokerage
        {
            get;
            protected set;
        }

        public virtual decimal ReferralLoading
        {
            get;
            set;
        }

        public virtual decimal ReferralLoadingAmount
        {
            get;
            set;
        }

        public virtual decimal ND
        {
            get;
            protected set;
        }

        public virtual decimal FSL
        {
            get;
            protected set;
        }

        public virtual decimal EQC
        {
            get;
            protected set;
        }

        public virtual Product Product
        {
            get;
            protected set;
        }

        public virtual IEnumerable<SubTermType> SubTermType
        {
            get;
            protected set;
        }

    }
}
