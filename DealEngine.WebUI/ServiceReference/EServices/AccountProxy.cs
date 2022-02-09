
namespace EServices.AccountProxy
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="GetAccountRequestTO", Namespace="urn:nz.co.lumley:eservices:entity:20130618")]
    public partial class GetAccountRequestTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.AccountProxy.AccountTO AccountField;
        
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
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public EServices.AccountProxy.AccountTO Account
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
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="AccountTO", Namespace="urn:nz.co.lumley:eservices:entity:20130618")]
    public partial class AccountTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string ExternalAccountIDField;
        
        private EServices.AccountProxy.ContactTO ContactTOField;
        
        private string ProducerCodeField;
        
        private string ANZSICActivityCodeField;
        
        private System.Nullable<EServices.AccountProxy.AccountOrgType> AccountOrgTypeField;
        
        private string BusOpsDescField;
        
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
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
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
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=1)]
        public EServices.AccountProxy.ContactTO ContactTO
        {
            get
            {
                return this.ContactTOField;
            }
            set
            {
                this.ContactTOField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=2)]
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
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=3)]
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
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=4)]
        public System.Nullable<EServices.AccountProxy.AccountOrgType> AccountOrgType
        {
            get
            {
                return this.AccountOrgTypeField;
            }
            set
            {
                this.AccountOrgTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=5)]
        public string BusOpsDesc
        {
            get
            {
                return this.BusOpsDescField;
            }
            set
            {
                this.BusOpsDescField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=6)]
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
    [System.Runtime.Serialization.DataContractAttribute(Name="ContactTO", Namespace="urn:nz.co.lumley:eservices:entity:20130618")]
    public partial class ContactTO : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private EServices.AccountProxy.ContactType AccountSubTypeField;
        
        private string OrganizationNameField;
        
        private string CityField;
        
        private string AddressLine1Field;
        
        private string AddressLine2Field;
        
        private string SuburbField;
        
        private string AddressDescriptionField;
        
        private System.Nullable<EServices.AccountProxy.Country> CountryField;
        
        private System.Nullable<EServices.AccountProxy.AddressType> AddressTypeField;
        
        private string TradingAsNameField;
        
        private string AlternativeEmailField;
        
        private string BusinessPhoneField;
        
        private string CellPhoneField;
        
        private string FaxNumberField;
        
        private string GSTNumberField;
        
        private string HomePhoneField;
        
        private string MainEmailField;
        
        private string PostCodeField;
        
        private string PrimaryPhoneField;
        
        private System.Nullable<EServices.AccountProxy.PrimaryPhoneType> PrimaryPhoneChoiceField;
        
        private System.Nullable<EServices.AccountProxy.State> StateField;
        
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
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public EServices.AccountProxy.ContactType AccountSubType
        {
            get
            {
                return this.AccountSubTypeField;
            }
            set
            {
                this.AccountSubTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public string OrganizationName
        {
            get
            {
                return this.OrganizationNameField;
            }
            set
            {
                this.OrganizationNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=2)]
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
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=3)]
        public string AddressLine1
        {
            get
            {
                return this.AddressLine1Field;
            }
            set
            {
                this.AddressLine1Field = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=4)]
        public string AddressLine2
        {
            get
            {
                return this.AddressLine2Field;
            }
            set
            {
                this.AddressLine2Field = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=5)]
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
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=6)]
        public string AddressDescription
        {
            get
            {
                return this.AddressDescriptionField;
            }
            set
            {
                this.AddressDescriptionField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=7)]
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
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=8)]
        public System.Nullable<EServices.AccountProxy.AddressType> AddressType
        {
            get
            {
                return this.AddressTypeField;
            }
            set
            {
                this.AddressTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=9)]
        public string TradingAsName
        {
            get
            {
                return this.TradingAsNameField;
            }
            set
            {
                this.TradingAsNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=10)]
        public string AlternativeEmail
        {
            get
            {
                return this.AlternativeEmailField;
            }
            set
            {
                this.AlternativeEmailField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=11)]
        public string BusinessPhone
        {
            get
            {
                return this.BusinessPhoneField;
            }
            set
            {
                this.BusinessPhoneField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=12)]
        public string CellPhone
        {
            get
            {
                return this.CellPhoneField;
            }
            set
            {
                this.CellPhoneField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=13)]
        public string FaxNumber
        {
            get
            {
                return this.FaxNumberField;
            }
            set
            {
                this.FaxNumberField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=14)]
        public string GSTNumber
        {
            get
            {
                return this.GSTNumberField;
            }
            set
            {
                this.GSTNumberField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=15)]
        public string HomePhone
        {
            get
            {
                return this.HomePhoneField;
            }
            set
            {
                this.HomePhoneField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=16)]
        public string MainEmail
        {
            get
            {
                return this.MainEmailField;
            }
            set
            {
                this.MainEmailField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=17)]
        public string PostCode
        {
            get
            {
                return this.PostCodeField;
            }
            set
            {
                this.PostCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=18)]
        public string PrimaryPhone
        {
            get
            {
                return this.PrimaryPhoneField;
            }
            set
            {
                this.PrimaryPhoneField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=19)]
        public System.Nullable<EServices.AccountProxy.PrimaryPhoneType> PrimaryPhoneChoice
        {
            get
            {
                return this.PrimaryPhoneChoiceField;
            }
            set
            {
                this.PrimaryPhoneChoiceField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=20)]
        public System.Nullable<EServices.AccountProxy.State> State
        {
            get
            {
                return this.StateField;
            }
            set
            {
                this.StateField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="AccountOrgType", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum AccountOrgType : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_branchesincoverseas_lgi = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_centralgovt_lgi = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_charitabletrusts_lgi = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_consulatesandembassies_lgi = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_coopcompanies_lgi = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_government = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_incandunincsocandassoc_lgi = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_individualpropship_lgi = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_jointventure = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_localauthoritytradingent_lgi = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_localgovt_lgi = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_other = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_partnership = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_company = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_individual = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_trustestate = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_commonownership = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_privatecorp = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_corporation = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_executortrustee = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_limitedpartnership = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_llc = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_llp = 22,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_nonprofit = 23,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_religious = 24,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_solepropship = 25,
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
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="AddressType", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum AddressType : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_billing = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_business = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_home = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_other = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PrimaryPhoneType", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum PrimaryPhoneType : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_work = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_home = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_mobile = 2,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="State", Namespace="urn:nz.co.lumley:eservices:enumeration:20130522")]
    public enum State : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NZ = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AL = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AK = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AB = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AZ = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_AR = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_BC = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CA = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CO = 8,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_CT = 9,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DE = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_DC = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_FM = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_FL = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GA = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_GU = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_HI = 16,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ID = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IL = 18,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IN = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_IA = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KS = 21,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_KY = 22,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_LA = 23,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ME = 24,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MB = 25,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MH = 26,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MD = 27,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MA = 28,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MI = 29,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MN = 30,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MS = 31,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MO = 32,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MT = 33,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NE = 34,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NV = 35,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NB = 36,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NL = 37,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NH = 38,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NJ = 39,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NM = 40,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NY = 41,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NC = 42,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ND = 43,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_MP = 44,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NT = 45,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NS = 46,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_NU = 47,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_OH = 48,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_OK = 49,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_ON = 50,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_OR = 51,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PA = 52,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PE = 53,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_PR = 54,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_QC = 55,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_RI = 56,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SK = 57,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SC = 58,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_SD = 59,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TN = 60,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_TX = 61,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_UT = 62,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_VT = 63,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_VA = 64,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_VI = 65,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WA = 66,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WV = 67,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WI = 68,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_WY = 69,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TC_YT = 70,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SOAPException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.AccountProxy.SOAPSenderException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.AccountProxy.PermissionException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.AccountProxy.ValidationException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.AccountProxy.SOAPServerException))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.AccountProxy.ServerStateException))]
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
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.AccountProxy.PermissionException))]
    public partial class SOAPSenderException : EServices.AccountProxy.SOAPException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    public partial class PermissionException : EServices.AccountProxy.SOAPSenderException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    public partial class ValidationException : EServices.AccountProxy.SOAPException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SOAPServerException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EServices.AccountProxy.ServerStateException))]
    public partial class SOAPServerException : EServices.AccountProxy.SOAPException
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
    public partial class ServerStateException : EServices.AccountProxy.SOAPServerException
    {
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="urn:nz.co.lumley:eservices:20130618", ConfigurationName="EServices.AccountProxy.IAccountService")]
    public interface IAccountService
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20130618/IAccountService/GetAccount", ReplyAction="urn:nz.co.lumley:eservices:20130618/IAccountService/GetAccountResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.AccountProxy.ServerStateException), Action="", Name="ServerStateException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.AccountProxy.PermissionException), Action="", Name="PermissionException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        [System.ServiceModel.FaultContractAttribute(typeof(EServices.AccountProxy.ValidationException), Action="", Name="ValidationException", Namespace="urn:nz.co.lumley:eservices:fault:20130510")]
        void GetAccount(EServices.AccountProxy.GetAccountRequestTO getAccountRequest);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:nz.co.lumley:eservices:20130618/IAccountService/GetAccount", ReplyAction="urn:nz.co.lumley:eservices:20130618/IAccountService/GetAccountResponse")]
        System.Threading.Tasks.Task GetAccountAsync(EServices.AccountProxy.GetAccountRequestTO getAccountRequest);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IAccountServiceChannel : EServices.AccountProxy.IAccountService, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class AccountServiceClient : System.ServiceModel.ClientBase<EServices.AccountProxy.IAccountService>, EServices.AccountProxy.IAccountService
    {
        
        public AccountServiceClient()
        {
        }
        
        public AccountServiceClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
        {
        }
        
        public AccountServiceClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public AccountServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public AccountServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
        {
        }
        
        public void GetAccount(EServices.AccountProxy.GetAccountRequestTO getAccountRequest)
        {
            base.Channel.GetAccount(getAccountRequest);
        }
        
        public System.Threading.Tasks.Task GetAccountAsync(EServices.AccountProxy.GetAccountRequestTO getAccountRequest)
        {
            return base.Channel.GetAccountAsync(getAccountRequest);
        }
    }
}

