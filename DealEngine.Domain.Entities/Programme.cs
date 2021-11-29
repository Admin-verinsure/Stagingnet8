using System;
using System.Collections.Generic;
using System.Linq;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{    
    public class Programme : EntityBase, IAggregateRoot
    {
        public virtual string Name { get; set; }
        public virtual Organisation Owner { get; protected set; }
        public virtual IList<Product> Products { get; protected set; }
        public virtual IList<TerritoryTemplate> TerritoryTemplates { get; set; }
        public virtual IList<BusinessActivityTemplate> BusinessActivityTemplates { get; set; }
        public virtual IList<SharedDataRoleTemplate> SharedDataRoleTemplates { get; set; }
        public virtual string Declaration { get; set; }
        public virtual IList<EmailTemplate> EmailTemplates { get; protected set; }
        public virtual IList<ClientProgramme> ClientProgrammes { get; protected set; }
        public virtual IList<Merchant> Merchants { get; protected set; }
        public virtual User BrokerContactUser { get; set; }
        public virtual bool IsPublic { get; set; }
        public virtual IList<Organisation> Parties { get; set; }
        public virtual IList<User> UISIssueNotifyUsers { get; set; }
        public virtual IList<UpdateType> UpdateTypes { get; set; }

        public virtual IList<User> UISSubmissionNotifyUsers { get; set; }
        public virtual IList<User> AgreementReferNotifyUsers { get; set; }
        public virtual IList<User> AgreementIssueNotifyUsers { get; set; }
        public virtual IList<User> AgreementBoundNotifyUsers { get; set; }
        public virtual IList<User> PaymentConfigNotifyUsers { get; set; }
        public virtual IList<User> InvoiceConfigNotifyUsers { get; set; }
        public virtual IList<User> RemoveAdvisorNotifyUsers { get; set; }
        public virtual IList<User> UISUpdateNotifyUsers { get; set; }
        public virtual IList<User> ClientNumberNotifyUsers { get; set; }
        public virtual bool StopAgreement { get; set; }
        public virtual bool StopDeclaration { get; set; }
        public virtual DateTime StopAgreementDateTime { get; set; }
        public virtual string StopAgreementMessage { get; set; }
        public virtual string SubsystemDeclaration { get; set; }
        public virtual string SubsystemStopAgreementMessage { get; set; }
        public virtual string NoPaymentRequiredMessage { get; set; }
        public virtual bool AllowUsesChange { get; set; }
        public virtual bool CalculateCancelTerm { get; set; }
        public virtual bool HasSubsystemEnabled { get; set; }
        public virtual bool HasCCPayment { get; set; }
        public virtual bool HasPFPayment { get; set; }
        public virtual bool HasInvoicePayment { get; set; }
        public virtual decimal TaxRate { get; set; }
        public virtual decimal SurchargeRate { get; set; }
        public virtual bool ProgrammeEmailCCToBroker { get; set; }

        public virtual IList<Package> Packages { get; protected set; }

        public virtual bool UsesEGlobal { get; set; }

        public virtual string PolicyNumberPrefixString { get; set; }
        public virtual string Claim { get; set; }
        public virtual bool ProgEnableEmail { get; set; }
        public virtual bool ProgEnableInsuredDateChange { get; set; }
        public virtual bool ProgHideAdminFee { get; set; }
        public virtual bool ProgStopPolicyDocAutoRelease { get; set; }
        public virtual bool ProgHidePremium { get; set; }
        public virtual bool ProgEnableHidedoctoClient { get; set; }
        public virtual bool ProgEnableSendPremiumAdvice { get; set; }
        public virtual string PremiumAdviceRecipent { get; set; }
        public virtual string PremiumAdviceRecipentCC { get; set; }
        public virtual bool EnableFullProposalReport{ get; set; }
        public virtual string FullProposalReportRecipent { get; set; }
        public virtual bool EnableMonthlyPremiumDisplay { get; set; }
        public virtual int MonthlyInstalmentNumber { get; set; }
        public virtual bool EnablePIReport { get; set; }
        public virtual bool EnableEDReport { get; set; }
        public virtual bool EnableCLReport { get; set; }
        public virtual bool EnableCyberReport { get; set; }
        public virtual bool EnableFAPReport { get; set; }
        public virtual bool EnableRevenueActivity { get; set; }
        public virtual string NamedPartyUnitName { get; set; }
        public virtual Programme RenewFromProgramme { get; set; }
        public virtual bool RenewWithOutRevenue { get; set; }
        public virtual bool DisplayGSTInclusive { get; set; }
        public virtual bool DisplayGSTExclusive { get; set; }
        public virtual bool ProgHidePlacementFee { get; set; }
        public virtual bool ProgHideAdditionalCertFee { get; set; }
        public virtual int RenewGracePriodInDays { get; set; }
        public virtual int NewGracePriodInDays { get; set; }
        public virtual bool ProgEnableProgEmailCC { get; set; }
        public virtual string ProgEmailCCRecipent { get; set; }
        public virtual bool IsPdfDoc{ get; set; }
        public virtual bool ProgEnableRequireNoCover { get; set; }
        public virtual bool IsFAPOrg { get; set; }
        public virtual string ProgMergeClassOfInsurance { get; set; }
        public virtual string ProgMergeInsurer { get; set; }
        public virtual string ProgMergeInsurerRating { get; set; }
        public virtual string ProgMergePolicyNumber { get; set; }
        public virtual bool IslastFinancialYear { get; set; }
        public virtual bool IsCurrentYear { get; set; }
        public virtual bool IsnextFinancialYear { get; set; }
        protected Programme() : this(null) { }

        public Programme(User createdBy) : base(createdBy)
        {
            SharedDataRoleTemplates = new List<SharedDataRoleTemplate>();
            BusinessActivityTemplates = new List<BusinessActivityTemplate>();
            TerritoryTemplates = new List<TerritoryTemplate>();
            Products = new List<Product>();            
            EmailTemplates = new List<EmailTemplate>();
            ClientProgrammes = new List<ClientProgramme>();
            Merchants = new List<Merchant>();
            Parties = new List<Organisation>();
            UISIssueNotifyUsers = new List<User>();
            UpdateTypes = new List<UpdateType>();
            UISSubmissionNotifyUsers = new List<User>();
            AgreementReferNotifyUsers = new List<User>();
            AgreementIssueNotifyUsers = new List<User>();
            AgreementBoundNotifyUsers = new List<User>();
            PaymentConfigNotifyUsers = new List<User>();
            InvoiceConfigNotifyUsers = new List<User>();
            RemoveAdvisorNotifyUsers = new List<User>();
            Packages = new List<Package>();
            UISUpdateNotifyUsers = new List<User>();
            ClientNumberNotifyUsers = new List<User>();
        }

        public virtual ClientProgramme IssueFor(Organisation clientOrganisation)
        {
            return new ClientProgramme(CreatedBy, clientOrganisation, this);
        }

        public virtual IEnumerable<Document> GetProductDocuments()
        {
            return Products.SelectMany((arg) => arg.Documents);
        }

        public virtual void LastModified(User user)
        {
            LastModifiedBy = user;
            LastModifiedOn = DateTime.Now;
        }
    }

}

