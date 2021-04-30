using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class Rule : EntityBase, IAggregateRoot
    {
        protected Rule() : base (null) { }

        public Rule(User createdBy, string name, string description, Product product, string value)
			: base (createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            Name = name;
            Description = description;
            Product = product;
            Value = value;
            IsPublic = false;
        }

        public virtual string Name
        {
            get;
            set;
        }

        public virtual string Description
        {
            get;
            set;
        }

        public virtual Product Product
        {
            get;
            protected set;
        }

        public virtual int OrderNumber
        {
            get;
            set;
        }

        public virtual string Value
        {
            get;
            set;
        }

        //uwrate, uwreferral
        public virtual string RuleCategory
        {
            get;
            set;
        }

        //ruleroletc, rulerolebroker, ruleroleinsurer
        public virtual string RuleRoleType
        {
            get;
            set;
        }

        public virtual bool IsPublic { get; protected set; }

        public virtual bool DoNotCheckForRenew { get; set; }

    }
}
