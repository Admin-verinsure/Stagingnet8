using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;
using Newtonsoft.Json;

namespace DealEngine.Domain.Entities
{
    public class ClientAgreementTermExtension : EntityBase, IAggregateRoot
    {
        public ClientAgreementTermExtension() : base (null) { }

        public ClientAgreementTermExtension(User createdBy, int termLimit, decimal excess, decimal premium, ClientAgreement clientAgreement)
			: base (createdBy)
        {
            if (string.IsNullOrWhiteSpace(termLimit.ToString()))
                throw new ArgumentNullException(nameof(termLimit));
            if (string.IsNullOrWhiteSpace(excess.ToString()))
                throw new ArgumentNullException(nameof(excess));
            if (string.IsNullOrWhiteSpace(premium.ToString()))
                throw new ArgumentNullException(nameof(premium));
            if (clientAgreement == null)
                throw new ArgumentNullException(nameof(clientAgreement));

            TermLimit = termLimit;
            Excess = excess;
            Premium = premium;
            
        }

        public virtual int TermLimit
        {
            get;
            set;
        }
        public virtual bool Bound
        {
            get;
            set;
        }

        public virtual decimal Excess
        {
            get;
            set;
        }

      
        public virtual decimal Premium
        {
            get;
            set;
        }

        public virtual ClientAgreement ClientAgreement
        {
            get;
            protected set;
        }
        public virtual string ExtentionName
        {
            get;
            set;
        }
    }
}
