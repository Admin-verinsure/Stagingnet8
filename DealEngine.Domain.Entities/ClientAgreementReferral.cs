using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class ClientAgreementReferral : EntityBase, IAggregateRoot
    {
        protected ClientAgreementReferral() : base(null) { }

        public ClientAgreementReferral(User createdBy, ClientAgreement clientAgreement, string name, string description, string status, string actionName, int orderNumber, bool doNotCheckForRenew)
            : base(createdBy)
        {
            if (clientAgreement == null)
                throw new ArgumentNullException(nameof(clientAgreement));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentNullException(nameof(actionName));

            ClientAgreement = clientAgreement;
            Name = name;
            Description = description;
            Status = status;
            ActionName = actionName;
            OrderNumber = orderNumber;
            DoNotCheckForRenew = doNotCheckForRenew;
        }

        public virtual ClientAgreement ClientAgreement
        {
            get;
            protected set;
        }

        public virtual string Name
        {
            get;
            set;
        }

        public virtual string ActionName
        {
            get;
            set;
        }

        public virtual string Description
        {
            get;
            set;
        }

        public virtual string Status
        {
            get;
            set;
        }

        public virtual int OrderNumber
        {
            get;
            set;
        }

        public virtual string Type
        {
            get;
            set;
        }

        public virtual DateTime Raised
        {
            get;
            set;
        }

        public virtual User RaisedBy
        {
            get;
            set;
        }

        public virtual DateTime Authorised
        {
            get;
            set;
        }

        public virtual User AuthorisedBy
        {
            get;
            set;
        }

        public virtual string AuthorisedComment
        {
            get;
            set;
        }

        public virtual bool DoNotCheckForRenew
        {
            get;
            set;
        }
    }
}
