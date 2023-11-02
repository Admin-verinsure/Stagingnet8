using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class Endorsement : EntityBase, IAggregateRoot
    {
        protected Endorsement() : base(null) { }

        public Endorsement(User createdBy, string name, string type, Product product, string value)
            : base(createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentNullException(nameof(type));
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            Name = name;
            Type = type;
            Product = product;
            Value = value;
        }

        public virtual string Name
        {
            get;
            protected set;
        }

        public virtual string Type
        {
            get;
            protected set;
        }

        public virtual Product Product
        {
            get;
            protected set;
        }

        public virtual InformationTemplate InformationTemplate
        {
            get;
            protected set;
        }

        public virtual Question Question
        {
            get;
            protected set;
        }

        public virtual int OrderNumber
        {
            get;
            protected set;
        }

        public virtual string Value
        {
            get;
            protected set;
        }

        public virtual string IsManual
        {
            get;
            protected set;
        }


    }
}

