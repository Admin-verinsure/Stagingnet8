using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class ClientAgreementRule : EntityBase, IAggregateRoot
    {
        protected ClientAgreementRule() : base (null) { }

		public ClientAgreementRule (User createdBy, Rule parentRule, ClientAgreement clientAgreement)
			: this (createdBy, parentRule, parentRule.Name, parentRule.Description, parentRule.Product, parentRule.Value, parentRule.OrderNumber, parentRule.RuleCategory, parentRule.RuleRoleType, parentRule.IsPublic, clientAgreement, parentRule.DoNotCheckForRenew)
		{ }

        public ClientAgreementRule(User createdBy, Rule parentRule, string name, string description, Product product, string value, int orderNumber, string ruleCategory, string ruleRoleType, bool isPublic, ClientAgreement clientAgreement, bool doNotCheckForRenew)
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
            if (string.IsNullOrWhiteSpace(orderNumber.ToString()))
                throw new ArgumentNullException(nameof(orderNumber));
            if (clientAgreement == null)
                throw new ArgumentNullException(nameof(clientAgreement));

            Name = name;
            Description = description;
            Product = product;
            Value = value;
            OrderNumber = orderNumber;
            RuleCategory = ruleCategory;
            RuleRoleType = ruleRoleType;
            IsPublic = isPublic;
            ClientAgreement = clientAgreement;
            DoNotCheckForRenew = doNotCheckForRenew;
        }

        public virtual string Name
        {
            get;
            protected set;
        }

        public virtual string Description
        {
            get;
            protected set;
        }

        public virtual string reference
        {
            get;
            protected set;
        }

        public virtual Rule Rule
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

        public virtual string RuleCategory
        {
            get;
            protected set;
        }

        public virtual string RuleRoleType
        {
            get;
            protected set;
        }

        public virtual bool IsPublic { get; protected set; }

        public virtual bool DoNotCheckForRenew { get; set; }

    }
}
