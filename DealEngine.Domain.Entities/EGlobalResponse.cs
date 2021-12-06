using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class EGlobalResponse : EntityBase, IAggregateRoot
    {
        protected EGlobalResponse() : base(null) { }

        public EGlobalResponse(User createdBy)
            : base(createdBy)
        {

        }

        public virtual string ResponseXML { get; set; }
        public virtual ClientProgramme EGlobalSubmissionClientProgramme { get; set; }
        public virtual EGlobalSubmission EGlobalSubmission { get; set; }
        public virtual string MasterAgreementReferenceID { get; set; }
        public virtual string ResponseType { get; set; }
        public virtual string ExtSysKey { get; set; }
        public virtual string ExtSysRef { get; set; }
        public virtual string Key { get; set; }
        public virtual string TranCode { get; set; }
        public virtual string Code { get; set; }
        public virtual string Description { get; set; }
        public virtual string ExtSysInput { get; set; }
        public virtual string ResponseText { get; set; }
        public virtual string Company { get; set; }
        public virtual string Branch { get; set; }
        public virtual string ClientNumber { get; set; }
        public virtual int CoverNumber { get; set; }
        public virtual int VersionNumber { get; set; }
        public virtual string Tranident { get; set; }
        public virtual int InvoiceNumber { get; set; }


    }
}

