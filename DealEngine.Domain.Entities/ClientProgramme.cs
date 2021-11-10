using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Configuration.Annotations;
using DealEngine.Domain.Entities.Abstracts;
using Newtonsoft.Json;

namespace DealEngine.Domain.Entities
{
    public class ClientProgramme : EntityBase, IAggregateRoot
    {
        public virtual Organisation Owner { get; set; }
        [JsonIgnore]
        public virtual Programme BaseProgramme { get; set; }
        [JsonIgnore]
        public virtual ClientInformationSheet InformationSheet { get; set; } //ignore for edit client page
        [JsonIgnore]
        public virtual Payment Payment { get; set; }
        [JsonIgnore]
        public virtual User BrokerContactUser { get; set; } //ignore for edit client page
        public virtual ChangeReason ChangeReason { get; set; }
        public virtual DateTime IssueDate { get; set; }
        public virtual DateTime ReminderDate { get; set; }
        public virtual DateTime RenewNotificationDate { get; set; }
        [JsonIgnore]
        public virtual IDictionary<Product, bool> Products { get; set; } //ignore for edit client page     
        [JsonIgnore]
        public virtual IList<ClientAgreement> Agreements { get; set; }
        public virtual IList<EGlobalSubmission> ClientAgreementEGlobalSubmissions { get; set; }
        public virtual IList<EGlobalResponse> ClientAgreementEGlobalResponses { get; set; }
        public virtual IList<SubClientProgramme> SubClientProgrammes { get; set; }       
        public virtual bool HasEGlobalCustomDescription { get; set; }
        public virtual string PaymentType { get; set; }
        public virtual string PaymentFrequency { get; set; }
        public virtual string EGlobalBranchCode { get; set; }
        public virtual string EGlobalClientNumber { get; set; }
        public virtual string EGlobalClientStatus { get; set; }
        public virtual string EGlobalCustomDescription { get; set; }
        public virtual string ClientProgrammeMembershipNumber { get; set; }
        public virtual string Tier { get; set; }
        public virtual bool IsDocsApproved { get; set; }
        public virtual ClientProgramme RenewFromClientProgramme { get; set; }
        protected ClientProgramme() : this(null, null, null) { }
        public virtual string EGlobalExternalContactNumber { get; set; }
        public ClientProgramme (User createdBy, Organisation createdFor, Programme baseProgramme)
			: base(createdBy)
		{
            //BrokerContactUser = baseProgramme.BrokerContactUser;
			Owner = createdFor;
			BaseProgramme = baseProgramme;
			Agreements = new List<ClientAgreement> ();
            ClientAgreementEGlobalSubmissions = new List<EGlobalSubmission>();
            ClientAgreementEGlobalResponses = new List<EGlobalResponse>();
            SubClientProgrammes = new List<SubClientProgramme>();
        }

        public virtual IEnumerable<Product> GetSelectedProducts ()
		{
			return Products.Where((arg) => arg.Value).Select((arg) => arg.Key);
			// because I can't quite figure out the Linq for this, so  use this instead
			//return from product in Products where product.Value == true select product.Key;
		}
	}

    public class SubClientProgramme : ClientProgramme
    {
        public virtual ClientProgramme BaseClientProgramme { get; set; }
        public SubClientProgramme() { }       
    }
}

