
namespace EServices.SubmissionProxy
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="QuoteSubmissionRequestTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class QuoteSubmissionRequestTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.AccountReferenceTO AccountField;
        
        private System.Nullable<System.DateTime> DateQuoteNeededField;
        
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
        
        private EServices.SubmissionProxy.ProductSelectionTO ProductSelectionField;
        
        private System.Nullable<EServices.SubmissionProxy.QuoteType> QuoteTypeField;
        
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
        public EServices.SubmissionProxy.AccountReferenceTO Account
        {
            get
            {
                return this.AccountField;
            }
            set
            {
                this.AccountField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> DateQuoteNeeded
        {
            get
            {
                return this.DateQuoteNeededField;
            }
            set
            {
                this.DateQuoteNeededField = value;
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
        public EServices.SubmissionProxy.ProductSelectionTO ProductSelection
        {
            get
            {
                return this.ProductSelectionField;
            }
            set
            {
                this.ProductSelectionField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.QuoteType> QuoteType
        {
            get
            {
                return this.QuoteTypeField;
            }
            set
            {
                this.QuoteTypeField = value;
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
    [System.Runtime.Serialization.DataContractAttribute(Name="AccountReferenceTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class AccountReferenceTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string ExternalAccountIDField;
        
        private string LumleyAccountNumberField;
        
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
        public string ExternalAccountID
        {
            get
            {
                return this.ExternalAccountIDField;
            }
            set
            {
                this.ExternalAccountIDField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LumleyAccountNumber
        {
            get
            {
                return this.LumleyAccountNumberField;
            }
            set
            {
                this.LumleyAccountNumberField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="UnderwritingIssueTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class UnderwritingIssueTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string NameField;
        
        private string ValueField;
        
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
        public string Name
        {
            get
            {
                return this.NameField;
            }
            set
            {
                this.NameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Value
        {
            get
            {
                return this.ValueField;
            }
            set
            {
                this.ValueField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PolicyLinesTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class PolicyLinesTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CommercialMotorVehicleLineTO CommercialMotorVehicleLineField;
        
        private EServices.SubmissionProxy.CommercialPropertyLineTO CommercialPropertyLineField;
        
        private EServices.SubmissionProxy.LiabilityLineTO LiabilityLineField;
        
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
        public EServices.SubmissionProxy.CommercialMotorVehicleLineTO CommercialMotorVehicleLine
        {
            get
            {
                return this.CommercialMotorVehicleLineField;
            }
            set
            {
                this.CommercialMotorVehicleLineField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialPropertyLineTO CommercialPropertyLine
        {
            get
            {
                return this.CommercialPropertyLineField;
            }
            set
            {
                this.CommercialPropertyLineField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.LiabilityLineTO LiabilityLine
        {
            get
            {
                return this.LiabilityLineField;
            }
            set
            {
                this.LiabilityLineField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ProductSelectionTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class ProductSelectionTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<int> NumToCreateField;
        
        private string ProductCodeField;
        
        private string ProductSelectionStatusField;
        
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
        public System.Nullable<int> NumToCreate
        {
            get
            {
                return this.NumToCreateField;
            }
            set
            {
                this.NumToCreateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ProductCode
        {
            get
            {
                return this.ProductCodeField;
            }
            set
            {
                this.ProductCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ProductSelectionStatus
        {
            get
            {
                return this.ProductSelectionStatusField;
            }
            set
            {
                this.ProductSelectionStatusField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LumleyBusinessUnit_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum LumleyBusinessUnit_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_auckland = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_christchurch = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_constengunderwriting = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_brokercorporate = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dunedin = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hamilton = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_headoffice = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lbsbrokerpersonallines = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lbsbusinesspartners = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lbsemployeeinsurancescheme = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lbswestpac = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_liabunderwriting = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_marineunderwriting = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_newplymouth = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_palmerstonnorth = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_smeschemeunderwriting = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tauranga_lgi = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_wellington = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_brokersme = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lbscorporatepartners = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lbsspecialist = 20,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PolicyPackage_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum PolicyPackage_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bulk = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_managementshield = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_priorperiodlaserpolicy = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ruralpak = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_standard = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_toppak = 5,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PolicyType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum PolicyType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_light = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_special = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_standard = 2,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PriorLossDetailsTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class PriorLossDetailsTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private string DescriptionField;
        
        private System.Nullable<System.DateTime> OccurrenceDateField;
        
        private System.Nullable<EServices.SubmissionProxy.PolicyLine> PolicyLineField;
        
        private System.Nullable<decimal> TotalIncurredField;
        
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
        public System.Nullable<System.DateTime> OccurrenceDate
        {
            get
            {
                return this.OccurrenceDateField;
            }
            set
            {
                this.OccurrenceDateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.PolicyLine> PolicyLine
        {
            get
            {
                return this.PolicyLineField;
            }
            set
            {
                this.PolicyLineField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> TotalIncurred
        {
            get
            {
                return this.TotalIncurredField;
            }
            set
            {
                this.TotalIncurredField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="QuoteType", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum QuoteType : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Full = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Quick = 1,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TermType", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum TermType : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Annual = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Annual_LGI = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Periodic = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Other = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_HalfYear = 4,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialMotorVehicleLineTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialMotorVehicleLineTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CommercialVehicleTO[] CommercialVehiclesField;
        
        private EServices.SubmissionProxy.CommercialMotorVehicleLineLevelModifierTO LineLevelModifierField;
        
        private System.Nullable<EServices.SubmissionProxy.CMVPolicyType_LGI> PolicyTypeField;
        
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
        public EServices.SubmissionProxy.CommercialVehicleTO[] CommercialVehicles
        {
            get
            {
                return this.CommercialVehiclesField;
            }
            set
            {
                this.CommercialVehiclesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialMotorVehicleLineLevelModifierTO LineLevelModifier
        {
            get
            {
                return this.LineLevelModifierField;
            }
            set
            {
                this.LineLevelModifierField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CMVPolicyType_LGI> PolicyType
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
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyLineTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyLineTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CommercialPropertyMDBITO CPMDBIField;
        
        private EServices.SubmissionProxy.CommercialPropertyRiskHistoryTO CPMDBIRiskHistoryField;
        
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
        public EServices.SubmissionProxy.CommercialPropertyMDBITO CPMDBI
        {
            get
            {
                return this.CPMDBIField;
            }
            set
            {
                this.CPMDBIField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialPropertyRiskHistoryTO CPMDBIRiskHistory
        {
            get
            {
                return this.CPMDBIRiskHistoryField;
            }
            set
            {
                this.CPMDBIRiskHistoryField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LiabilityLineTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class LiabilityLineTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.EmployersLiabilityTO EmployersLiabilityField;
        
        private EServices.SubmissionProxy.GeneralCoverDetailsTO GeneralCoverDetailsField;
        
        private EServices.SubmissionProxy.GeneralLiabilityTO GeneralLiabilityField;
        
        private EServices.SubmissionProxy.StatutoryLiabilityTO StatutoryLiabilityField;
        
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
        public EServices.SubmissionProxy.EmployersLiabilityTO EmployersLiability
        {
            get
            {
                return this.EmployersLiabilityField;
            }
            set
            {
                this.EmployersLiabilityField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.GeneralCoverDetailsTO GeneralCoverDetails
        {
            get
            {
                return this.GeneralCoverDetailsField;
            }
            set
            {
                this.GeneralCoverDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.GeneralLiabilityTO GeneralLiability
        {
            get
            {
                return this.GeneralLiabilityField;
            }
            set
            {
                this.GeneralLiabilityField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.StatutoryLiabilityTO StatutoryLiability
        {
            get
            {
                return this.StatutoryLiabilityField;
            }
            set
            {
                this.StatutoryLiabilityField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialMotorVehicleLineLevelModifierTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialMotorVehicleLineLevelModifierTO : EServices.SubmissionProxy.LineLevelModifierTO
    {
        
        private System.Nullable<decimal> PercentOfPremiumPerGroupField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> PercentOfPremiumPerGroup
        {
            get
            {
                return this.PercentOfPremiumPerGroupField;
            }
            set
            {
                this.PercentOfPremiumPerGroupField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialVehicleTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialVehicleResponseTO))]
    public partial class CommercialVehicleTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private System.Nullable<EServices.SubmissionProxy.AreaOfOperation_LGI> AreaOfOperationField;
        
        private EServices.SubmissionProxy.CommercialVehicleDetailsTO[] CommercialVehicleDetailsField;
        
        private System.Nullable<EServices.SubmissionProxy.CoverOption_LGI> CoverOptionField;
        
        private EServices.SubmissionProxy.CoverageTO[] CoveragesField;
        
        private EServices.SubmissionProxy.DollarModifierTO DollarModifierField;
        
        private EServices.SubmissionProxy.LocationTO GarageLocationField;
        
        private System.Nullable<EServices.SubmissionProxy.Grade_LGI> GradeField;
        
        private System.Nullable<decimal> GroupSumInsuredField;
        
        private System.Nullable<EServices.SubmissionProxy.CMVLargeLoss_LGI> LargeLossField;
        
        private EServices.SubmissionProxy.PercentageModifierTO PercentageModifierField;
        
        private System.Nullable<EServices.SubmissionProxy.CMVSubUseType_LGI> SubUseTypeField;
        
        private System.Nullable<EServices.SubmissionProxy.CMVUseType_LGI> UseTypeField;
        
        private System.Nullable<EServices.SubmissionProxy.VehicleType> VehicleTypeField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.AreaOfOperation_LGI> AreaOfOperation
        {
            get
            {
                return this.AreaOfOperationField;
            }
            set
            {
                this.AreaOfOperationField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialVehicleDetailsTO[] CommercialVehicleDetails
        {
            get
            {
                return this.CommercialVehicleDetailsField;
            }
            set
            {
                this.CommercialVehicleDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CoverOption_LGI> CoverOption
        {
            get
            {
                return this.CoverOptionField;
            }
            set
            {
                this.CoverOptionField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CoverageTO[] Coverages
        {
            get
            {
                return this.CoveragesField;
            }
            set
            {
                this.CoveragesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.DollarModifierTO DollarModifier
        {
            get
            {
                return this.DollarModifierField;
            }
            set
            {
                this.DollarModifierField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.LocationTO GarageLocation
        {
            get
            {
                return this.GarageLocationField;
            }
            set
            {
                this.GarageLocationField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.Grade_LGI> Grade
        {
            get
            {
                return this.GradeField;
            }
            set
            {
                this.GradeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> GroupSumInsured
        {
            get
            {
                return this.GroupSumInsuredField;
            }
            set
            {
                this.GroupSumInsuredField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CMVLargeLoss_LGI> LargeLoss
        {
            get
            {
                return this.LargeLossField;
            }
            set
            {
                this.LargeLossField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.PercentageModifierTO PercentageModifier
        {
            get
            {
                return this.PercentageModifierField;
            }
            set
            {
                this.PercentageModifierField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CMVSubUseType_LGI> SubUseType
        {
            get
            {
                return this.SubUseTypeField;
            }
            set
            {
                this.SubUseTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CMVUseType_LGI> UseType
        {
            get
            {
                return this.UseTypeField;
            }
            set
            {
                this.UseTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.VehicleType> VehicleType
        {
            get
            {
                return this.VehicleTypeField;
            }
            set
            {
                this.VehicleTypeField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CMVPolicyType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum CMVPolicyType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fleet = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nonfleet = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_individual = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_1to10 = 3,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="IdAwareTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.HazardousSubstancesTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.LocationTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialPropertyLocationDetailTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.InterestedPartyTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialPropertyLoactionResponseTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.HotworkTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialVehicleDetailsTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.SubContractorTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialPropertyLossDetailTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.GeneralCoverActivityTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.PriorLossDetailsTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.ProductDetailTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialVehicleTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialVehicleResponseTO))]
    public partial class IdAwareTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string ExternalIdField;
        
        private string ExternalProducerField;
        
        private string LumleyIdField;
        
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
        public string ExternalId
        {
            get
            {
                return this.ExternalIdField;
            }
            set
            {
                this.ExternalIdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ExternalProducer
        {
            get
            {
                return this.ExternalProducerField;
            }
            set
            {
                this.ExternalProducerField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LumleyId
        {
            get
            {
                return this.LumleyIdField;
            }
            set
            {
                this.LumleyIdField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="HazardousSubstancesTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class HazardousSubstancesTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private string OtherTypeDetailsField;
        
        private System.Nullable<EServices.SubmissionProxy.LiaGLHazSubType_LGI> TypeField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OtherTypeDetails
        {
            get
            {
                return this.OtherTypeDetailsField;
            }
            set
            {
                this.OtherTypeDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.LiaGLHazSubType_LGI> Type
        {
            get
            {
                return this.TypeField;
            }
            set
            {
                this.TypeField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LocationTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialPropertyLocationDetailTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialPropertyLoactionResponseTO))]
    public partial class LocationTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private string AddrLine1Field;
        
        private string AddrLine2Field;
        
        private string AreaUnitField;
        
        private string CNARIDField;
        
        private string CityField;
        
        private System.Nullable<EServices.AccountProxy.Country> CountryField;
        
        private string DPIDField;
        
        private System.Nullable<int> GeocodeReliabilityField;
        
        private System.Nullable<decimal> LatitudeField;
        
        private System.Nullable<decimal> LongitudeField;
        
        private string MeshBlockIDField;
        
        private string PostcodeField;
        
        private string SuburbField;
        
        private string ValidationStatusField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AddrLine1
        {
            get
            {
                return this.AddrLine1Field;
            }
            set
            {
                this.AddrLine1Field = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AddrLine2
        {
            get
            {
                return this.AddrLine2Field;
            }
            set
            {
                this.AddrLine2Field = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AreaUnit
        {
            get
            {
                return this.AreaUnitField;
            }
            set
            {
                this.AreaUnitField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string CNARID
        {
            get
            {
                return this.CNARIDField;
            }
            set
            {
                this.CNARIDField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string City
        {
            get
            {
                return this.CityField;
            }
            set
            {
                this.CityField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.AccountProxy.Country> Country
        {
            get
            {
                return this.CountryField;
            }
            set
            {
                this.CountryField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string DPID
        {
            get
            {
                return this.DPIDField;
            }
            set
            {
                this.DPIDField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> GeocodeReliability
        {
            get
            {
                return this.GeocodeReliabilityField;
            }
            set
            {
                this.GeocodeReliabilityField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> Latitude
        {
            get
            {
                return this.LatitudeField;
            }
            set
            {
                this.LatitudeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> Longitude
        {
            get
            {
                return this.LongitudeField;
            }
            set
            {
                this.LongitudeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string MeshBlockID
        {
            get
            {
                return this.MeshBlockIDField;
            }
            set
            {
                this.MeshBlockIDField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Postcode
        {
            get
            {
                return this.PostcodeField;
            }
            set
            {
                this.PostcodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Suburb
        {
            get
            {
                return this.SuburbField;
            }
            set
            {
                this.SuburbField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ValidationStatus
        {
            get
            {
                return this.ValidationStatusField;
            }
            set
            {
                this.ValidationStatusField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyLocationDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialPropertyLoactionResponseTO))]
    public partial class CommercialPropertyLocationDetailTO : EServices.SubmissionProxy.LocationTO
    {
        
        private EServices.SubmissionProxy.CommercialPropertyBuildingDetailTO BuildingField;
        
        private EServices.SubmissionProxy.InterestedPartyTO[] InterestedPartiesField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialPropertyBuildingDetailTO Building
        {
            get
            {
                return this.BuildingField;
            }
            set
            {
                this.BuildingField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.InterestedPartyTO[] InterestedParties
        {
            get
            {
                return this.InterestedPartiesField;
            }
            set
            {
                this.InterestedPartiesField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="InterestedPartyTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class InterestedPartyTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private System.Nullable<EServices.AccountProxy.ContactType> AccountTypeField;
        
        private string CityField;
        
        private string FirstNameField;
        
        private string LastNameField;
        
        private string OrganisationNameField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.AccountProxy.ContactType> AccountType
        {
            get
            {
                return this.AccountTypeField;
            }
            set
            {
                this.AccountTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string City
        {
            get
            {
                return this.CityField;
            }
            set
            {
                this.CityField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FirstName
        {
            get
            {
                return this.FirstNameField;
            }
            set
            {
                this.FirstNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LastName
        {
            get
            {
                return this.LastNameField;
            }
            set
            {
                this.LastNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OrganisationName
        {
            get
            {
                return this.OrganisationNameField;
            }
            set
            {
                this.OrganisationNameField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyLoactionResponseTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyLoactionResponseTO : EServices.SubmissionProxy.CommercialPropertyLocationDetailTO
    {
        
        private EServices.SubmissionProxy.CommercialPropertyCoverageDetailTO[] CoveragesField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialPropertyCoverageDetailTO[] Coverages
        {
            get
            {
                return this.CoveragesField;
            }
            set
            {
                this.CoveragesField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="HotworkTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class HotworkTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private System.Nullable<EServices.SubmissionProxy.LiaGLHotWorkLocation_LGI> LocationDetailsField;
        
        private string OtherTypeDetailsField;
        
        private System.Nullable<int> TurnoverPctField;
        
        private System.Nullable<EServices.SubmissionProxy.LiaGLHotworkType_LGI> TypeField;
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public System.Nullable<EServices.SubmissionProxy.LiaGLHotWorkLocation_LGI> LocationDetails
        {
            get
            {
                return this.LocationDetailsField;
            }
            set
            {
                this.LocationDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OtherTypeDetails
        {
            get
            {
                return this.OtherTypeDetailsField;
            }
            set
            {
                this.OtherTypeDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> TurnoverPct
        {
            get
            {
                return this.TurnoverPctField;
            }
            set
            {
                this.TurnoverPctField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.LiaGLHotworkType_LGI> Type
        {
            get
            {
                return this.TypeField;
            }
            set
            {
                this.TypeField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialVehicleDetailsTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialVehicleDetailsTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private System.Nullable<EServices.SubmissionProxy.ExternalBodyType_LGI> BodyStyleField;
        
        private string BodyStyleAsStringField;
        
        private System.Nullable<EServices.SubmissionProxy.BodyType> BodyTypeField;
        
        private System.Nullable<bool> CheckedField;
        
        private System.Nullable<bool> DetailsUnknownField;
        
        private System.Nullable<int> GrossVehicleMassField;
        
        private EServices.SubmissionProxy.InterestedPartyTO[] InterestedPartiesField;
        
        private string LicensePlateField;
        
        private System.Nullable<EServices.SubmissionProxy.VehicleMake_LGI> MakeField;
        
        private string MakeAsStringField;
        
        private string ModelField;
        
        private System.Nullable<int> NumberOfAxlesField;
        
        private System.Nullable<decimal> StatedValueField;
        
        private string VINField;
        
        private System.Nullable<EServices.SubmissionProxy.ExternalVehicleType_LGI> VehicleTypeField;
        
        private string VehicleTypeAsStringField;
        
        private System.Nullable<int> YearField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.ExternalBodyType_LGI> BodyStyle
        {
            get
            {
                return this.BodyStyleField;
            }
            set
            {
                this.BodyStyleField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string BodyStyleAsString
        {
            get
            {
                return this.BodyStyleAsStringField;
            }
            set
            {
                this.BodyStyleAsStringField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.BodyType> BodyType
        {
            get
            {
                return this.BodyTypeField;
            }
            set
            {
                this.BodyTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> Checked
        {
            get
            {
                return this.CheckedField;
            }
            set
            {
                this.CheckedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> DetailsUnknown
        {
            get
            {
                return this.DetailsUnknownField;
            }
            set
            {
                this.DetailsUnknownField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> GrossVehicleMass
        {
            get
            {
                return this.GrossVehicleMassField;
            }
            set
            {
                this.GrossVehicleMassField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.InterestedPartyTO[] InterestedParties
        {
            get
            {
                return this.InterestedPartiesField;
            }
            set
            {
                this.InterestedPartiesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LicensePlate
        {
            get
            {
                return this.LicensePlateField;
            }
            set
            {
                this.LicensePlateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.VehicleMake_LGI> Make
        {
            get
            {
                return this.MakeField;
            }
            set
            {
                this.MakeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string MakeAsString
        {
            get
            {
                return this.MakeAsStringField;
            }
            set
            {
                this.MakeAsStringField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Model
        {
            get
            {
                return this.ModelField;
            }
            set
            {
                this.ModelField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> NumberOfAxles
        {
            get
            {
                return this.NumberOfAxlesField;
            }
            set
            {
                this.NumberOfAxlesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> StatedValue
        {
            get
            {
                return this.StatedValueField;
            }
            set
            {
                this.StatedValueField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string VIN
        {
            get
            {
                return this.VINField;
            }
            set
            {
                this.VINField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.ExternalVehicleType_LGI> VehicleType
        {
            get
            {
                return this.VehicleTypeField;
            }
            set
            {
                this.VehicleTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string VehicleTypeAsString
        {
            get
            {
                return this.VehicleTypeAsStringField;
            }
            set
            {
                this.VehicleTypeAsStringField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> Year
        {
            get
            {
                return this.YearField;
            }
            set
            {
                this.YearField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SubContractorTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class SubContractorTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private System.Nullable<decimal> AnnualPaymentsField;
        
        private string TypeOfWorkField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> AnnualPayments
        {
            get
            {
                return this.AnnualPaymentsField;
            }
            set
            {
                this.AnnualPaymentsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string TypeOfWork
        {
            get
            {
                return this.TypeOfWorkField;
            }
            set
            {
                this.TypeOfWorkField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyLossDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyLossDetailTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private System.Nullable<System.DateTime> DateOfLossField;
        
        private string DetailsOfLossField;
        
        private System.Nullable<decimal> ValueOfLossField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> DateOfLoss
        {
            get
            {
                return this.DateOfLossField;
            }
            set
            {
                this.DateOfLossField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string DetailsOfLoss
        {
            get
            {
                return this.DetailsOfLossField;
            }
            set
            {
                this.DetailsOfLossField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> ValueOfLoss
        {
            get
            {
                return this.ValueOfLossField;
            }
            set
            {
                this.ValueOfLossField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="GeneralCoverActivityTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class GeneralCoverActivityTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private System.Nullable<decimal> ActualTurnoverField;
        
        private string DescriptionField;
        
        private System.Nullable<decimal> EstimatedTurnoverField;
        
        private string InsuredNameField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> ActualTurnover
        {
            get
            {
                return this.ActualTurnoverField;
            }
            set
            {
                this.ActualTurnoverField = value;
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
        public System.Nullable<decimal> EstimatedTurnover
        {
            get
            {
                return this.EstimatedTurnoverField;
            }
            set
            {
                this.EstimatedTurnoverField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string InsuredName
        {
            get
            {
                return this.InsuredNameField;
            }
            set
            {
                this.InsuredNameField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ProductDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class ProductDetailTO : EServices.SubmissionProxy.IdAwareTO
    {
        
        private System.Nullable<EServices.SubmissionProxy.LiaGLPrdDetailBizType_LGI> BizTypeField;
        
        private System.Nullable<EServices.SubmissionProxy.LiaGLPrdDetailCountry_LGI> ExportCountryField;
        
        private System.Nullable<EServices.SubmissionProxy.LiaGLPrdDetailCountry_LGI> ImportCountryField;
        
        private string ProductDetailDescriptionField;
        
        private System.Nullable<decimal> TurnoverField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.LiaGLPrdDetailBizType_LGI> BizType
        {
            get
            {
                return this.BizTypeField;
            }
            set
            {
                this.BizTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.LiaGLPrdDetailCountry_LGI> ExportCountry
        {
            get
            {
                return this.ExportCountryField;
            }
            set
            {
                this.ExportCountryField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.LiaGLPrdDetailCountry_LGI> ImportCountry
        {
            get
            {
                return this.ImportCountryField;
            }
            set
            {
                this.ImportCountryField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ProductDetailDescription
        {
            get
            {
                return this.ProductDetailDescriptionField;
            }
            set
            {
                this.ProductDetailDescriptionField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> Turnover
        {
            get
            {
                return this.TurnoverField;
            }
            set
            {
                this.TurnoverField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialVehicleResponseTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialVehicleResponseTO : EServices.SubmissionProxy.CommercialVehicleTO
    {
        
        private EServices.SubmissionProxy.ConditionTO[] ConditionsField;
        
        private EServices.SubmissionProxy.ExclusionTO[] ExclusionsField;
        
        private EServices.SubmissionProxy.CommercialVehiclePremiumTO PremiumDetailsField;
        
        private EServices.SubmissionProxy.TransactionCostTO TransactionCostsField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.ConditionTO[] Conditions
        {
            get
            {
                return this.ConditionsField;
            }
            set
            {
                this.ConditionsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.ExclusionTO[] Exclusions
        {
            get
            {
                return this.ExclusionsField;
            }
            set
            {
                this.ExclusionsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialVehiclePremiumTO PremiumDetails
        {
            get
            {
                return this.PremiumDetailsField;
            }
            set
            {
                this.PremiumDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.TransactionCostTO TransactionCosts
        {
            get
            {
                return this.TransactionCostsField;
            }
            set
            {
                this.TransactionCostsField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DollarModifierTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class DollarModifierTO : EServices.SubmissionProxy.ModifierTO
    {
        
        private System.Nullable<decimal> DollarModifierValueField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> DollarModifierValue
        {
            get
            {
                return this.DollarModifierValueField;
            }
            set
            {
                this.DollarModifierValueField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PercentageModifierTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class PercentageModifierTO : EServices.SubmissionProxy.ModifierTO
    {
        
        private System.Nullable<decimal> PercentageModifierValueField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> PercentageModifierValue
        {
            get
            {
                return this.PercentageModifierValueField;
            }
            set
            {
                this.PercentageModifierValueField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="AreaOfOperation_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum AreaOfOperation_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NRTH = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AUCK = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WAIK = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BBAY = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GISB = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_HAWK = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TARA = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MANA = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WELL = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MARL = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NELS = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WCST = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CANT = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_OTAG = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_STHD = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AUS = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SPC = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CHCH = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NEWP = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_RONI = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ROSI = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_EXCH = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SORR = 22,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CoverOption_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum CoverOption_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pvt_home_suminsured = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pvt_home_area = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pvt_contents_full = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pvt_contents_renters = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pvt_contents_barracks = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pvt_vhcl_full = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pvt_vhcl_tpft = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pvt_vhcl_tpo = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pvt_boat = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_all = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cmv_vhcl_full = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cmv_vhcl_fire_only = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cmv_vhcl_tpft = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cmv_vhcl_tpo = 13,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CoverageTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CoverageDetailTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.LiabilityCoverageDetailTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialPropertyCoverageDetailTO))]
    public partial class CoverageTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CovTermTO[] CovTermsField;
        
        private string NameField;
        
        private string PatternCodeField;
        
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
        public EServices.SubmissionProxy.CovTermTO[] CovTerms
        {
            get
            {
                return this.CovTermsField;
            }
            set
            {
                this.CovTermsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name
        {
            get
            {
                return this.NameField;
            }
            set
            {
                this.NameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PatternCode
        {
            get
            {
                return this.PatternCodeField;
            }
            set
            {
                this.PatternCodeField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Grade_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum Grade_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_1 = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_2 = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_3 = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_4 = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CMVLargeLoss_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum CMVLargeLoss_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_OnePlusInLast3Years = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NilInLast3Years = 1,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CMVSubUseType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum CMVSubUseType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_accountingcomputerconsultants = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_advertisingservices = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_agricultural = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_agriculturalcontractingservices = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_All = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_architecturalengineeringsurveying = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_autorepair_lgi = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_builders = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_buildmatsuphard = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bulkcartage = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_buses = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_businessprofessionalassociations = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cartruckrentalservices = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_carparkingservices = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_carriersbulkgrain = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_centralgovernmentadministration = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_civilcontracting = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cleaningservices = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_concretedelivery = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_constructionbuilding = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_containertransport = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_contractorsother = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dangerousgoods = 22,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_drycleaningdomesticservices = 23,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_earthmoving = 24,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_educationresearchinstitutes = 25,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_electricians = 26,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_electricitygaswater = 27,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_entertainmentsporting = 28,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_farming = 29,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_financing = 30,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_firefighting = 31,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fishing = 32,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fishingconsultants = 33,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_forestry = 34,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_freightagents = 35,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_freighttransportfurnitureremoval = 36,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_freighttransportgeneralcarriage = 37,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_freighttransportlogginghaulage = 38,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_freighttransportrefrigeratedhaulage = 39,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_freighttransportstockhaulage = 40,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_frequentdelivery = 41,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_funeraldirectorservices = 42,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_funeralHomeService = 43,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_gardensrvc_lgi = 44,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_generalconstruction = 45,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_generalfreight = 46,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_generalinsurance = 47,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_generaltankers = 48,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_glazier = 49,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_government = 50,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_groundspreaders = 51,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hairdressersbeautyservices = 52,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_heavycommrental = 53,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_heavyretail = 54,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_insuranceagentsbrokers = 55,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_internationalbodies = 56,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_legalservices = 57,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lifeinsurance = 58,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lightcommrental = 59,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lightretail = 60,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_livestock = 61,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_localgovernmentadministration = 62,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_logging = 63,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_loggingothertimberfelling = 64,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_machineryequipmentrental = 65,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_manufacturing = 66,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_manufacturingallchemicalsandpetroleum = 67,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_manufacturingbuildingmaterials = 68,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_manufacturingfoodbeveragetobacco = 69,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_manufacturingmachineryequipment = 70,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_manufacturingotherindustries = 71,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_manufacturingpaperproducts = 72,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_manufacturingtextileclothing = 73,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_manufacturingwoodproducts = 74,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_medicaldentalveterinaryservices = 75,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_medicalinsurance = 76,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mining = 77,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_miningextractionexploration = 78,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mobilecranes = 79,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_otherbusinessservices = 80,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_otherretailtrade = 81,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_painters = 82,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_photographicservices = 83,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pipelinetransport = 84,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_plumbers = 85,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_postalcourierservices = 86,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_postalcourierlight = 87,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_professionalservices = 88,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_publishing = 89,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_realestate = 90,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_refrigeratedcarcase = 91,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_refrigeratedpallets = 92,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_religiouscommunityservices = 93,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_repairservices = 94,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_restaurantshotels = 95,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailofanimalsunprocessedprimaryproducts = 96,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailofbuildingmaterialssupplieshardware = 97,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailoffarminglivestock = 98,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailoffoodbeveragestobaccoproducts = 99,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailofhouseholdappliancesfurniturefloorcoverings = 100,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailofmachineryequipment = 101,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailofotherwholesaletrade = 102,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailofpaintwallpaperhardware = 103,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailofpaperproducts = 104,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailofpharmaceuticalsandpetroleumproducts = 105,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailofpharmaceuticalsupplies = 106,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailoftextilesclothingfootwear = 107,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailsector = 108,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_roadconstruction = 109,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sales = 110,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_saunasmassageparlours = 111,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_servicesector = 112,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_skipdelivery = 113,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_specialheavyhaulage = 114,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_storagewarehousing = 115,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_superannuationinsurance = 116,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_taxis = 117,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_technicalservices = 118,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_telephoneservices = 119,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_towercranes = 120,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transportair = 121,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transportbus = 122,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transportotherservices = 123,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transportrailway = 124,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transportsea = 125,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transporttaxi = 126,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transportvehiclesandfuels = 127,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_traveloperators = 128,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_unprocessedprimaryproducts = 129,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_various = 130,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_vehicletransport = 131,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_wastecompactors = 132,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_other = 133,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_agriculture_lgi = 134,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailofhouseholdappliancesfurniture = 135,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_retailoftextilesandclothing = 136,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CMVUseType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum CMVUseType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_All = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cartruckrental = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_civilcontracting = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_contractorsbuildeng = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lmvd = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_localgenbususe = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_passengertransport = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_taxis = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transport = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transportlinehaul = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transportlocal = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_wastemanagement = 11,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="VehicleType", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum VehicleType : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_other = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_All = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_busCoach_LGI = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_car_lgi = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_caravan_LGI = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_classicCar_LGI = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_courier_LGI = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_farmBike_LGI = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fireAppliance_LGI = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fourWheelDrive_LGI = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_funeralHearse_LGI = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_heavyMobilePlant_LGI = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_heavyTrailer_LGI = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_heavyTruck_LGI = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_homeMade_LGI = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lightTruck_LGI = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lightvehicle_LGI = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mediumTruck_LGI = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mobilePlant_LGI = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_motorCaravan_LGI = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_motorcycle_LGI = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_motorhome_LGI = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_muscleCar_LGI = 22,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sportsCar_LGI = 23,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_taxi_LGI = 24,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tractor_LGI = 25,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_trailer_LGI = 26,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_uteVan_LGI = 27,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_van_LGI = 28,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Commercial = 29,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PP = 30,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PublicTransport = 31,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Special = 32,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_auto = 33,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_convertedVehicle_LGI = 34,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lightvehicles_LGI = 35,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LiaGLHazSubType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum LiaGLHazSubType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bulklliquieshaz = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bulkliquidsnonhaz = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_chemicals = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dangerousgoods = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_explosives = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fertpestfungus = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_other = 6,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Country", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum Country : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZ = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AF = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AL = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DZ = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AS = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AD = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AO = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AI = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AQ = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AG = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AR = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AM = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AW = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AU = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AT = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AZ = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BS = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BH = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BD = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BB = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BY = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BE = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BZ = 22,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BJ = 23,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BM = 24,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BT = 25,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BO = 26,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BA = 27,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BW = 28,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BV = 29,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BR = 30,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IO = 31,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_VG = 32,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BN = 33,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BG = 34,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BF = 35,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BI = 36,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KH = 37,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CM = 38,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CA = 39,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CV = 40,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KY = 41,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CF = 42,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TD = 43,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CL = 44,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CN = 45,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CX = 46,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CC = 47,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CO = 48,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KM = 49,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CG = 50,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CD = 51,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CK = 52,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CR = 53,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_HR = 54,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CU = 55,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CY = 56,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CZ = 57,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DK = 58,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DJ = 59,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DM = 60,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DO = 61,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_EC = 62,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_EG = 63,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SV = 64,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GQ = 65,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ER = 66,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_EE = 67,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ET = 68,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_FO = 69,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_FK = 70,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_FJ = 71,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_FI = 72,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_FR = 73,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GF = 74,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PF = 75,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TF = 76,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GA = 77,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GM = 78,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GE = 79,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DE = 80,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GH = 81,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GI = 82,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GR = 83,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GL = 84,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GD = 85,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GP = 86,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GU = 87,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GT = 88,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GN = 89,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GW = 90,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GY = 91,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_HT = 92,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_HM = 93,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_HN = 94,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_HK = 95,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_HU = 96,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IS = 97,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IN = 98,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ID = 99,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IR = 100,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IQ = 101,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IE = 102,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IL = 103,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IT = 104,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CI = 105,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_JM = 106,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_JP = 107,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_JO = 108,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KZ = 109,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KE = 110,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KI = 111,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KW = 112,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KG = 113,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LA = 114,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LV = 115,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LB = 116,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LS = 117,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LR = 118,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LY = 119,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LI = 120,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LT = 121,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LU = 122,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MO = 123,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MK = 124,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MG = 125,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MW = 126,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MY = 127,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MV = 128,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ML = 129,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MT = 130,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MH = 131,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MQ = 132,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MR = 133,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MU = 134,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_YT = 135,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MX = 136,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_FM = 137,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MD = 138,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MC = 139,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MN = 140,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ME = 141,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MS = 142,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MA = 143,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MZ = 144,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MM = 145,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NA = 146,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NR = 147,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NP = 148,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NL = 149,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AN = 150,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NC = 151,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NI = 152,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NE = 153,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NG = 154,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NU = 155,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NF = 156,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MP = 157,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KP = 158,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NO = 159,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_OM = 160,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PK = 161,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PW = 162,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PS = 163,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PA = 164,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PG = 165,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PY = 166,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PE = 167,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PH = 168,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PN = 169,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PL = 170,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PT = 171,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PR = 172,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_QA = 173,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_RE = 174,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_RO = 175,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_RU = 176,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_RW = 177,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WS = 178,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SM = 179,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ST = 180,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SA = 181,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SN = 182,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_RS = 183,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SC = 184,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SL = 185,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SG = 186,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SK = 187,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SI = 188,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SB = 189,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SO = 190,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ZA = 191,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GS = 192,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KR = 193,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ES = 194,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LK = 195,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SH = 196,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KN = 197,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LC = 198,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PM = 199,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_VC = 200,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SD = 201,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SR = 202,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SJ = 203,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SZ = 204,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SE = 205,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CH = 206,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SY = 207,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TW = 208,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TJ = 209,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TZ = 210,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TH = 211,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TL = 212,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TG = 213,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TK = 214,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TO = 215,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TT = 216,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TN = 217,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TR = 218,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TM = 219,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TC = 220,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TV = 221,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_UG = 222,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_UA = 223,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AE = 224,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GB = 225,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_UM = 226,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_US = 227,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_UY = 228,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_VI = 229,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_UZ = 230,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_VU = 231,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_VA = 232,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_VE = 233,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_VN = 234,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WF = 235,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_EH = 236,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_YE = 237,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ZM = 238,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ZW = 239,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_unknown = 240,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyBuildingDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyBuildingDetailTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string AdjANZSICCodeField;
        
        private string AutoFireSuppressionSystemDescField;
        
        private string BuildingDescField;
        
        private System.Nullable<System.DateTime> BuildingLastValuationDateField;
        
        private System.Nullable<EServices.SubmissionProxy.CPConstructionType_LGI> ConstructionTypeField;
        
        private System.Nullable<int> ConstructionYearField;
        
        private System.Nullable<bool> HasAlarmField;
        
        private bool HasInsulatedSandwichPanelsField;
        
        private System.Nullable<bool> HasOtherAutoFireSuppressSysField;
        
        private System.Nullable<bool> HasSmokeDetectorsField;
        
        private System.Nullable<bool> HasSprinklersField;
        
        private string InsuredANZSICCodeField;
        
        private bool IsBuilding3OrMoreStoreysHighField;
        
        private System.Nullable<bool> IsDualWaterSupplyField;
        
        private bool IsHalfOrMoreUnOccupiedField;
        
        private System.Nullable<bool> IsNoneOfTheAboveField;
        
        private System.Nullable<bool> IsOccupBurglarRiskField;
        
        private System.Nullable<bool> IsOccupFireRiskField;
        
        private bool IsTownWaterSuppliedField;
        
        private System.Nullable<System.DateTime> LastFPISInspectionField;
        
        private System.Nullable<EServices.SubmissionProxy.CPCompliantOption_LGI> NZS4541CompliantField;
        
        private System.Nullable<int> NumOfUnitsField;
        
        private System.Nullable<int> NumberOfStoreysField;
        
        private System.Nullable<int> PercentOfInsuSandwichPanelsField;
        
        private System.Nullable<System.DateTime> PlantLastValuationDateField;
        
        private System.Nullable<EServices.SubmissionProxy.CPRiskSurveyClass_LGI> RiskSurveyClassificationField;
        
        private System.Nullable<EServices.SubmissionProxy.CPStructFramingType_LGI> StructuralFramingField;
        
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
        public string AdjANZSICCode
        {
            get
            {
                return this.AdjANZSICCodeField;
            }
            set
            {
                this.AdjANZSICCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AutoFireSuppressionSystemDesc
        {
            get
            {
                return this.AutoFireSuppressionSystemDescField;
            }
            set
            {
                this.AutoFireSuppressionSystemDescField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string BuildingDesc
        {
            get
            {
                return this.BuildingDescField;
            }
            set
            {
                this.BuildingDescField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> BuildingLastValuationDate
        {
            get
            {
                return this.BuildingLastValuationDateField;
            }
            set
            {
                this.BuildingLastValuationDateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CPConstructionType_LGI> ConstructionType
        {
            get
            {
                return this.ConstructionTypeField;
            }
            set
            {
                this.ConstructionTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> ConstructionYear
        {
            get
            {
                return this.ConstructionYearField;
            }
            set
            {
                this.ConstructionYearField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasAlarm
        {
            get
            {
                return this.HasAlarmField;
            }
            set
            {
                this.HasAlarmField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool HasInsulatedSandwichPanels
        {
            get
            {
                return this.HasInsulatedSandwichPanelsField;
            }
            set
            {
                this.HasInsulatedSandwichPanelsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasOtherAutoFireSuppressSys
        {
            get
            {
                return this.HasOtherAutoFireSuppressSysField;
            }
            set
            {
                this.HasOtherAutoFireSuppressSysField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasSmokeDetectors
        {
            get
            {
                return this.HasSmokeDetectorsField;
            }
            set
            {
                this.HasSmokeDetectorsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasSprinklers
        {
            get
            {
                return this.HasSprinklersField;
            }
            set
            {
                this.HasSprinklersField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string InsuredANZSICCode
        {
            get
            {
                return this.InsuredANZSICCodeField;
            }
            set
            {
                this.InsuredANZSICCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsBuilding3OrMoreStoreysHigh
        {
            get
            {
                return this.IsBuilding3OrMoreStoreysHighField;
            }
            set
            {
                this.IsBuilding3OrMoreStoreysHighField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> IsDualWaterSupply
        {
            get
            {
                return this.IsDualWaterSupplyField;
            }
            set
            {
                this.IsDualWaterSupplyField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsHalfOrMoreUnOccupied
        {
            get
            {
                return this.IsHalfOrMoreUnOccupiedField;
            }
            set
            {
                this.IsHalfOrMoreUnOccupiedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> IsNoneOfTheAbove
        {
            get
            {
                return this.IsNoneOfTheAboveField;
            }
            set
            {
                this.IsNoneOfTheAboveField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> IsOccupBurglarRisk
        {
            get
            {
                return this.IsOccupBurglarRiskField;
            }
            set
            {
                this.IsOccupBurglarRiskField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> IsOccupFireRisk
        {
            get
            {
                return this.IsOccupFireRiskField;
            }
            set
            {
                this.IsOccupFireRiskField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsTownWaterSupplied
        {
            get
            {
                return this.IsTownWaterSuppliedField;
            }
            set
            {
                this.IsTownWaterSuppliedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> LastFPISInspection
        {
            get
            {
                return this.LastFPISInspectionField;
            }
            set
            {
                this.LastFPISInspectionField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CPCompliantOption_LGI> NZS4541Compliant
        {
            get
            {
                return this.NZS4541CompliantField;
            }
            set
            {
                this.NZS4541CompliantField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> NumOfUnits
        {
            get
            {
                return this.NumOfUnitsField;
            }
            set
            {
                this.NumOfUnitsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> NumberOfStoreys
        {
            get
            {
                return this.NumberOfStoreysField;
            }
            set
            {
                this.NumberOfStoreysField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> PercentOfInsuSandwichPanels
        {
            get
            {
                return this.PercentOfInsuSandwichPanelsField;
            }
            set
            {
                this.PercentOfInsuSandwichPanelsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> PlantLastValuationDate
        {
            get
            {
                return this.PlantLastValuationDateField;
            }
            set
            {
                this.PlantLastValuationDateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CPRiskSurveyClass_LGI> RiskSurveyClassification
        {
            get
            {
                return this.RiskSurveyClassificationField;
            }
            set
            {
                this.RiskSurveyClassificationField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CPStructFramingType_LGI> StructuralFraming
        {
            get
            {
                return this.StructuralFramingField;
            }
            set
            {
                this.StructuralFramingField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CPConstructionType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum CPConstructionType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Massive = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BrickOrConcrete = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Mixed = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_InsulatedSandwichPanels = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Other = 4,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CPCompliantOption_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum CPCompliantOption_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_No = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NoWithMinorIssues = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Yes = 2,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CPRiskSurveyClass_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum CPRiskSurveyClass_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Superior = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GoodPlus = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Good = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GoodMinus = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Fair = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_FairMinus = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Poor = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Unacceptable = 7,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CPStructFramingType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum CPStructFramingType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_InsulatedSandwichPanels = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ReinforcedConcreteBeams = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SteelOrRSJs = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Timber = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_UnreinforcedMasonryOrBrick = 4,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ContactType", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum ContactType : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_company = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_person = 1,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyCoverageDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyCoverageDetailTO : EServices.SubmissionProxy.CoverageDetailTO
    {
        
        private System.Nullable<decimal> BenchmarkRateField;
        
        private System.Nullable<bool> MinimumPremiumUsedField;
        
        private EServices.SubmissionProxy.RateModifierTO RateModifierField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> BenchmarkRate
        {
            get
            {
                return this.BenchmarkRateField;
            }
            set
            {
                this.BenchmarkRateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> MinimumPremiumUsed
        {
            get
            {
                return this.MinimumPremiumUsedField;
            }
            set
            {
                this.MinimumPremiumUsedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.RateModifierTO RateModifier
        {
            get
            {
                return this.RateModifierField;
            }
            set
            {
                this.RateModifierField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CoverageDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.LiabilityCoverageDetailTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialPropertyCoverageDetailTO))]
    public partial class CoverageDetailTO : EServices.SubmissionProxy.CoverageTO
    {
        
        private EServices.SubmissionProxy.PremiumTO PremiumDetailsField;
        
        private EServices.SubmissionProxy.TransactionCostTO TransactionCostsField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.PremiumTO PremiumDetails
        {
            get
            {
                return this.PremiumDetailsField;
            }
            set
            {
                this.PremiumDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.TransactionCostTO TransactionCosts
        {
            get
            {
                return this.TransactionCostsField;
            }
            set
            {
                this.TransactionCostsField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LiabilityCoverageDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class LiabilityCoverageDetailTO : EServices.SubmissionProxy.CoverageDetailTO
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CovTermTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CovTermTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string CodeField;
        
        private string FormattedValueField;
        
        private string NameField;
        
        private string ValueField;
        
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
        public string Code
        {
            get
            {
                return this.CodeField;
            }
            set
            {
                this.CodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FormattedValue
        {
            get
            {
                return this.FormattedValueField;
            }
            set
            {
                this.FormattedValueField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name
        {
            get
            {
                return this.NameField;
            }
            set
            {
                this.NameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Value
        {
            get
            {
                return this.ValueField;
            }
            set
            {
                this.ValueField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PremiumTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialVehiclePremiumTO))]
    public partial class PremiumTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<decimal> CompanyPremiumField;
        
        private System.Nullable<decimal> EQCField;
        
        private System.Nullable<decimal> FSLField;
        
        private System.Nullable<decimal> GSTField;
        
        private System.Nullable<decimal> TotalPremiumField;
        
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
        public System.Nullable<decimal> CompanyPremium
        {
            get
            {
                return this.CompanyPremiumField;
            }
            set
            {
                this.CompanyPremiumField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> EQC
        {
            get
            {
                return this.EQCField;
            }
            set
            {
                this.EQCField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> FSL
        {
            get
            {
                return this.FSLField;
            }
            set
            {
                this.FSLField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> GST
        {
            get
            {
                return this.GSTField;
            }
            set
            {
                this.GSTField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> TotalPremium
        {
            get
            {
                return this.TotalPremiumField;
            }
            set
            {
                this.TotalPremiumField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TransactionCostTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class TransactionCostTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<decimal> CompanyPremiumField;
        
        private System.Nullable<decimal> EQCField;
        
        private System.Nullable<decimal> FSLField;
        
        private System.Nullable<decimal> GSTField;
        
        private System.Nullable<decimal> TotalPremiumField;
        
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
        public System.Nullable<decimal> CompanyPremium
        {
            get
            {
                return this.CompanyPremiumField;
            }
            set
            {
                this.CompanyPremiumField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> EQC
        {
            get
            {
                return this.EQCField;
            }
            set
            {
                this.EQCField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> FSL
        {
            get
            {
                return this.FSLField;
            }
            set
            {
                this.FSLField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> GST
        {
            get
            {
                return this.GSTField;
            }
            set
            {
                this.GSTField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> TotalPremium
        {
            get
            {
                return this.TotalPremiumField;
            }
            set
            {
                this.TotalPremiumField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="RateModifierTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class RateModifierTO : EServices.SubmissionProxy.ModifierTO
    {
        
        private System.Nullable<decimal> RateModifierValueField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> RateModifierValue
        {
            get
            {
                return this.RateModifierValueField;
            }
            set
            {
                this.RateModifierValueField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialVehiclePremiumTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialVehiclePremiumTO : EServices.SubmissionProxy.PremiumTO
    {
        
        private System.Nullable<decimal> CommissionRateField;
        
        private System.Nullable<int> VehicleNumberField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> CommissionRate
        {
            get
            {
                return this.CommissionRateField;
            }
            set
            {
                this.CommissionRateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> VehicleNumber
        {
            get
            {
                return this.VehicleNumberField;
            }
            set
            {
                this.VehicleNumberField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ModifierTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.DollarModifierTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.PercentageModifierTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.RateModifierTO))]
    public partial class ModifierTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string DetailsField;
        
        private System.Nullable<EServices.SubmissionProxy.RateFactorReason_LGI> ReasonField;
        
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
        public string Details
        {
            get
            {
                return this.DetailsField;
            }
            set
            {
                this.DetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.RateFactorReason_LGI> Reason
        {
            get
            {
                return this.ReasonField;
            }
            set
            {
                this.ReasonField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="RateFactorReason_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum RateFactorReason_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_clienthistory = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_meetcompetitor = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_packagediscount = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_other = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LiaGLHotWorkLocation_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum LiaGLHotWorkLocation_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Both = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_OwnPremises = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ThirdPartyPremises = 2,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LiaGLHotworkType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum LiaGLHotworkType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_gasdisccutting = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_grinding = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_other = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_solderingbrazing = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_heatguns = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_welding = 5,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ExternalBodyType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum ExternalBodyType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_at = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tb = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cc = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tc = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cv = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_td = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tf = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ft = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ha = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hb = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hv = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lv = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lb = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mm = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mc = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_or = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_to = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_others = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sl = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sc = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sp = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sw = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ta = 22,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ut = 23,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="BodyType", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum BodyType : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_otherPlant_LGI = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mobilePlantAgricultural_LGI = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bobcat_LGI = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bTrain_LGI = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bulldozer_LGI = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_car_LGI = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_caravan_LGI = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_compactor_LGI = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_concretemixer_LGI = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_concretepump_LGI = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_convertible = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_coupe = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_courierCar_LGI = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_courierLightTruck_LGI = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_courierUte_LGI = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_courierVan_LGI = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_crane_LGI = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_craneMobile_LGI = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_craneStructure_LGI = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_customervehicleliability_LGI = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_excavator_LGI = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fireAppliance_LGI = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mobilePlantForestry_LGI = 22,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_forkhoist_LGI = 23,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fourdoor = 24,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fullTrailer_LGI = 25,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_funeralHearse_LGI = 26,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_golfcart_LGI = 27,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_grader_LGI = 28,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hearse_LGI = 29,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_heavyBusCoach_LGI = 30,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hybridcar_LGI = 31,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hybridute_LGI = 32,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hybridvan_LGI = 33,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lightTrailer_LGI = 34,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lightTruck_LGI = 35,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_minibusCoach_LGI = 36,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mobileCrane_LGI = 37,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mobilePlantContractors_LGI = 38,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_motorCaravan_LGI = 39,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_motorVehicleTypeCaravan = 40,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_motorcycle_LGI = 41,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pickup = 42,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_quadbike_lgi = 43,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_rigidTruck_LGI = 44,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_roller_LGI = 45,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_rv = 46,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_semiTrailer_LGI = 47,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_shuttleVan_LGI = 48,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_skipdeliverytruck_LGI = 49,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_snowmobile = 50,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_wagon = 51,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_suv = 52,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tippingTrailer_LGI = 53,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tipperTruck_LGI = 54,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_towTruck_LGI = 55,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tractor_LGI = 56,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tractorPrime_LGI = 57,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_trailer = 58,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_truck = 59,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_twodoor = 60,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_unknown_LGI = 61,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ute_LGI = 62,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_util45trailer = 63,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_van = 64,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_van_LGI = 65,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_wasteCompactorTruck_LGI = 66,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_wasteTruck_LGI = 67,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_xplate_LGI = 68,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_farmTractor_LGI = 69,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_motorcycle = 70,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_taxicar_LGI = 71,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_taxivan_LGI = 72,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="VehicleMake_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum VehicleMake_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_aec = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_agrimachine_lgi = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_alexanderdennis_lgi = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_alfa_romeo = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_allischalmers_lgi = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_All = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_american_lgi = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_aprilia = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_astonmartin_lgi = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_atkinson = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_audi = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_austin = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_austin_healey = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bailey_lgi = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bedford = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_belarus_lgi = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bentley = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bmc_lgi = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bmw = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bombardier_lgi = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bsa = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_buell = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_bugatti_lgi = 22,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_buick_lgi = 23,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cadillac = 24,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cagiva_lgi = 25,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_canam_lgi = 26,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_caravan = 27,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_case_lgi = 28,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_caterpillar = 29,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_chamberlain_lgi = 30,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_chery_lgi = 31,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_chevrolet = 32,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_chrysler = 33,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_citroen = 34,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_claas_lgi = 35,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_clark = 36,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cleveland_lgi = 37,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_commer = 38,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cooper_lgi = 39,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_crown_lgi = 40,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_daewoo = 41,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_daf = 42,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_daihatsu = 43,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_daimler = 44,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_datsun_lgi = 45,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_davidbrown_lgi = 46,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_daytona_lgi = 47,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dealer_lgi = 48,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_delorean_lgi = 49,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_delta_LGI = 50,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dodge = 51,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_denning_lgi = 52,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dennis_lgi = 53,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_designline_lgi = 54,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_desoto_lgi = 55,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_detomaso_lgi = 56,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_deutzfahr_lgi = 57,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ditchwitch_lgi = 58,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ducati = 59,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_eldorado_lgi = 60,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_elf_lgi = 61,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_enfield_lgi = 62,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_erf = 63,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_essex_lgi = 64,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_excalibur_lgi = 65,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_excelsior_lgi = 66,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fairfax_LGI = 67,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fendt_lgi = 68,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ferguson_lgi = 69,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ferrari = 70,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fiat = 71,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_foden = 72,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ford = 73,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_foton_lgi = 74,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fraser_lgi = 75,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_freightliner = 76,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fruehauf_LGI = 77,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fuso = 78,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_geely_lgi = 79,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_gmc_lgi = 80,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_goldoni_lgi = 81,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_greatwall_lgi = 82,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_haflinger_lgi = 83,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hamelex_LGI = 84,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hammar_LGI = 85,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_harley_davidson = 86,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_henderson_lgi = 87,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hero_lgi = 88,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_higer_lgi = 89,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hillman_lgi = 90,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hino = 91,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hitachi = 92,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_holden = 93,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_honda = 94,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hummer_lgi = 95,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_husqvana_lgi = 96,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hymac_lgi = 97,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hyosung = 98,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hyster = 99,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_hyundai = 100,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_imperial_lgi = 101,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_indian_lgi = 102,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_infiniti_lgi = 103,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_intercons_lgi = 104,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_international = 105,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_iseki_lgi = 106,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_isuzu = 107,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_iveco = 108,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_jac_lgi = 109,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_jackson_LGI = 110,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_jacobsen_lgi = 111,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_jaguar = 112,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_jayco_lgi = 113,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_jcb_lgi = 114,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_jeep_lgi = 115,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_jensen_lgi = 116,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_johndeere_lgi = 117,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_kato_lgi = 118,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_kawasaki = 119,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_kenworth = 120,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_kia = 121,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_kiwibus_lgi = 122,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_kobelco = 123,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_komatsu = 124,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ktm = 125,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_kubota_lgi = 126,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lada = 127,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lagonda_lgi = 128,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lamborghini = 129,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lambretta_lgi = 130,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lancia = 131,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_landini_lgi = 132,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_landrover = 133,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lanz_lgi = 134,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_leader = 135,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lees = 136,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lexus = 137,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_leyland = 138,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lincoln_lgi = 139,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_lotus = 140,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mack = 141,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mahindra_lgi = 142,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_man = 143,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_manitou_lgi = 144,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_maserati = 145,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_masseycom_lgi = 146,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_massey_lgi = 147,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_matchless_lgi = 148,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_maxitrans_LGI = 149,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_maxwell_lgi = 150,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_maybach_lgi = 151,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mazda = 152,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mccormick_lgi = 153,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mclaren_lgi = 154,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mcw_lgi = 155,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mercedes = 156,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mercury_lgi = 157,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_metro = 158,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mg = 159,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_millstui_LGI = 160,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mini_lgi = 161,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mitsubishi = 162,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_moped_lgi = 163,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_morris = 164,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_motoguzzi = 165,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mte_LGI = 166,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_munro_lgi = 167,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_musso = 168,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mv_augusta = 169,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nash_lgi = 170,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nda_LGI = 171,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_newholland_lgi = 172,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nissan = 173,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_norton = 174,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nuffield_lgi = 175,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_oldsmobile_lgi = 176,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_opel = 177,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_opinionholdings_LGI = 178,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_oshkosh_lgi = 179,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_other = 180,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_overland_lgi = 181,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pacific = 182,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_packard_lgi = 183,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_panther_lgi = 184,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_patchell_LGI = 185,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_peterbilt_lgi = 186,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_peugeot = 187,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_phillips_lgi = 188,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_piaggio = 189,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pioneer_lgi = 190,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_plymouth_lgi = 191,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_pontiac = 192,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_porsche = 193,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_premier_lgi = 194,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_prince_lgi = 195,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_proton = 196,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_raleigh_lgi = 197,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_rambler_lgi = 198,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_range_rover = 199,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_reliant_lgi = 200,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_renault = 201,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_rhino_lgi = 202,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_riley_lgi = 203,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_roadmaster_LGI = 204,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_rolls_royce = 205,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_rover = 206,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_saab = 207,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sakai_lgi = 208,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_same_lgi = 209,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_samsung_lgi = 210,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_scammel = 211,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_scania = 212,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_seat = 213,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_silver_lgi = 214,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_skoda = 215,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_smart_lgi = 216,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ssangyong = 217,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_standard_lgi = 218,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_star_lgi = 219,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_steelbro_LGI = 220,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sterling_lgi = 221,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_studebaker_lgi = 222,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_subaru = 223,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sunbeam_lgi = 224,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_suzuki = 225,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_swift_lgi = 226,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tandg_LGI = 227,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tadano_lgi = 228,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tankerengineering_LGI = 229,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tauris_lgi = 230,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tcm_lgi = 231,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tekeuchi = 232,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_terex = 233,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tesla_lgi = 234,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_timberjack_lgi = 235,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_titan_lgi = 236,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tmc_LGI = 237,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tntmotor_lgi = 238,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_toyota = 240,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tractor_lgi = 241,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_trailer = 242,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transfleet_LGI = 243,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_transporttrailers_LGI = 244,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_trike_lgi = 245,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_triton_lgi = 246,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_triumph = 247,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tvr_lgi = 248,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_udtrucks_lgi = 249,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_universal_lgi = 250,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_unknown_LGI = 251,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_valmet_ranger = 252,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_vauxhall = 253,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_vespa = 254,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_veteran_lgi = 255,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_victory_lgi = 256,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_vincent_lgi = 257,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_vintage_lgi = 258,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_volkswagen = 259,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_volvo = 260,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_wabco = 261,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_wagener = 262,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_walker_lgi = 263,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_western_star = 264,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_white = 265,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_willys_lgi = 266,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_wolseley_lgi = 267,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_yamaha = 268,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_yanmar_lgi = 269,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_yugo = 270,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ExternalVehicleType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum ExternalVehicleType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_04 = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_12 = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_09 = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_08 = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_06 = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_01 = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_10 = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_11 = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_others = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_07 = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_13 = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_03 = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_02 = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_05 = 13,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PolicyLine", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum PolicyLine : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BoatLine_LGI = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BusinessOwnersLine = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BusinessAutoLine = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LGICMVLine = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CommercialPropertyLine_LGI = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ContentsLine_LGI = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CommercialPropertyLine = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_EngineeringLine_LGI = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GeneralLiabilityLine = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_HomeLine_LGI = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_InlandMarineLine = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LiabilityLine_LGI = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MarineLine_LGI = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PersonalAutoLine = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PolicyLine = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LGIPVLine = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BulkLine_LGI = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WorkersCompLine = 17,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LiaGLPrdDetailBizType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum LiaGLPrdDetailBizType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_M = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_D = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_E = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_I = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MD = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ME = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MI = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MDE = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MDI = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MEI = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DE = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DI = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DEI = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_EI = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_All = 14,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LiaGLPrdDetailCountry_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum LiaGLPrdDetailCountry_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZ = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AUS = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SthPac = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Asia = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_UKEurope = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_USACanada = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MiddleEast = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_RestOfWorld = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CentralSouthAmerica = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Africa = 9,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ConditionTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class ConditionTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CovTermTO[] CovTermsField;
        
        private string DescriptionField;
        
        private string NameField;
        
        private string PatternCodeField;
        
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
        public EServices.SubmissionProxy.CovTermTO[] CovTerms
        {
            get
            {
                return this.CovTermsField;
            }
            set
            {
                this.CovTermsField = value;
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
        public string Name
        {
            get
            {
                return this.NameField;
            }
            set
            {
                this.NameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PatternCode
        {
            get
            {
                return this.PatternCodeField;
            }
            set
            {
                this.PatternCodeField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ExclusionTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class ExclusionTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CovTermTO[] CovTermsField;
        
        private string DescriptionField;
        
        private string NameField;
        
        private string PatternCodeField;
        
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
        public EServices.SubmissionProxy.CovTermTO[] CovTerms
        {
            get
            {
                return this.CovTermsField;
            }
            set
            {
                this.CovTermsField = value;
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
        public string Name
        {
            get
            {
                return this.NameField;
            }
            set
            {
                this.NameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PatternCode
        {
            get
            {
                return this.PatternCodeField;
            }
            set
            {
                this.PatternCodeField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LineLevelModifierTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.CommercialMotorVehicleLineLevelModifierTO))]
    public partial class LineLevelModifierTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<decimal> OverallCompanyPremiumField;
        
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
        public System.Nullable<decimal> OverallCompanyPremium
        {
            get
            {
                return this.OverallCompanyPremiumField;
            }
            set
            {
                this.OverallCompanyPremiumField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyMDBITO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyMDBITO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CommercialPropertyCoverageTO[] CoveragesField;
        
        private EServices.SubmissionProxy.CommercialPropertyLocationTO[] LocationsField;
        
        private EServices.SubmissionProxy.CommercialPropertyMDBIDetailTO MDBIDetailField;
        
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
        public EServices.SubmissionProxy.CommercialPropertyCoverageTO[] Coverages
        {
            get
            {
                return this.CoveragesField;
            }
            set
            {
                this.CoveragesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialPropertyLocationTO[] Locations
        {
            get
            {
                return this.LocationsField;
            }
            set
            {
                this.LocationsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialPropertyMDBIDetailTO MDBIDetail
        {
            get
            {
                return this.MDBIDetailField;
            }
            set
            {
                this.MDBIDetailField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyRiskHistoryTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyRiskHistoryTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CommercialPropertyClaimSummaryTO[] ClaimSummariesField;
        
        private System.Nullable<System.DateTime> DateOfClaimsReportField;
        
        private bool HasClaimsOver10000Field;
        
        private EServices.SubmissionProxy.CommercialPropertyLossDetailTO[] LossDetailsField;
        
        private System.Nullable<EServices.SubmissionProxy.YearsInBusiness_LGI> YearsInBusinessField;
        
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
        public EServices.SubmissionProxy.CommercialPropertyClaimSummaryTO[] ClaimSummaries
        {
            get
            {
                return this.ClaimSummariesField;
            }
            set
            {
                this.ClaimSummariesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> DateOfClaimsReport
        {
            get
            {
                return this.DateOfClaimsReportField;
            }
            set
            {
                this.DateOfClaimsReportField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool HasClaimsOver10000
        {
            get
            {
                return this.HasClaimsOver10000Field;
            }
            set
            {
                this.HasClaimsOver10000Field = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialPropertyLossDetailTO[] LossDetails
        {
            get
            {
                return this.LossDetailsField;
            }
            set
            {
                this.LossDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.YearsInBusiness_LGI> YearsInBusiness
        {
            get
            {
                return this.YearsInBusinessField;
            }
            set
            {
                this.YearsInBusinessField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyMDBIDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyMDBIDetailTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string BusinessDescriptionField;
        
        private System.Nullable<decimal> FSLDeclaredIndemnityValueField;
        
        private System.Nullable<bool> HasFSLDeclarationField;
        
        private bool IsBIIdenticalCoverageField;
        
        private bool IsBINaturalDisasterSelectedField;
        
        private bool IsBIOtherThanNDSelectedField;
        
        private bool IsMDIdenticalCoverageField;
        
        private bool IsMDNaturalDisasterSelectedField;
        
        private bool IsMDOtherThanNDSelectedField;
        
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
        public string BusinessDescription
        {
            get
            {
                return this.BusinessDescriptionField;
            }
            set
            {
                this.BusinessDescriptionField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> FSLDeclaredIndemnityValue
        {
            get
            {
                return this.FSLDeclaredIndemnityValueField;
            }
            set
            {
                this.FSLDeclaredIndemnityValueField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasFSLDeclaration
        {
            get
            {
                return this.HasFSLDeclarationField;
            }
            set
            {
                this.HasFSLDeclarationField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsBIIdenticalCoverage
        {
            get
            {
                return this.IsBIIdenticalCoverageField;
            }
            set
            {
                this.IsBIIdenticalCoverageField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsBINaturalDisasterSelected
        {
            get
            {
                return this.IsBINaturalDisasterSelectedField;
            }
            set
            {
                this.IsBINaturalDisasterSelectedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsBIOtherThanNDSelected
        {
            get
            {
                return this.IsBIOtherThanNDSelectedField;
            }
            set
            {
                this.IsBIOtherThanNDSelectedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsMDIdenticalCoverage
        {
            get
            {
                return this.IsMDIdenticalCoverageField;
            }
            set
            {
                this.IsMDIdenticalCoverageField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsMDNaturalDisasterSelected
        {
            get
            {
                return this.IsMDNaturalDisasterSelectedField;
            }
            set
            {
                this.IsMDNaturalDisasterSelectedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsMDOtherThanNDSelected
        {
            get
            {
                return this.IsMDOtherThanNDSelectedField;
            }
            set
            {
                this.IsMDOtherThanNDSelectedField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyCoverageTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyCoverageTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CoverageTO CoverageField;
        
        private EServices.SubmissionProxy.RateModifierTO RateModifierField;
        
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
        public EServices.SubmissionProxy.CoverageTO Coverage
        {
            get
            {
                return this.CoverageField;
            }
            set
            {
                this.CoverageField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.RateModifierTO RateModifier
        {
            get
            {
                return this.RateModifierField;
            }
            set
            {
                this.RateModifierField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyLocationTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyLocationTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CommercialPropertyLocationDetailTO CPLocationDetailField;
        
        private EServices.SubmissionProxy.CommercialPropertyCoverageTO[] CoveragesField;
        
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
        public EServices.SubmissionProxy.CommercialPropertyLocationDetailTO CPLocationDetail
        {
            get
            {
                return this.CPLocationDetailField;
            }
            set
            {
                this.CPLocationDetailField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.CommercialPropertyCoverageTO[] Coverages
        {
            get
            {
                return this.CoveragesField;
            }
            set
            {
                this.CoveragesField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CommercialPropertyClaimSummaryTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class CommercialPropertyClaimSummaryTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<EServices.SubmissionProxy.ClaimHistoryPeriod_LGI> ClaimHistoryPeriodField;
        
        private System.Nullable<decimal> CostOfLossField;
        
        private System.Nullable<int> NoOfClaimsField;
        
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
        public System.Nullable<EServices.SubmissionProxy.ClaimHistoryPeriod_LGI> ClaimHistoryPeriod
        {
            get
            {
                return this.ClaimHistoryPeriodField;
            }
            set
            {
                this.ClaimHistoryPeriodField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> CostOfLoss
        {
            get
            {
                return this.CostOfLossField;
            }
            set
            {
                this.CostOfLossField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> NoOfClaims
        {
            get
            {
                return this.NoOfClaimsField;
            }
            set
            {
                this.NoOfClaimsField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="YearsInBusiness_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum YearsInBusiness_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_0 = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_1 = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_2 = 2,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ClaimHistoryPeriod_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum ClaimHistoryPeriod_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_expiringYear = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_previousYear = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_3YearsAgo = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_4YearsAgo = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_5YearsAgo = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_6YearsAgo = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_7YearsAgo = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_8YearsAgo = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_9YearsAgo = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_10YearsAgo = 9,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="EmployersLiabilityTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class EmployersLiabilityTO : EServices.SubmissionProxy.LiabilityTO
    {
        
        private EServices.SubmissionProxy.EmployersLiabilityDetailTO EmployersLiabilityDetailField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.EmployersLiabilityDetailTO EmployersLiabilityDetail
        {
            get
            {
                return this.EmployersLiabilityDetailField;
            }
            set
            {
                this.EmployersLiabilityDetailField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="GeneralCoverDetailsTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class GeneralCoverDetailsTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string ANZSICActivityCodeField;
        
        private System.Nullable<bool> ComplianceWithLegislationField;
        
        private EServices.SubmissionProxy.GeneralCoverActivityTO[] GeneralCoverActivitiesField;
        
        private System.Nullable<System.DateTime> LastProposalDateField;
        
        private System.Nullable<int> NumberOfEmployeesField;
        
        private System.Nullable<int> NumberOfYearsInBusinessField;
        
        private string OutsideActivityDescriptionField;
        
        private System.Nullable<bool> PenaltyOrPremiumLoadingImposedField;
        
        private System.Nullable<bool> PolicyDeclinedField;
        
        private EServices.SubmissionProxy.SubContractorTO[] SubContractorsField;
        
        private System.Nullable<int> TotalNoOfClientsPerEventField;
        
        private System.Nullable<int> TotalNoOfEventsPerYearField;
        
        private System.Nullable<int> TotalNoOfParticipantsPerEventField;
        
        private System.Nullable<int> TotalNoOfVolunteersPerEventField;
        
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
        public string ANZSICActivityCode
        {
            get
            {
                return this.ANZSICActivityCodeField;
            }
            set
            {
                this.ANZSICActivityCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> ComplianceWithLegislation
        {
            get
            {
                return this.ComplianceWithLegislationField;
            }
            set
            {
                this.ComplianceWithLegislationField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.GeneralCoverActivityTO[] GeneralCoverActivities
        {
            get
            {
                return this.GeneralCoverActivitiesField;
            }
            set
            {
                this.GeneralCoverActivitiesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> LastProposalDate
        {
            get
            {
                return this.LastProposalDateField;
            }
            set
            {
                this.LastProposalDateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> NumberOfEmployees
        {
            get
            {
                return this.NumberOfEmployeesField;
            }
            set
            {
                this.NumberOfEmployeesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> NumberOfYearsInBusiness
        {
            get
            {
                return this.NumberOfYearsInBusinessField;
            }
            set
            {
                this.NumberOfYearsInBusinessField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OutsideActivityDescription
        {
            get
            {
                return this.OutsideActivityDescriptionField;
            }
            set
            {
                this.OutsideActivityDescriptionField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> PenaltyOrPremiumLoadingImposed
        {
            get
            {
                return this.PenaltyOrPremiumLoadingImposedField;
            }
            set
            {
                this.PenaltyOrPremiumLoadingImposedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> PolicyDeclined
        {
            get
            {
                return this.PolicyDeclinedField;
            }
            set
            {
                this.PolicyDeclinedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.SubContractorTO[] SubContractors
        {
            get
            {
                return this.SubContractorsField;
            }
            set
            {
                this.SubContractorsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> TotalNoOfClientsPerEvent
        {
            get
            {
                return this.TotalNoOfClientsPerEventField;
            }
            set
            {
                this.TotalNoOfClientsPerEventField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> TotalNoOfEventsPerYear
        {
            get
            {
                return this.TotalNoOfEventsPerYearField;
            }
            set
            {
                this.TotalNoOfEventsPerYearField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> TotalNoOfParticipantsPerEvent
        {
            get
            {
                return this.TotalNoOfParticipantsPerEventField;
            }
            set
            {
                this.TotalNoOfParticipantsPerEventField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> TotalNoOfVolunteersPerEvent
        {
            get
            {
                return this.TotalNoOfVolunteersPerEventField;
            }
            set
            {
                this.TotalNoOfVolunteersPerEventField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="GeneralLiabilityTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class GeneralLiabilityTO : EServices.SubmissionProxy.LiabilityTO
    {
        
        private EServices.SubmissionProxy.GeneralLiabilityDetailTO GeneralLiabilityDetailField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.GeneralLiabilityDetailTO GeneralLiabilityDetail
        {
            get
            {
                return this.GeneralLiabilityDetailField;
            }
            set
            {
                this.GeneralLiabilityDetailField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="StatutoryLiabilityTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class StatutoryLiabilityTO : EServices.SubmissionProxy.LiabilityTO
    {
        
        private EServices.SubmissionProxy.StatutoryLiabilityDetailTO StatutoryLiabilityDetailField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.StatutoryLiabilityDetailTO StatutoryLiabilityDetail
        {
            get
            {
                return this.StatutoryLiabilityDetailField;
            }
            set
            {
                this.StatutoryLiabilityDetailField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LiabilityTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.StatutoryLiabilityTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.GeneralLiabilityTO))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.EmployersLiabilityTO))]
    public partial class LiabilityTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.CoverageTO[] CoveragesField;
        
        private EServices.SubmissionProxy.DollarModifierTO DollarModifierField;
        
        private EServices.SubmissionProxy.PercentageModifierTO PercentageModifierField;
        
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
        public EServices.SubmissionProxy.CoverageTO[] Coverages
        {
            get
            {
                return this.CoveragesField;
            }
            set
            {
                this.CoveragesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.DollarModifierTO DollarModifier
        {
            get
            {
                return this.DollarModifierField;
            }
            set
            {
                this.DollarModifierField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.PercentageModifierTO PercentageModifier
        {
            get
            {
                return this.PercentageModifierField;
            }
            set
            {
                this.PercentageModifierField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="EmployersLiabilityDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class EmployersLiabilityDetailTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<System.DateTime> RetroactiveDateField;
        
        private System.Nullable<EServices.SubmissionProxy.RetroactiveType_LGI> RetroactiveTypeField;
        
        private System.Nullable<EServices.SubmissionProxy.CountryLimit_LGI> TerritorialLimitsField;
        
        private string TerritorialLimitsOtherField;
        
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
        public System.Nullable<System.DateTime> RetroactiveDate
        {
            get
            {
                return this.RetroactiveDateField;
            }
            set
            {
                this.RetroactiveDateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.RetroactiveType_LGI> RetroactiveType
        {
            get
            {
                return this.RetroactiveTypeField;
            }
            set
            {
                this.RetroactiveTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CountryLimit_LGI> TerritorialLimits
        {
            get
            {
                return this.TerritorialLimitsField;
            }
            set
            {
                this.TerritorialLimitsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string TerritorialLimitsOther
        {
            get
            {
                return this.TerritorialLimitsOtherField;
            }
            set
            {
                this.TerritorialLimitsOtherField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="StatutoryLiabilityDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class StatutoryLiabilityDetailTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<System.DateTime> RetroactiveDateField;
        
        private System.Nullable<EServices.SubmissionProxy.RetroactiveType_LGI> RetroactiveTypeField;
        
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
        public System.Nullable<System.DateTime> RetroactiveDate
        {
            get
            {
                return this.RetroactiveDateField;
            }
            set
            {
                this.RetroactiveDateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.RetroactiveType_LGI> RetroactiveType
        {
            get
            {
                return this.RetroactiveTypeField;
            }
            set
            {
                this.RetroactiveTypeField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="RetroactiveType_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum RetroactiveType_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Date = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Unlimited = 1,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="GeneralLiabilityDetailTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class GeneralLiabilityDetailTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<decimal> AnnualVehicleServiceTurnoverField;
        
        private System.Nullable<decimal> AnnualWatercraftServTurnoverField;
        
        private System.Nullable<bool> CarsAndMotorcyclesSelectedField;
        
        private System.Nullable<bool> HasAircraftPartsField;
        
        private System.Nullable<bool> HasChargeForBusinessAdviceOrServiceField;
        
        private System.Nullable<bool> HasChargeForPropertyOfOthersInControlField;
        
        private System.Nullable<bool> HasChemicalsProductField;
        
        private System.Nullable<bool> HasContractualLiabilityField;
        
        private System.Nullable<bool> HasDesignTheProductField;
        
        private System.Nullable<bool> HasEthicalDrugsField;
        
        private System.Nullable<bool> HasExportProductsField;
        
        private System.Nullable<bool> HasFertilisersField;
        
        private System.Nullable<bool> HasHazardousGoodsField;
        
        private System.Nullable<bool> HasMaintainQAOrRecordSysField;
        
        private System.Nullable<bool> HasPropertyOfOthersInControlField;
        
        private System.Nullable<bool> HasProvidedBusinessAdviceOrServiceField;
        
        private System.Nullable<bool> HasRadioactiveMaterialField;
        
        private System.Nullable<bool> HasWatercraftExceed8mField;
        
        private System.Nullable<bool> HasWithdrawnProductField;
        
        private EServices.SubmissionProxy.HazardousSubstancesTO[] HazardousSubstancesField;
        
        private System.Nullable<bool> HazardousSubstancesSubComplyWithLawsField;
        
        private System.Nullable<bool> HeavyTrucksSelectedField;
        
        private EServices.SubmissionProxy.HotworkTO[] HotworksField;
        
        private System.Nullable<EServices.SubmissionProxy.CountryLimit_LGI> JurisdictionalLimitsField;
        
        private string JurisdictionalLimitsOtherField;
        
        private System.Nullable<bool> LightTrucksAndVansSelectedField;
        
        private System.Nullable<bool> MobilePlantAndMachySelectedField;
        
        private System.Nullable<int> NumberOfLocationsField;
        
        private string OtherVehicleDetailsField;
        
        private System.Nullable<bool> OtherVehicleSelectedField;
        
        private System.Nullable<bool> PerfAndRacingCarsSelectedField;
        
        private EServices.SubmissionProxy.ProductDetailTO[] ProductDetailsField;
        
        private System.Nullable<bool> ServiceAndRepairMotorVehiclesField;
        
        private System.Nullable<bool> ServiceAndRepairWatercraftField;
        
        private System.Nullable<EServices.SubmissionProxy.CountryLimit_LGI> TerritorialLimitsField;
        
        private string TerritorialLimitsOtherField;
        
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
        public System.Nullable<decimal> AnnualVehicleServiceTurnover
        {
            get
            {
                return this.AnnualVehicleServiceTurnoverField;
            }
            set
            {
                this.AnnualVehicleServiceTurnoverField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<decimal> AnnualWatercraftServTurnover
        {
            get
            {
                return this.AnnualWatercraftServTurnoverField;
            }
            set
            {
                this.AnnualWatercraftServTurnoverField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> CarsAndMotorcyclesSelected
        {
            get
            {
                return this.CarsAndMotorcyclesSelectedField;
            }
            set
            {
                this.CarsAndMotorcyclesSelectedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasAircraftParts
        {
            get
            {
                return this.HasAircraftPartsField;
            }
            set
            {
                this.HasAircraftPartsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasChargeForBusinessAdviceOrService
        {
            get
            {
                return this.HasChargeForBusinessAdviceOrServiceField;
            }
            set
            {
                this.HasChargeForBusinessAdviceOrServiceField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasChargeForPropertyOfOthersInControl
        {
            get
            {
                return this.HasChargeForPropertyOfOthersInControlField;
            }
            set
            {
                this.HasChargeForPropertyOfOthersInControlField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasChemicalsProduct
        {
            get
            {
                return this.HasChemicalsProductField;
            }
            set
            {
                this.HasChemicalsProductField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasContractualLiability
        {
            get
            {
                return this.HasContractualLiabilityField;
            }
            set
            {
                this.HasContractualLiabilityField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasDesignTheProduct
        {
            get
            {
                return this.HasDesignTheProductField;
            }
            set
            {
                this.HasDesignTheProductField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasEthicalDrugs
        {
            get
            {
                return this.HasEthicalDrugsField;
            }
            set
            {
                this.HasEthicalDrugsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasExportProducts
        {
            get
            {
                return this.HasExportProductsField;
            }
            set
            {
                this.HasExportProductsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasFertilisers
        {
            get
            {
                return this.HasFertilisersField;
            }
            set
            {
                this.HasFertilisersField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasHazardousGoods
        {
            get
            {
                return this.HasHazardousGoodsField;
            }
            set
            {
                this.HasHazardousGoodsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasMaintainQAOrRecordSys
        {
            get
            {
                return this.HasMaintainQAOrRecordSysField;
            }
            set
            {
                this.HasMaintainQAOrRecordSysField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasPropertyOfOthersInControl
        {
            get
            {
                return this.HasPropertyOfOthersInControlField;
            }
            set
            {
                this.HasPropertyOfOthersInControlField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasProvidedBusinessAdviceOrService
        {
            get
            {
                return this.HasProvidedBusinessAdviceOrServiceField;
            }
            set
            {
                this.HasProvidedBusinessAdviceOrServiceField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasRadioactiveMaterial
        {
            get
            {
                return this.HasRadioactiveMaterialField;
            }
            set
            {
                this.HasRadioactiveMaterialField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasWatercraftExceed8m
        {
            get
            {
                return this.HasWatercraftExceed8mField;
            }
            set
            {
                this.HasWatercraftExceed8mField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HasWithdrawnProduct
        {
            get
            {
                return this.HasWithdrawnProductField;
            }
            set
            {
                this.HasWithdrawnProductField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.HazardousSubstancesTO[] HazardousSubstances
        {
            get
            {
                return this.HazardousSubstancesField;
            }
            set
            {
                this.HazardousSubstancesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HazardousSubstancesSubComplyWithLaws
        {
            get
            {
                return this.HazardousSubstancesSubComplyWithLawsField;
            }
            set
            {
                this.HazardousSubstancesSubComplyWithLawsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> HeavyTrucksSelected
        {
            get
            {
                return this.HeavyTrucksSelectedField;
            }
            set
            {
                this.HeavyTrucksSelectedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.HotworkTO[] Hotworks
        {
            get
            {
                return this.HotworksField;
            }
            set
            {
                this.HotworksField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CountryLimit_LGI> JurisdictionalLimits
        {
            get
            {
                return this.JurisdictionalLimitsField;
            }
            set
            {
                this.JurisdictionalLimitsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string JurisdictionalLimitsOther
        {
            get
            {
                return this.JurisdictionalLimitsOtherField;
            }
            set
            {
                this.JurisdictionalLimitsOtherField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> LightTrucksAndVansSelected
        {
            get
            {
                return this.LightTrucksAndVansSelectedField;
            }
            set
            {
                this.LightTrucksAndVansSelectedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> MobilePlantAndMachySelected
        {
            get
            {
                return this.MobilePlantAndMachySelectedField;
            }
            set
            {
                this.MobilePlantAndMachySelectedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> NumberOfLocations
        {
            get
            {
                return this.NumberOfLocationsField;
            }
            set
            {
                this.NumberOfLocationsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OtherVehicleDetails
        {
            get
            {
                return this.OtherVehicleDetailsField;
            }
            set
            {
                this.OtherVehicleDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> OtherVehicleSelected
        {
            get
            {
                return this.OtherVehicleSelectedField;
            }
            set
            {
                this.OtherVehicleSelectedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> PerfAndRacingCarsSelected
        {
            get
            {
                return this.PerfAndRacingCarsSelectedField;
            }
            set
            {
                this.PerfAndRacingCarsSelectedField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public EServices.SubmissionProxy.ProductDetailTO[] ProductDetails
        {
            get
            {
                return this.ProductDetailsField;
            }
            set
            {
                this.ProductDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> ServiceAndRepairMotorVehicles
        {
            get
            {
                return this.ServiceAndRepairMotorVehiclesField;
            }
            set
            {
                this.ServiceAndRepairMotorVehiclesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> ServiceAndRepairWatercraft
        {
            get
            {
                return this.ServiceAndRepairWatercraftField;
            }
            set
            {
                this.ServiceAndRepairWatercraftField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.CountryLimit_LGI> TerritorialLimits
        {
            get
            {
                return this.TerritorialLimitsField;
            }
            set
            {
                this.TerritorialLimitsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string TerritorialLimitsOther
        {
            get
            {
                return this.TerritorialLimitsOtherField;
            }
            set
            {
                this.TerritorialLimitsOtherField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CountryLimit_LGI", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum CountryLimit_LGI : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZ = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZAus = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZPacExclUSACanada = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Fiji = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WorldwideExclUSACanada = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Worldwide = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Aus = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZAsiaPacExclUSACanada = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZAusAsiaPacExclUSACanada = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZPac = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZAsiaPac = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZAusAsiaPac = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_Other = 12,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="BindSubmissionRequestTO", Namespace="urn:nz.co.lumley:eservices:entity:20130711")]
    public partial class BindSubmissionRequestTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.ClosingSlipTO[] ClosingSlipsField;
        
        private string SubmissionNumberField;
        
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
        public string SubmissionNumber
        {
            get
            {
                return this.SubmissionNumberField;
            }
            set
            {
                this.SubmissionNumberField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ClosingSlipTO", Namespace="urn:nz.co.lumley:eservices:entity:20130711")]
    public partial class ClosingSlipTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string ClosingSlipNumberField;
        
        private System.Nullable<EServices.SubmissionProxy.PolicyLine> PolicyLineField;
        
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
        public string ClosingSlipNumber
        {
            get
            {
                return this.ClosingSlipNumberField;
            }
            set
            {
                this.ClosingSlipNumberField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.PolicyLine> PolicyLine
        {
            get
            {
                return this.PolicyLineField;
            }
            set
            {
                this.PolicyLineField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="EditSubmissionRequestTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class EditSubmissionRequestTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<System.DateTime> DateQuoteNeededField;
        
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
        
        private EServices.SubmissionProxy.ProductSelectionTO ProductSelectionField;
        
        private System.Nullable<EServices.SubmissionProxy.QuoteType> QuoteTypeField;
        
        private string SubmissionNumberField;
        
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
        public System.Nullable<System.DateTime> DateQuoteNeeded
        {
            get
            {
                return this.DateQuoteNeededField;
            }
            set
            {
                this.DateQuoteNeededField = value;
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
        public EServices.SubmissionProxy.ProductSelectionTO ProductSelection
        {
            get
            {
                return this.ProductSelectionField;
            }
            set
            {
                this.ProductSelectionField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.QuoteType> QuoteType
        {
            get
            {
                return this.QuoteTypeField;
            }
            set
            {
                this.QuoteTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string SubmissionNumber
        {
            get
            {
                return this.SubmissionNumberField;
            }
            set
            {
                this.SubmissionNumberField = value;
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
    [System.Runtime.Serialization.DataContractAttribute(Name="NotTakeSubmissionRequestTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class NotTakeSubmissionRequestTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<EServices.SubmissionProxy.OtherInsurer_LGI> OtherInsurerField;
        
        private System.Nullable<EServices.SubmissionProxy.ReasonCode> ReasonCodeField;
        
        private string ReasonDetailsField;
        
        private string SubmissionNumberField;
        
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
        public System.Nullable<EServices.SubmissionProxy.OtherInsurer_LGI> OtherInsurer
        {
            get
            {
                return this.OtherInsurerField;
            }
            set
            {
                this.OtherInsurerField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<EServices.SubmissionProxy.ReasonCode> ReasonCode
        {
            get
            {
                return this.ReasonCodeField;
            }
            set
            {
                this.ReasonCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ReasonDetails
        {
            get
            {
                return this.ReasonDetailsField;
            }
            set
            {
                this.ReasonDetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string SubmissionNumber
        {
            get
            {
                return this.SubmissionNumberField;
            }
            set
            {
                this.SubmissionNumberField = value;
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
    [System.Runtime.Serialization.DataContractAttribute(Name="ReasonCode", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum ReasonCode : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_businessSoldOrClosed_lgi = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_cancel = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_changedEmployment_lgi = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_changeInCoverReq_lgi = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_changeOfBroker_lgi = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_condemn = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_createderrdup_lgi = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_criminal = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_deceased_lgi = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_deleteGroup_lgi = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_deleteVehicle_lgi = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_difflumleysubbound_lgi = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dissatisfiedWithClaims_lgi = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dissatisfiedWithSvc_lgi = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ProdRequirements = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_dualInsurance_lgi = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_failsafe = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_failterm = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_failcoop = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fraud = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_freeLook_lgi = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_goneIntoGlobalAcct_lgi = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_goneOverseas_lgi = 22,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_fincononpay = 23,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_noc = 24,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_insuredElsewhere_lgi = 25,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_issuedInError_lgi = 26,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LossHistory = 27,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_reinsurance = 28,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_noemployee = 29,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_eligibility = 30,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_noLongerRequired_lgi = 31,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nondisclose = 32,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nonpayment = 33,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nonreport = 34,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_OpsChars = 35,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_otherdissatisfaction_lgi = 36,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sold = 37,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_wrapup = 38,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PaymentHistory = 39,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nottaken = 40,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_policyReplaced_lgi = 41,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_midtermrewrite = 42,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_flatrewrite = 43,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_policyterms_lgi = 44,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_premium_lgi = 45,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ProductsChars = 46,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CovsNotAvailable = 47,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_InfoNotProvided = 48,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_sold_lgi = 49,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_riskchange = 50,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_suspension = 51,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_tooDear_lgi = 52,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_totalLoss_lgi = 53,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_uwreasons = 54,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_unknown_lgi = 55,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_vacant = 56,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_violation = 57,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="WithdrawSubmissionRequestTO", Namespace="urn:nz.co.lumley:eservices:entity:20131111")]
    public partial class WithdrawSubmissionRequestTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Nullable<EServices.SubmissionProxy.ReasonCode> ReasonCodeField;
        
        private string SubmissionNumberField;
        
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
        public System.Nullable<EServices.SubmissionProxy.ReasonCode> ReasonCode
        {
            get
            {
                return this.ReasonCodeField;
            }
            set
            {
                this.ReasonCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string SubmissionNumber
        {
            get
            {
                return this.SubmissionNumberField;
            }
            set
            {
                this.SubmissionNumberField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="BindSubsetOfSubmissionLinesRequestTO", Namespace="urn:nz.co.lumley:eservices:entity:20130711")]
    public partial class BindSubsetOfSubmissionLinesRequestTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.SubmissionProxy.ClosingSlipTO[] ClosingSlipsField;
        
        private EServices.SubmissionProxy.SubmissionLine[] LinesToExcludeField;
        
        private string SubmissionNumberField;
        
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
        public EServices.SubmissionProxy.SubmissionLine[] LinesToExclude
        {
            get
            {
                return this.LinesToExcludeField;
            }
            set
            {
                this.LinesToExcludeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string SubmissionNumber
        {
            get
            {
                return this.SubmissionNumberField;
            }
            set
            {
                this.SubmissionNumberField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SubmissionLine", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum SubmissionLine : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        CMV = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        MD = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        BI = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        GL = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        SL = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        EL = 5,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SOAPException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.SOAPSenderException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.PermissionException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.ValidationException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.SOAPServerException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.ServerStateException))]
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
    [System.Runtime.Serialization.DataContractAttribute(Name="SOAPSenderException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.PermissionException))]
    public partial class SOAPSenderException : EServices.SubmissionProxy.SOAPException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    public partial class PermissionException : EServices.SubmissionProxy.SOAPSenderException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    public partial class ValidationException : EServices.SubmissionProxy.SOAPException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SOAPServerException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.SubmissionProxy.ServerStateException))]
    public partial class SOAPServerException : EServices.SubmissionProxy.SOAPException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    public partial class ServerStateException : EServices.SubmissionProxy.SOAPServerException
    {
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="urn:nz.co.lumley:eservices:20131111", ConfigurationName="EServices.SubmissionProxy.ISubmissionService")]
    public interface ISubmissionService
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/QuoteSubmission", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/QuoteSubmissionResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ServerStateException), Action="", Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.PermissionException), Action="", Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ValidationException), Action="", Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        void QuoteSubmission(EServices.SubmissionProxy.QuoteSubmissionRequestTO quoteSubmissionRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/QuoteSubmission", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/QuoteSubmissionResponse")]
        System.Threading.Tasks.Task QuoteSubmissionAsync(EServices.SubmissionProxy.QuoteSubmissionRequestTO quoteSubmissionRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/BindSubmission", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/BindSubmissionResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ServerStateException), Action="", Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.PermissionException), Action="", Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ValidationException), Action="", Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        void BindSubmission(EServices.SubmissionProxy.BindSubmissionRequestTO bindSubmissionRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/BindSubmission", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/BindSubmissionResponse")]
        System.Threading.Tasks.Task BindSubmissionAsync(EServices.SubmissionProxy.BindSubmissionRequestTO bindSubmissionRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/EditSubmission", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/EditSubmissionResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ServerStateException), Action="", Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.PermissionException), Action="", Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ValidationException), Action="", Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        void EditSubmission(EServices.SubmissionProxy.EditSubmissionRequestTO editSubmissionRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/EditSubmission", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/EditSubmissionResponse")]
        System.Threading.Tasks.Task EditSubmissionAsync(EServices.SubmissionProxy.EditSubmissionRequestTO editSubmissionRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/NotTakeSubmission", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/NotTakeSubmissionResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ValidationException), Action="", Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ServerStateException), Action="", Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.PermissionException), Action="", Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        void NotTakeSubmission(EServices.SubmissionProxy.NotTakeSubmissionRequestTO notTakeSubmissionRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/NotTakeSubmission", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/NotTakeSubmissionResponse")]
        System.Threading.Tasks.Task NotTakeSubmissionAsync(EServices.SubmissionProxy.NotTakeSubmissionRequestTO notTakeSubmissionRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/WithdrawSubmission", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/WithdrawSubmissionResponse" +
                                                        "")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ValidationException), Action="", Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ServerStateException), Action="", Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.PermissionException), Action="", Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        void WithdrawSubmission(EServices.SubmissionProxy.WithdrawSubmissionRequestTO withdrawSubmissionRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/WithdrawSubmission", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/WithdrawSubmissionResponse" +
                                                        "")]
        System.Threading.Tasks.Task WithdrawSubmissionAsync(EServices.SubmissionProxy.WithdrawSubmissionRequestTO withdrawSubmissionRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/BindSubsetOfSubmissionLine" +
                                                        "s", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/BindSubsetOfSubmissionLine" +
                                                        "sResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ServerStateException), Action="", Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.PermissionException), Action="", Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.SubmissionProxy.ValidationException), Action="", Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        void BindSubsetOfSubmissionLines(EServices.SubmissionProxy.BindSubsetOfSubmissionLinesRequestTO bindSubsetOfSubmissionLinesRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20131111/ISubmissionService/BindSubsetOfSubmissionLine" +
                                                        "s", ReplyAction="urn:nz.co.lumley:eservices:20131111/ISubmissionService/BindSubsetOfSubmissionLine" +
                                                        "sResponse")]
        System.Threading.Tasks.Task BindSubsetOfSubmissionLinesAsync(EServices.SubmissionProxy.BindSubsetOfSubmissionLinesRequestTO bindSubsetOfSubmissionLinesRequest);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ISubmissionServiceChannel : EServices.SubmissionProxy.ISubmissionService, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SubmissionServiceClient : System.ServiceModel.ClientBase<EServices.SubmissionProxy.ISubmissionService>, EServices.SubmissionProxy.ISubmissionService
    {
        
        public SubmissionServiceClient()
        {
        }
        
        public SubmissionServiceClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
        {
        }
        
        public SubmissionServiceClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public SubmissionServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public SubmissionServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
        {
        }
        
        public void QuoteSubmission(EServices.SubmissionProxy.QuoteSubmissionRequestTO quoteSubmissionRequest)
        {
            base.Channel.QuoteSubmission(quoteSubmissionRequest);
        }
        
        public System.Threading.Tasks.Task QuoteSubmissionAsync(EServices.SubmissionProxy.QuoteSubmissionRequestTO quoteSubmissionRequest)
        {
            return base.Channel.QuoteSubmissionAsync(quoteSubmissionRequest);
        }
        
        public void BindSubmission(EServices.SubmissionProxy.BindSubmissionRequestTO bindSubmissionRequest)
        {
            base.Channel.BindSubmission(bindSubmissionRequest);
        }
        
        public System.Threading.Tasks.Task BindSubmissionAsync(EServices.SubmissionProxy.BindSubmissionRequestTO bindSubmissionRequest)
        {
            return base.Channel.BindSubmissionAsync(bindSubmissionRequest);
        }
        
        public void EditSubmission(EServices.SubmissionProxy.EditSubmissionRequestTO editSubmissionRequest)
        {
            base.Channel.EditSubmission(editSubmissionRequest);
        }
        
        public System.Threading.Tasks.Task EditSubmissionAsync(EServices.SubmissionProxy.EditSubmissionRequestTO editSubmissionRequest)
        {
            return base.Channel.EditSubmissionAsync(editSubmissionRequest);
        }
        
        public void NotTakeSubmission(EServices.SubmissionProxy.NotTakeSubmissionRequestTO notTakeSubmissionRequest)
        {
            base.Channel.NotTakeSubmission(notTakeSubmissionRequest);
        }
        
        public System.Threading.Tasks.Task NotTakeSubmissionAsync(EServices.SubmissionProxy.NotTakeSubmissionRequestTO notTakeSubmissionRequest)
        {
            return base.Channel.NotTakeSubmissionAsync(notTakeSubmissionRequest);
        }
        
        public void WithdrawSubmission(EServices.SubmissionProxy.WithdrawSubmissionRequestTO withdrawSubmissionRequest)
        {
            base.Channel.WithdrawSubmission(withdrawSubmissionRequest);
        }
        
        public System.Threading.Tasks.Task WithdrawSubmissionAsync(EServices.SubmissionProxy.WithdrawSubmissionRequestTO withdrawSubmissionRequest)
        {
            return base.Channel.WithdrawSubmissionAsync(withdrawSubmissionRequest);
        }
        
        public void BindSubsetOfSubmissionLines(EServices.SubmissionProxy.BindSubsetOfSubmissionLinesRequestTO bindSubsetOfSubmissionLinesRequest)
        {
            base.Channel.BindSubsetOfSubmissionLines(bindSubsetOfSubmissionLinesRequest);
        }
        
        public System.Threading.Tasks.Task BindSubsetOfSubmissionLinesAsync(EServices.SubmissionProxy.BindSubsetOfSubmissionLinesRequestTO bindSubsetOfSubmissionLinesRequest)
        {
            return base.Channel.BindSubsetOfSubmissionLinesAsync(bindSubsetOfSubmissionLinesRequest);
        }
    }
}
