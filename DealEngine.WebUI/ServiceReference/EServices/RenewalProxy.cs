
namespace EServices.RenewalProxy
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="EditRenewalRequestTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class EditRenewalRequestTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<System.DateTime> EffectiveDateField;
        
        private System.Nullable<System.DateTime> ExpirationDateField;
        
        private System.Nullable<EServices.SubmissionProxy.LumleyBusinessUnit_LGI> LumleyBusinessUnitField;
        
        private EServices.SubmissionProxy.UnderwritingIssueTO ManualReferralReasonField;
        
        private string OfferingCodeField;
        
        private EServices.SubmissionProxy.PolicyLinesTO PolicyLinesField;
        
        private System.Nullable<EServices.SubmissionProxy.PolicyPackage_LGI> PolicyPackageField;
        
        private System.Nullable<EServices.SubmissionProxy.PolicyType_LGI> PolicyTypeField;
        
        private EServices.SubmissionProxy.PriorLossDetailsTO[] PriorLossesField;
        
        private string ProducerCodeField;
        
        private string ProducerSalesPersonUserNameField;
        
        private string RenewalNumberField;
        
        private System.Nullable<EServices.SubmissionProxy.TermType> TermTypeField;
        
        [System.Runtime.Serialization.IgnoreDataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> EffectiveDate
        {
            get
            {
                return this.EffectiveDateField;
            }
            set
            {
                this.EffectiveDateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> ExpirationDate
        {
            get
            {
                return this.ExpirationDateField;
            }
            set
            {
                this.ExpirationDateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.LumleyBusinessUnit_LGI> LumleyBusinessUnit
        {
            get
            {
                return this.LumleyBusinessUnitField;
            }
            set
            {
                this.LumleyBusinessUnitField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.UnderwritingIssueTO ManualReferralReason
        {
            get
            {
                return this.ManualReferralReasonField;
            }
            set
            {
                this.ManualReferralReasonField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OfferingCode
        {
            get
            {
                return this.OfferingCodeField;
            }
            set
            {
                this.OfferingCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.PolicyLinesTO PolicyLines
        {
            get
            {
                return this.PolicyLinesField;
            }
            set
            {
                this.PolicyLinesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.PolicyPackage_LGI> PolicyPackage
        {
            get
            {
                return this.PolicyPackageField;
            }
            set
            {
                this.PolicyPackageField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.PolicyType_LGI> PolicyType
        {
            get
            {
                return this.PolicyTypeField;
            }
            set
            {
                this.PolicyTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.PriorLossDetailsTO[] PriorLosses
        {
            get
            {
                return this.PriorLossesField;
            }
            set
            {
                this.PriorLossesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ProducerCode
        {
            get
            {
                return this.ProducerCodeField;
            }
            set
            {
                this.ProducerCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ProducerSalesPersonUserName
        {
            get
            {
                return this.ProducerSalesPersonUserNameField;
            }
            set
            {
                this.ProducerSalesPersonUserNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string RenewalNumber
        {
            get
            {
                return this.RenewalNumberField;
            }
            set
            {
                this.RenewalNumberField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.TermType> TermType
        {
            get
            {
                return this.TermTypeField;
            }
            set
            {
                this.TermTypeField = value;
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="IssueRenewalRequestTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class IssueRenewalRequestTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.ClosingSlipTO[] ClosingSlipsField;
        
        private string RenewalNumberField;
        
        [System.Runtime.Serialization.IgnoreDataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.ClosingSlipTO[] ClosingSlips
        {
            get
            {
                return this.ClosingSlipsField;
            }
            set
            {
                this.ClosingSlipsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string RenewalNumber
        {
            get
            {
                return this.RenewalNumberField;
            }
            set
            {
                this.RenewalNumberField = value;
            }
        }
    }
       
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="NotTakeRenewalRequestTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class NotTakeRenewalRequestTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string DescriptionField;
        
        private System.Nullable<EServices.RenewalProxy.OtherInsurer_LGI> NewInsurerField;
        
        private System.Nullable<EServices.RenewalProxy.NotTakenCode_LGI> NotTakenCodeField;
        
        private string RenewalNumberField;
        
        private System.Nullable<EServices.RenewalProxy.RenewalSource_LGI> RenewalSourceField;
        
        private System.Nullable<EServices.PolicyProxy.DeliveryMethod_LGI> RequestMethodField;
        
        private bool RequestMethodSpecifiedField;
        
        private string RequestedByField;
        
        [System.Runtime.Serialization.IgnoreDataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Description
        {
            get
            {
                return this.DescriptionField;
            }
            set
            {
                this.DescriptionField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.RenewalProxy.OtherInsurer_LGI> NewInsurer
        {
            get
            {
                return this.NewInsurerField;
            }
            set
            {
                this.NewInsurerField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.RenewalProxy.NotTakenCode_LGI> NotTakenCode
        {
            get
            {
                return this.NotTakenCodeField;
            }
            set
            {
                this.NotTakenCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string RenewalNumber
        {
            get
            {
                return this.RenewalNumberField;
            }
            set
            {
                this.RenewalNumberField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.RenewalProxy.RenewalSource_LGI> RenewalSource
        {
            get
            {
                return this.RenewalSourceField;
            }
            set
            {
                this.RenewalSourceField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.PolicyProxy.DeliveryMethod_LGI> RequestMethod
        {
            get
            {
                return this.RequestMethodField;
            }
            set
            {
                this.RequestMethodField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool RequestMethodSpecified
        {
            get
            {
                return this.RequestMethodSpecifiedField;
            }
            set
            {
                this.RequestMethodSpecifiedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string RequestedBy
        {
            get
            {
                return this.RequestedByField;
            }
            set
            {
                this.RequestedByField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="OtherInsurer_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum OtherInsurer_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_aa = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ace = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_aigamericanhomeassuranceco = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_allianz = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_amiinsurance = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_amp = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_anz = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_asb = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bnz = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_chartis = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_china = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_civicassurance = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_clubauto = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_combinedinsurance = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_farmersmutualgroup = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fintel = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hsbc = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_iag = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_kiwibank = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lloyds = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lumleybusinesssolutions = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lginzltd = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_medicalassurance = 22,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mitsuisumitomo = 23,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_motorandgeneral = 24,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nationalbank = 25,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nationaltransportinsurance = 26,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_newindia = 27,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_none = 28,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nzinsurance = 29,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_other = 30,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_overseasother = 31,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_overseas_sa_uk_or_au = 32,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_psis = 33,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_qbeinsuranceintlltd = 34,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sisinsurance = 35,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_stateinsuranceltd = 36,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sunderlandmarine = 37,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sundirect = 38,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tower = 39,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_unknown = 40,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_vero = 41,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_westernpacific = 42,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_westpac = 43,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_zurich = 44,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="NotTakenCode_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum NotTakenCode_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_changedEmployment_lgi = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_deceased_lgi = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dissatisfiedWithClaims_lgi = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dissatisfiedWithSvc_lgi = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dualInsurance_lgi = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_goneOverseas_lgi = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_insuredElsewhere_lgi = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_inviteexpired_lgi = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_noLongerRequired_lgi = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nonpayment = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_policyReplaced_lgi = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sold_lgi = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tooDear_lgi = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_totalLoss_lgi = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_uwreasons = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_unknown_lgi = 15,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="RenewalSource_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum RenewalSource_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_insuredCommercial = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_insuredDomestic = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lumleyCommercial = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lumleyDomestic = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_producerCommercial = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_producerDomestic = 5,
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SOAPException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.RenewalProxy.SOAPServerException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.RenewalProxy.ServerStateException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.RenewalProxy.ValidationException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.RenewalProxy.SOAPSenderException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.RenewalProxy.PermissionException))]
    public partial class SOAPException : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string MessageField;
        
        private string OtherMessagesField;
        
        [System.Runtime.Serialization.IgnoreDataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Message
        {
            get
            {
                return this.MessageField;
            }
            set
            {
                this.MessageField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OtherMessages
        {
            get
            {
                return this.OtherMessagesField;
            }
            set
            {
                this.OtherMessagesField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SOAPServerException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.RenewalProxy.ServerStateException))]
    public partial class SOAPServerException : EServices.RenewalProxy.SOAPException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    public partial class ServerStateException : EServices.RenewalProxy.SOAPServerException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    public partial class ValidationException : EServices.RenewalProxy.SOAPException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SOAPSenderException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.RenewalProxy.PermissionException))]
    public partial class SOAPSenderException : EServices.RenewalProxy.SOAPException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    public partial class PermissionException : EServices.RenewalProxy.SOAPSenderException
    {
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="urn:nz.co.lumley:eservices:20131111", ConfigurationName="EServices.RenewalProxy.IRenewalService")]
    public interface IRenewalService
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/IRenewalService/EditRenewal", ReplyAction="urn:nz.co.lumley:eservices:20131111/IRenewalService/EditRenewalResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.RenewalProxy.PermissionException), Action="", Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.RenewalProxy.ServerStateException), Action="", Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.RenewalProxy.ValidationException), Action="", Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        void EditRenewal(EServices.RenewalProxy.EditRenewalRequestTO editRenewalRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/IRenewalService/EditRenewal", ReplyAction="urn:nz.co.lumley:eservices:20131111/IRenewalService/EditRenewalResponse")]
        System.Threading.Tasks.Task EditRenewalAsync(EServices.RenewalProxy.EditRenewalRequestTO editRenewalRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/IRenewalService/IssueRenewal", ReplyAction="urn:nz.co.lumley:eservices:20131111/IRenewalService/IssueRenewalResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.RenewalProxy.ServerStateException), Action="", Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.RenewalProxy.PermissionException), Action="", Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.RenewalProxy.ValidationException), Action="", Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        void IssueRenewal(EServices.RenewalProxy.IssueRenewalRequestTO issueRenewalRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/IRenewalService/IssueRenewal", ReplyAction="urn:nz.co.lumley:eservices:20131111/IRenewalService/IssueRenewalResponse")]
        System.Threading.Tasks.Task IssueRenewalAsync(EServices.RenewalProxy.IssueRenewalRequestTO issueRenewalRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/IRenewalService/NotTakeRenewal", ReplyAction="urn:nz.co.lumley:eservices:20131111/IRenewalService/NotTakeRenewalResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.RenewalProxy.ServerStateException), Action="", Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.RenewalProxy.PermissionException), Action="", Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.RenewalProxy.ValidationException), Action="", Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        void NotTakeRenewal(EServices.RenewalProxy.NotTakeRenewalRequestTO notTakeRenewalRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/IRenewalService/NotTakeRenewal", ReplyAction="urn:nz.co.lumley:eservices:20131111/IRenewalService/NotTakeRenewalResponse")]
        System.Threading.Tasks.Task NotTakeRenewalAsync(EServices.RenewalProxy.NotTakeRenewalRequestTO notTakeRenewalRequest);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IRenewalServiceChannel : EServices.RenewalProxy.IRenewalService, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class RenewalServiceClient : System.ServiceModel.ClientBase<EServices.RenewalProxy.IRenewalService>, EServices.RenewalProxy.IRenewalService
    {
        
        public RenewalServiceClient()
        {
        }
        
        public RenewalServiceClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
        {
        }
        
        public RenewalServiceClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public RenewalServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public RenewalServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
        {
        }
        
        public void EditRenewal(EServices.RenewalProxy.EditRenewalRequestTO editRenewalRequest)
        {
            base.Channel.EditRenewal(editRenewalRequest);
        }
        
        public System.Threading.Tasks.Task EditRenewalAsync(EServices.RenewalProxy.EditRenewalRequestTO editRenewalRequest)
        {
            return base.Channel.EditRenewalAsync(editRenewalRequest);
        }
        
        public void IssueRenewal(EServices.RenewalProxy.IssueRenewalRequestTO issueRenewalRequest)
        {
            base.Channel.IssueRenewal(issueRenewalRequest);
        }
        
        public System.Threading.Tasks.Task IssueRenewalAsync(EServices.RenewalProxy.IssueRenewalRequestTO issueRenewalRequest)
        {
            return base.Channel.IssueRenewalAsync(issueRenewalRequest);
        }
        
        public void NotTakeRenewal(EServices.RenewalProxy.NotTakeRenewalRequestTO notTakeRenewalRequest)
        {
            base.Channel.NotTakeRenewal(notTakeRenewalRequest);
        }
        
        public System.Threading.Tasks.Task NotTakeRenewalAsync(EServices.RenewalProxy.NotTakeRenewalRequestTO notTakeRenewalRequest)
        {
            return base.Channel.NotTakeRenewalAsync(notTakeRenewalRequest);
        }
    }
}