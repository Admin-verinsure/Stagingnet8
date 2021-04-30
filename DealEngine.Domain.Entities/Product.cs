using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities.Abstracts;
using Newtonsoft.Json;

namespace DealEngine.Domain.Entities
{
    public class Product : EntityBase, IAggregateRoot
    {
        public virtual Guid OriginalProductId { get; set; }
        public virtual Guid CreatorCompany { get; set; }
        public virtual Guid OwnerCompany { get; set; }
        public virtual Guid InvoiceId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual IList<string> Languages { get; set; }
        public virtual IList<RiskCover> RiskCategoriesCovered { get; set; }
        //[Obsolete ("No longer required with the new Programme implementation")]
        public virtual InformationTemplate InformationTemplate { get; set; }
        public virtual InformationTemplate SubInformationTemplate { get; set; }
        public virtual IList<InformationSection> SharedViews { get; set; }
        public virtual IList<InformationSection> UniqueQuestions { get; set; }
        public virtual IList<Document> Documents { get; set; }
        public virtual bool Public { get; set; }
        public virtual bool IsBaseProduct { get; set; }
        public virtual string ProductNotes { get; set; }
        public virtual decimal DefaultBrokerage { get; set; }
        public virtual bool Published { get; set; }
        public virtual bool Active { get; set; }        
        public virtual bool DefaultEnableBrokerFee { get; set; }
        public virtual decimal DefaultBrokerFee { get; set; }
        public virtual decimal TaxRate { get; set; }
        public virtual int OrderNumber { get; set; }
        public virtual string ProductClassOfBusinessAlias { get; set; }
        public virtual IList<Rule> Rules { get; protected set; }
        public virtual IList<Endorsement> Endorsements { get; protected set; }
        public virtual DateTime DefaultInceptionDate { get; set; }
        public virtual DateTime DefaultExpiryDate { get; set; }
        public virtual IList<EmailTemplate> EmailTemplates { get; protected set; }
        public virtual IList<Product> ChildProducts { get; set; }

        [Obsolete("No longer required with the new Programme implementation")]
        public virtual ProductPackage ProductPackage { get; set; }
        public virtual string UnderwritingModuleCode { get; set; }
        public virtual bool UnderwritingEnabled { get; set; }
        public virtual bool IsMasterProduct { get; set; }
        public virtual bool IsMultipleOption { get; set; }
        public virtual bool IsOptionalProduct { get; set; }
        public virtual string OptionalProductRequiredAnswer { get; set; }
        public virtual Product DependableProduct { get; set; }
        public virtual string WordingDownloadURL { get; set; }
        public virtual bool IsOptionalProductBasedSub { get; set; }
        public virtual bool IsOptionalCombinedProduct { get; set; }
        public virtual bool ProductEnablePremiumAdvice { get; set; }
        public virtual string WordingDownloadURLAlternative { get; set; }
        public virtual decimal DefaultPlacementFee { get; set; }
        public virtual decimal DefaultAdditionalCertFee { get; set; }

        protected Product() : base(null) { }

        protected Product(User createdBy)
            : base(createdBy)
        {
            Languages = new List<string>();
            RiskCategoriesCovered = new List<RiskCover>();
            Rules = new List<Rule>();
            EmailTemplates = new List<EmailTemplate>();
            Public = false;
            IsMultipleOption = false;
            IsOptionalProduct = false;
        }

        public Product(User createdBy, Guid creatorCompany, string name)
            : this(createdBy)
        {
            if (creatorCompany == Guid.Empty)
                throw new ArgumentNullException(nameof(creatorCompany));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            CreatorCompany = creatorCompany;
        }

        public virtual void MakePublic()
        {
            Public = true;
        }
    }

    //public class SubProduct : Product
    //{
    //    public virtual Product BaseProduct { get; set; }
    //    public SubProduct() { }
    //}
}