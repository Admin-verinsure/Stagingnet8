using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;
using Newtonsoft.Json;

namespace DealEngine.Domain.Entities
{
    public class ClientAgreementTerm : EntityBase, IAggregateRoot
    {
        public ClientAgreementTerm() : base (null) { }

        public ClientAgreementTerm(User createdBy, int termLimit, decimal excess, decimal premium, decimal fSL, decimal brokerageRate, decimal brokerage, ClientAgreement clientAgreement, string subTermType)
			: base (createdBy)
        {
            if (string.IsNullOrWhiteSpace(termLimit.ToString()))
                throw new ArgumentNullException(nameof(termLimit));
            if (string.IsNullOrWhiteSpace(excess.ToString()))
                throw new ArgumentNullException(nameof(excess));
            if (string.IsNullOrWhiteSpace(premium.ToString()))
                throw new ArgumentNullException(nameof(premium));
            if (string.IsNullOrWhiteSpace(fSL.ToString()))
                throw new ArgumentNullException(nameof(fSL));
            if (string.IsNullOrWhiteSpace(brokerageRate.ToString()))
                throw new ArgumentNullException(nameof(brokerageRate));
            if (string.IsNullOrWhiteSpace(brokerage.ToString()))
                throw new ArgumentNullException(nameof(brokerage));
            if (clientAgreement == null)
                throw new ArgumentNullException(nameof(clientAgreement));

            TermLimit = termLimit;
            Excess = excess;
            Premium = premium;
            FSL = fSL;
            BrokerageRate = brokerageRate;
            Brokerage = brokerage;
            ClientAgreement = clientAgreement;
            SubTermType = subTermType;
        }

        public virtual int TermLimit
        {
            get;
            set;
        }

        public virtual int AggregateLimit
        {
            get;
            set;
        }

        public virtual decimal Excess
        {
            get;
            set;
        }

        public virtual int HigherExcess
        {
            get;
            set;
        }

        public virtual decimal Premium
        {
            get;
            set;
        }

        public virtual string Reference
        {
            get;
            protected set;
        }

        public virtual bool DefaultTerm
        {
            get;
            set;
        }

        public virtual int OrderNumber
        {
            get;
            protected set;
        }

        public virtual bool Bound
        {
            get;
            set;
        }

        public virtual decimal BrokerageRate
        {
            get;
            set;
        }

        public virtual decimal Brokerage
        {
            get;
            set;
        }

        public virtual decimal NDBrokerageRate
        {
            get;
            set;
        }

        public virtual decimal NDBrokerage
        {
            get;
            set;
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
            set;
        }

        public virtual decimal FSL
        {
            get;
            set;
        }

        public virtual decimal EQC
        {
            get;
            set;
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

        public virtual string SubTermType
        {
            get;
            protected set;
        }

        public virtual int TermLimitPre
        {
            get;
            set;
        }

        public virtual int AggregateLimitPre
        {
            get;
            set;
        }

        public virtual decimal ExcessPre
        {
            get;
            set;
        }

        public virtual int HigherExcessPre
        {
            get;
            set;
        }

        public virtual decimal PremiumPre
        {
            get;
            set;
        }

        public virtual int TermLimitDiffer
        {
            get;
            set;
        }

        public virtual int AggregateLimitDiffer
        {
            get;
            set;
        }

        public virtual decimal ExcessDiffer
        {
            get;
            set;
        }

        public virtual int HigherExcessDiffer
        {
            get;
            set;
        }

        public virtual decimal PremiumDiffer
        {
            get;
            set;
        }

		public virtual IList<ClientAgreementMVTerm> MotorTerms
		{
			get;
			set;
		}

        public virtual IList<ClientAgreementBVTerm> BoatTerms
        {
            get;
            set;
        }

        public virtual decimal BurnerPremium {
			get;
			set;
		}

		public virtual decimal BurnerPremiumPre {
			get;
			set;
		}

		public virtual decimal BurnerPremiumDiffer {
			get;
			set;
		}

		public virtual decimal NDPre {
			get;
			set;
		}

		public virtual decimal FSLPre {
			get;
			set;
		}

		public virtual decimal EQCPre {
			get;
			set;
		}

		public virtual decimal NDDiffer {
			get;
			set;
		}

		public virtual decimal FSLDiffer {
			get;
			set;
		}

		public virtual decimal EQCDiffer {
			get;
			set;
		}

        public virtual string MergeCode
        {
            get;
            set;
        }

        public virtual string RiskCode
        {
            get;
            set;
        }

        public virtual string SubCoverString { get; set; }

        public virtual string AuthorisationNotes { get; set; }

        public virtual decimal BrokerageDiffer
        {
            get;
            set;
        }

        public virtual decimal BrokeragePre
        {
            get;
            set;
        }

        public virtual decimal NDBrokeragePre
        {
            get;
            set;
        }

        public virtual decimal NDBrokerageDiffer
        {
            get;
            set;
        }

        public virtual decimal BasePremium
        {
            get;
            set;
        }

        public virtual decimal FAPPremium
        {
            get;
            set;
        }

    }
}
