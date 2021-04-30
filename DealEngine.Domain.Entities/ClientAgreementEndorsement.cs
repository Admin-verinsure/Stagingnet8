using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class ClientAgreementEndorsement : EntityBase, IAggregateRoot
    {
        protected ClientAgreementEndorsement() : base(null) { }

		public ClientAgreementEndorsement(User createdBy, Endorsement parentEndorsement, ClientAgreement clientAgreement)
			: this (createdBy, parentEndorsement.Name, parentEndorsement.Type, parentEndorsement.Product, parentEndorsement.Value, parentEndorsement.OrderNumber, clientAgreement)
		{ }

        public ClientAgreementEndorsement(User createdBy, string name, string type, Product product, string value, int orderNumber, ClientAgreement clientAgreement)
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
            if (string.IsNullOrWhiteSpace(orderNumber.ToString()))
                throw new ArgumentNullException(nameof(orderNumber));
            if (clientAgreement == null)
                throw new ArgumentNullException(nameof(clientAgreement));

            Name = name;
            Type = type;
            Product = product;
            Value = value;
            OrderNumber = orderNumber;
            ClientAgreement = clientAgreement;
        }

        public virtual string Name
        {
            get;
            set;
        }

        public virtual string Type
        {
            get;
            set;
        }

        public virtual Endorsement Endorsement
        {
            get;
            protected set;
        }

        public virtual Product Product
        {
            get;
            protected set;
        }

        public virtual ClientAgreement ClientAgreement
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
            set;
        }

        public virtual bool Removed
        {
            get;
            set;
        }

    }
}
