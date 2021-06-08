using System;
using System.Collections.Generic;
using System.Linq;
using DealEngine.Domain.Entities.Abstracts;
using Newtonsoft.Json;

namespace DealEngine.Domain.Entities
{
	public class ClientAgreement : EntityBase, IAggregateRoot
	{
		protected ClientAgreement () : this (null) { }

        protected ClientAgreement (User createdBy)
			: base (createdBy)
        {
            ClientAgreementTerms = new List<ClientAgreementTerm>();
            ClientAgreementRules = new List<ClientAgreementRule>();
            ClientAgreementEndorsements = new List<ClientAgreementEndorsement>();
            Documents = new List<Document> ();
            ClientAgreementAuditLogs = new List<AuditLog>();
            ClientAgreementReferrals = new List<ClientAgreementReferral>();
            ClientAgreementTermsCancel = new List<ClientAgreementTermCancel>();
        }

		public ClientAgreement(User createdBy, string insuredName, DateTime inceptionDate, DateTime expiryDate, decimal brokerage, decimal brokerFee, ClientInformationSheet clientInformationSheet, Product product, string reference)
            : this (createdBy)
        {
            if (string.IsNullOrWhiteSpace(insuredName))
				throw new ArgumentNullException(nameof (insuredName));
            if (clientInformationSheet == null)
				throw new ArgumentNullException(nameof (clientInformationSheet));
			if (product == null)
				throw new ArgumentNullException (nameof (product));

            InsuredName = insuredName;
            InceptionDate = inceptionDate;
            ExpiryDate = expiryDate;
            Brokerage = brokerage;
            BrokerFee = brokerFee;
            ClientInformationSheet = clientInformationSheet;
			Product = product;
            ReferenceId = reference;

        }
        public virtual IList<ClientAgreementTerm> ClientAgreementTerms { get; set; }
        [JsonIgnore]
        public virtual IList<ClientAgreementRule> ClientAgreementRules { get; protected set; }
        [JsonIgnore]
        public virtual IList<ClientAgreementReferral> ClientAgreementReferrals { get; protected set; }
        [JsonIgnore]
        public virtual IList<AuditLog> ClientAgreementAuditLogs { get; protected set; }
        [JsonIgnore]
        public virtual IList<ClientAgreementEndorsement> ClientAgreementEndorsements { get; protected set; }
        public virtual string InsuredName { get;  set; }
        public virtual bool CustomInceptionDate { get; set; }
        public virtual string AgreementReference { get; set; }
        public virtual string ReferenceId { get; set; }
        public virtual string Content { get; set; }
        public virtual DateTime InceptionDate { get; set; }
        public virtual DateTime ExpiryDate { get; set; }
        public virtual DateTime QuoteDate { get; set; }
        public virtual bool SentOnlineAcceptance { get; set; }
        public virtual decimal Brokerage { get; set; }
        public virtual decimal BrokerFee { get; set; }
        public virtual string InsurerQuoteIssueComment { get; protected set; }
        public virtual string BreachProfDuty { get; protected set; }
        public virtual bool EnableRetroactiveDate { get; set; }
        public virtual string RetroactiveDate { get; set; }
        public virtual bool Bound { get; protected set; }
        public virtual DateTime BoundDate { get; set; }
        public virtual DateTime FinalacceptanceDate { get;  set; }
        public virtual DateTime ClientAgreementExpiredDate { get; set; }
        public virtual bool InsurerDeclined { get; set; }
        public virtual string InsurerDeclinedComment { get;  set; }
        public virtual User InsurerDeclinedUserID { get; set; }
        public virtual bool InsuredDeclined { get; set; }
        public virtual string InsuredDeclinedComment { get; set; }
        public virtual DateTime InsurerDeclinedDate { get; set; }
        public virtual User UndeclinedUserID { get; set; }
        public virtual DateTime UndeclinedDate { get; set; }
        public virtual bool Cancelled { get; set; }
        public virtual string CancelledNote { get; set; }
        public virtual DateTime CancelledEffectiveDate { get; set; }
        public virtual User CancelledByUserID { get; set; }
        public virtual string CancelAgreementReason { get; set; }
        public virtual string PolicyNumber { get; set; }
        public virtual int ReferenceNumber { get; protected set; }
        public virtual Product Product { get; protected set; }
        public virtual AgreementTemplate AgreementTemplate { get; protected set; }
        [JsonIgnore]
        public virtual ClientInformationSheet ClientInformationSheet { get; protected set; }
        [JsonIgnore]
		public virtual IList<Document> Documents { get; protected set; }
        public virtual string ClientNumber { get; set; }
        public virtual string Jurisdiction { get; set; }
        public virtual string TerritoryLimit { get; set; }
        public virtual string ProfessionalBusiness { get; set; }

        //Quoted; Referred; Bound and pending payment; Bound and invoice pending; Bound and invoiced; Bound; Declined by Insurer; Declined by Insured; Cancelled; Cancel Pending
        public virtual string Status { get; set; }
        public virtual bool ReferToTC { get; set; }
        public virtual DateTime CancelledDate { get; set; }
        public virtual bool MasterAgreement { get; set; }
        public virtual ClientAgreement PreviousAgreement { get; set; }
        [JsonIgnore]
        public virtual IList<ClientAgreementTermCancel> ClientAgreementTermsCancel { get; set; }
        public virtual string issuetobrokercomment { get; set; }
        public virtual DateTime IssuedToBroker { get; set; }
        public virtual string issuetobrokerby { get; set; }
        public virtual string issuetobrokerto { get; set; }
        public virtual User SelectedBroker { get; set; }
        public virtual bool IsUnbind { get; set; }
        public virtual string UnbindNotes { get; set; }
        public virtual DateTime UnbindEffectiveDate { get; set; }
        public virtual User UnbindByUserID { get; set; }
        public virtual bool IsPolicyDocSend { get; set; }
        public virtual DateTime DocIssueDate { get; set; }
        public virtual string BindNotes { get; set; }
        public virtual DateTime BindEffectiveDate { get; set; }
        public virtual User BindByUserID { get; set; }
        public virtual bool IsPDFgenerated { get; set; }
        public virtual bool IsFullProposalDocSend { get; set; }
        public virtual decimal PlacementFee { get; set; }
        public virtual decimal AdditionalCertFee { get; set; }
        public virtual List<Document> GetDocuments()
        {
            return Documents.Where(d => d.DateDeleted == null).ToList();                        
        }
    }
}

