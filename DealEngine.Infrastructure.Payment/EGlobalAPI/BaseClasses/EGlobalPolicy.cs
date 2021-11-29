
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DealEngine.Domain.Entities;

namespace DealEngine.Infrastructure.Payment.EGlobalAPI.BaseClasses
{
    [XmlRoot("Cover")]
    public class EGlobalPolicy
    {
        [XmlIgnore]
        public ClientProgramme ClientProgramme { get; set; }
        [XmlIgnore]
        public Package Package { get; set; }
        [XmlIgnore]
        public EGlobalPolicyConfig EGlobalPolicyConfig { get; set; }
        [XmlIgnore]
        public List<EGlobalPolicyRiskConfig> EGlobalPolicyRiskConfig { get; set; }
        [XmlIgnore]
        public Guid EGlobalPolicyId { get; set; }
        [XmlIgnore]
        public decimal DiscountRate { get; set; }
        [XmlIgnore]
        protected string Description1;
        [XmlIgnore]
        protected string Description2;
        [XmlIgnore]
        public decimal PremiumFunding { get; set; }
        [XmlIgnore]
        public int PolicyReference { get; set; }
        [XmlIgnore]
        public decimal PaymentDirection = 1;
        [XmlIgnore]
        public decimal SurchargeRate { get; set; }
        [XmlIgnore]
        protected decimal BrokerFee = -1;
        [XmlIgnore]
        public string PaymentType;


        /// <summary>
		/// Returns true if we are running on a Unix style OS (including Linux and OS X), and false if Windows.
		/// </summary>
		/// <value><c>true</c> if is linux; otherwise, <c>false</c>.</value>
		public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public string UserTimeZone
        {
            get { return IsLinux ? "NZ" : "New Zealand Standard Time"; } //Pacific/Auckland
        }

        public EGlobalPolicy() {}

        public EGlobalPolicy(Package package, ClientProgramme programme)
        {
            Package = package;
            ClientProgramme = programme;
            EGlobalPolicyRiskConfig = new List<EGlobalPolicyRiskConfig>();
        }


        #region Properties

        [XmlIgnore]
            public virtual string Branch
            {
                get
                {
                    return EGlobalPolicyConfig.Branch;
                }
            }

            [XmlIgnore]
            public virtual string GetDescription1
            {
                get
                {

                var description = "";

                ClientAgreement objClientAgreement = null;
                foreach (ClientAgreement clientAgreement in ClientProgramme.Agreements)
                {
                    if (objClientAgreement == null && clientAgreement.MasterAgreement)
                    {
                        objClientAgreement = clientAgreement;
                    }
                }
                
                if (ClientProgramme.InformationSheet.IsRenewawl && ClientProgramme.InformationSheet.PreviousInformationSheet != null)
                {
                    description = Package.DescriptionRenew;
                }
                else if (ClientProgramme.InformationSheet.IsRenewawl && ClientProgramme.InformationSheet.PreviousInformationSheet == null)
                {
                    description = Package.DescriptionRenew;
                } else if (ClientProgramme.InformationSheet.IsChange && ClientProgramme.InformationSheet.PreviousInformationSheet != null)
                {
                    description = Package.DescriptionChange;
                } else
                {
                    description = Package.DescriptionNew;
                }
                if (ClientProgramme.HasEGlobalCustomDescription)
                {
                    description = ClientProgramme.EGlobalCustomDescription;
                }
                if (objClientAgreement.ClientAgreementTermsCancel.Where(catcan => catcan.DateDeleted == null).Count() > 0)
                {
                    description = Package.DescriptionCancel;
                }

                /*if (gv_transactionType == TransactionType.Lapse)
                        return Package.DescriptionLapse;*/

                /*if (EGlobalPolicy.CancelledEffectiveDate != DateTime.MinValue || gv_transactionType == TransactionType.Cancel)
                    return Package.DescriptionCancel;*/

                Description1 = string.Format(description,
                            TimeZoneInfo.ConvertTimeFromUtc(objClientAgreement.InceptionDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ")),
                            TimeZoneInfo.ConvertTimeFromUtc(objClientAgreement.ExpiryDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ")));

                    return Description1;
                }
                set
                {
                    Description1 = value;
                }
            }

        public string Serialize()
        {
            var serxml = new XmlSerializer(GetType());
            var ms = new MemoryStream();
            serxml.Serialize(ms, this);
            string xml = Encoding.UTF8.GetString(ms.ToArray());

            return xml;
        }

        [XmlIgnore]
            public virtual string GetDescription2
            {
                get
                {

                ClientAgreement objClientAgreement = null;
                foreach (ClientAgreement clientAgreement in ClientProgramme.Agreements)
                {
                    if (objClientAgreement == null && clientAgreement.MasterAgreement)
                    {
                        objClientAgreement = clientAgreement;
                    }
                }


                /*if (TCPolicy.CancelledEffectiveDate != DateTime.MinValue || gv_transactionType == TransactionType.Cancel)
                {
                    Description2 = Package.StatementCancel;
                }*/
                if (ClientProgramme.InformationSheet.IsRenewawl)
                    {
                        Description2 = Package.StatementRenew;
                    }
                    else if (ClientProgramme.InformationSheet.IsChange)
                    {
                        Description2 = Package.StatementChange;
                    }
                    else
                    {
                        Description2 = Package.StatementNew;
                    }

                if (objClientAgreement.ClientAgreementTermsCancel.Where(catcan => catcan.DateDeleted == null).Count() > 0)
                {
                    Description2 = Package.StatementCancel;
                }
                    
                    return Description2;
                }
                set
                {
                    Description2 = value;
                }
            }

            [XmlIgnore]
            public virtual string ExtensionCode
            {
                get
                {
                    return EGlobalPolicyConfig.ContractCode;
                }
            }

            [XmlIgnore]
            public virtual string IncomeClass
            {
                get
                {
                string incomeClass = ClientProgramme.EGlobalClientStatus;
                if (string.IsNullOrEmpty(incomeClass))
                {
                    if (ClientProgramme.InformationSheet.IsRenewawl)
                    {
                        incomeClass = "REN";
                    } else
                    {
                        incomeClass = "NEW";
                    }
                }
                return incomeClass;
                }
            }

            [XmlIgnore]
            public virtual int MultiRisk
            {
                get { return -1; }
            }

            [XmlIgnore]
            public string FTPFolder
            {
                get { return EGlobalPolicyConfig.FTPFolder; }
            }

            [XmlIgnore]
            public decimal GetBrokerFee
            {
                get
                {                    
                    return BrokerFee;
                }
                set
                {
                    BrokerFee = value;
                }
            }

            #endregion            

            #region serialization feilds

            [XmlElement("Client")]
            public EBixClient Client
            {
                get;
                set;
            }

            [XmlElement("Policy")]
            public EBixPolicy Policy
            {
                get;
                set;
            }

            [XmlArray("POLICYRISKS")]
            [XmlArrayItem("POLICYRISK")]
            public List<EBixPolicyRisk> PolicyRisks
            {
                get;
                set;
            }

            [XmlArray("INSURERS")]
            [XmlArrayItem("INSURER")]
            public List<EBixInsurer> Insurers
            {
                get;
                set;
            }

            [XmlElement("QUEUE")]
            public EBixQueue Queue
            {
                get;
                set;
            }

            [XmlArray("SubAgents")]
            [XmlArrayItem("SubAgent")]
            public List<EBixSubAgent> SubAgents
            {
                get;
                set;
            }

            [XmlElement("PUF")]
            public EBixPUF PUF
            {
                get;
                set;
            }

            #endregion
        }
        #region nested XML elements

        [XmlRoot("Client")]
        public class EBixClient
        {
            [XmlElement("CLI_EXTSYSFLAG")]
            public string ExtSystemFlag
            {
                get;
                set;
            }
        }

        [XmlRoot("Policy")]
        public class EBixPolicy
        {
            private IFormatProvider culture = new System.Globalization.CultureInfo("fr-FR", true);

            [XmlIgnore()]
            public DateTime PolicyDateTime
            {
                get;
                set;
            }

            [XmlElement("DateTime")]
            public string DateTimeString
            {
                get
                {
                    return PolicyDateTime.ToString("yyyy-MM-dd HH:mm:ss.f");
                }
                set
                {
                    PolicyDateTime = DateTime.Parse(value, culture);
                }
            }

            [XmlElement("INTERNAL")]
            public string Internal
            {
                get;
                set;
            }

            [XmlElement("PLY_TRANSACTIONTYPE")]
            public int TransactionType
            {
                get;
                set;
            }

            [XmlElement("PLY_CREATEDBYUSERID")]
            public string CreatedByUser
            {
                get;
                set;
            }

            [XmlElement("PLY_BRANCH")]
            public string Branch
            {
                get;
                set;
            }

            [XmlElement("PLY_COMPANY")]
            public string Company
            {
                get;
                set;
            }

            [XmlElement("PLY_COUNTRYCODE")]
            public string CountryCode
            {
                get;
                set;
            }

            [XmlIgnore()]
            public int DirectCredit
            {
                get;
                set;
            }

            [XmlElement("PLY_DIRECTCREDIT")]
            public string DirectCreditString
            {
                get
                {
                    return DirectCredit.ToString();
                }
                set
                {
                    DirectCredit = Convert.ToInt32(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlIgnore()]
            public int MultiRisk
            {
                get;
                set;
            }

            [XmlElement("PLY_MULTIRISK")]
            public string MultiRiskString
            {
                get
                {
                    return MultiRisk.ToString();
                }
                set
                {
                    MultiRisk = -1;
            }
            }

            [XmlIgnore()]
            public decimal DueByClient
            {
                get;
                set;
            }

            [XmlElement("PLY_T_DUEBYCLIENT")]
            public string DueByClientString
            {
                get
                {
                    return DueByClient.ToString("0.00");
                }
                set
                {
                    DueByClient = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal BrokerAmountDue
            {
                get;
                set;
            }

            [XmlElement("PLY_T_BROKAMTDUE")]
            public string BrokerAmountDueString
            {
                get
                {
                    return BrokerAmountDue.ToString("0.00");
                }
                set
                {
                    BrokerAmountDue = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal BrokerCeqDue
            {
                get;
                set;
            }

            [XmlElement("PLY_T_BROKCEQDUE")]
            public string BrokerCeqDueString
            {
                get
                {
                    return BrokerCeqDue.ToString("0.00");
                }
                set
                {
                    BrokerCeqDue = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal BSCAmount
            {
                get;
                set;
            }

            [XmlElement("PLY_T_BSCAMOUNT")]
            public string BSCAmountString
            {
                get
                {
                    return BSCAmount.ToString("0.00");
                }
                set
                {
                    BSCAmount = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal CEQuake
            {
                get;
                set;
            }

            [XmlElement("PLY_T_CEQUAKE")]
            public string CEQuakeString
            {
                get
                {
                    return CEQuake.ToString("0.00");
                }
                set
                {
                    CEQuake = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal BscGST
            {
                get;
                set;
            }

            [XmlElement("PLY_T_BSCGST")]
            public string BscGSTString
            {
                get
                {
                    return BscGST.ToString("0.00");
                }
                set
                {
                    BscGST = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal GSTPremium
            {
                get;
                set;
            }

            [XmlElement("PLY_T_GSTPREM")]
            public string GSTPremiumString
            {
                get
                {
                    return GSTPremium.ToString("0.00");
                }
                set
                {
                    GSTPremium = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal LeviesA
            {
                get;
                set;
            }

            [XmlElement("PLY_T_LEVIESA")]
            public string LeviesAString
            {
                get
                {
                    return LeviesA.ToString("0.00");
                }
                set
                {
                    LeviesA = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal LeviesB
            {
                get;
                set;
            }

            [XmlElement("PLY_T_LEVIESB")]
            public string LeviesBString
            {
                get
                {
                    return LeviesB.ToString("0.00");
                }
                set
                {
                    LeviesB = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal CoyPremium
            {
                get;
                set;
            }

            [XmlElement("PLY_T_COYPREM")]
            public string CoyPremiumString
            {
                get
                {
                    return CoyPremium.ToString("0.00");
                }
                set
                {
                    CoyPremium = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal GSTBrokerage
            {
                get;
                set;
            }

            [XmlElement("PLY_T_GSTBROK")]
            public string GSTBrokerageString
            {
                get
                {
                    return GSTBrokerage.ToString("0.00");
                }
                set
                {
                    GSTBrokerage = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal SPCFee
            {
                get;
                set;
            }

            [XmlElement("PLY_T_SPCFEE")]
            public string SPCFeeString
            {
                get
                {
                    return SPCFee.ToString("0.00");
                }
                set
                {
                    SPCFee = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlElement("PLY_DEPARTMENT")]
            public string Department
            {
                get;
                set;
            }

            [XmlIgnore()]
            public DateTime EffectiveDate
            {
                get;
                set;
            }

            [XmlElement("PLY_EFFDATE")]
            public string EffectiveDateString
            {
                get
                {
                    return EffectiveDate.ToString("dd/MM/yyyy");
                }
                set
                {
                    EffectiveDate = DateTime.Parse(value, culture);
                }
            }

            [XmlElement("PLY_BILLINGCURRRATE")]
            public int BillingCurrencyRate
            {
                get;
                set;
            }

            [XmlIgnore()]
            public DateTime ExpiryDate
            {
                get;
                set;
            }

            [XmlElement("PLY_EXPIRYDATE")]
            public string ExpiryDateString
            {
                get
                {
                    return ExpiryDate.ToString("dd/MM/yyyy");
                }
                set
                {
                    ExpiryDate = DateTime.Parse(value, culture);
                }
            }

            [XmlElement("PLY_EXTERNALSYS")]
            public int ExternalSystem
            {
                get;
                set;
            }

            [XmlElement("PLY_EXTERNALSYSTEMCONTRACTNO")]
            public string ExternalSystemContractID
            {
                get;
                set;
            }

            [XmlIgnore()]
            public DateTime ExternalSystemInvDate
            {
                get;
                set;
            }

            [XmlElement("PLY_EXTERNALSYSTEMINVDATE")]
            public string ExternalSystemInvDateString
            {
                get;
                set;
            }

            [XmlIgnore()] //added as Marsh requested to remove this tag
            [XmlElement("PLY_EXTERNALSYSTEMINVNO")]
            public int ExternalSystemInvNumber
            {
                get;
                set;
            }

            [XmlIgnore()]
            public int ExternalSystemVersionNumber
            {
                get;
                set;
            }

            [XmlElement("PLY_EXTERNALSYSTEMVERNO")]
            public string ExternalSystemVersionNumberString
            {
                get
                {
                    //return this.ExternalSystemVersionNumber.ToString("000");
                    return ExternalSystemVersionNumber.ToString("D3");
                }
                set
                {
                    ExternalSystemVersionNumber = Convert.ToInt32(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlIgnore()]
            public DateTime InceptionDate
            {
                get;
                set;
            }

            [XmlElement("PLY_INCEPTIONDATE")]
            public string InceptionDateString
            {
                get
                {
                    return InceptionDate.ToString("dd/MM/yyyy");
                }
                set
                {
                    InceptionDate = DateTime.Parse(value, culture);
                }
            }

            [XmlElement("PLY_INCOMECLASS")]
            public string IncomeClass
            {
                get;
                set;
            }

            [XmlElement("PLY_LASTMAJORTRANS")]
            public string LastMajorTrans
            {
                get;
                set;
            }

            [XmlIgnore()]
            public DateTime LastRenewalDate
            {
                get;
                set;
            }

            [XmlElement("PLY_LASTRENDATE")]
            public string LastRenewalDateString
            {
                get
                {
                    return LastRenewalDate.ToString("dd/MM/yyyy");
                }
                set
                {
                    LastRenewalDate = DateTime.Parse(value, culture);
                }
            }

            [XmlElement("PLY_RENEWABLE")]
            public int Renewable
            {
                get;
                set;
            }

            [XmlElement("PLY_RISKCODE")]
            public string RiskCode
            {
                get;
                set;
            }

            [XmlElement("PLY_USERIDSERV")]
            public string UserIDServ
            {
                get;
                set;
            }

            [XmlElement("PLY_TERMSOFTRADECODE")]
            public string TermsofTradeCode
            {
                get;
                set;
            }

            [XmlElement("PLY_COVERNUMBER")]
            public string CoverNumber
            {
                get;
                set;
            }

            [XmlElement("PLY_VERNO")]
            public string VersionNumber
            {
                get;
                set;
            }

            [XmlElement("PLY_DESCRIPTION")]
            public string Description
            {
                get;
                set;
            }

            [XmlElement("PDT_DOCUMENT")]
            public string Document
            {
                get;
                set;
            }

            [XmlElement("PDT_LOCATED")]
            public string Located
            {
                get;
                set;
            }

            [XmlIgnore()]
            public DateTime PolicyTime
            {
                get;
                set;
            }

            [XmlElement("PLY_TIME")]
            public string PolicyTimeString
            {
                get
                {
                    return PolicyTime.ToString("yyyy-MM-dd HH:mm:ss.f");
                }
                set
                {
                    PolicyTime = DateTime.Parse(value, culture);
                }
            }

            [XmlElement("PLY_CLIENTNUMBER")]
            public string ClientNumber
            {
                get;
                set;
            }

            [XmlElement("PLY_IRCOMPANY")]
            public string IRCompany
            {
                get;
                set;
            }

            [XmlElement("PLY_IRBRANCH")]
            public string IRBranch
            {
                get;
                set;
            }

            [XmlElement("PLY_PREMIUMFUNDED")]
            public int PremiumFunded
            {
                get;
                set;
            }

            [XmlElement("PLY_STATEMENTDESC")]
            public string StatementDescription
            {
                get;
                set;
            }

            [XmlElement("PLY_InsurerReference")]
            public string InsurerReference
            {
                get;
                set;
            }

            [XmlIgnore()]
            public DateTime TermsofTradeDate
            {
                get;
                set;
            }

            [XmlElement("PLY_TERMSOFTRADEDATE")]
            public string TermsofTradeDateString
            {
                get
                {
                    return this.TermsofTradeDate.ToString("dd/MM/yyyy");
                }
                set
                {
                    this.TermsofTradeDate = DateTime.Parse(value, culture);
                }
            }

            [XmlIgnore()]
            public DateTime RenewalDate
            {
                get;
                set;
            }

            [XmlElement("PLY_RENDATE")]
            public string RenewalDateString
            {
                get
                {
                    return this.RenewalDate.ToString("dd/MM/yyyy");
                }
                set
                {
                    this.RenewalDate = DateTime.Parse(value, culture);
                }
            }
        }

        [XmlRoot("POLICYRISK")]
        public class EBixPolicyRisk
        {
            [XmlIgnore]
            public Guid ID
            {
                get;
                set;
            }

            [XmlIgnore]
            public string TCMergeCode
            {
                get;
                set;
            }

            [XmlIgnore]
            public Guid TCClassOfBusiness
            {
                get;
                set;
            }

            [XmlElement("PLR_RISKCODE")]
            public string RiskCode
            {
                get;
                set;
            }

            [XmlIgnore()]
            public int SubCover
            {
                get;
                set;
            }

            [XmlElement("PLR_SUBCOVER")]
            public string SubCoverString
            {
                get
                {
                    return this.SubCover.ToString("00");
                }
                set
                {
                    this.SubCover = Convert.ToInt32(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlIgnore()]
            public decimal GSTPremium
            {
                get;
                set;
            }

            [XmlElement("PLR_T_GSTPREM")]
            public string GSTPremiumString
            {
                get
                {
                    return GSTPremium.ToString("0.00");
                }
                set
                {
                    GSTPremium = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal DueByClient
            {
                get;
                set;
            }

            [XmlElement("PLR_T_DUEBYCLIENT")]
            public string DueByClientString
            {
                get
                {
                    return DueByClient.ToString("0.00");
                }
                set
                {
                    DueByClient = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal LeviesA
            {
                get;
                set;
            }

            [XmlElement("PLR_T_LEVIESA")]
            public string LeviesAString
            {
                get
                {
                    return LeviesA.ToString("0.00");
                }
                set
                {
                    LeviesA = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal LeviesB
            {
                get;
                set;
            }

            [XmlElement("PLR_T_LEVIESB")]
            public string LeviesBString
            {
                get
                {
                    return LeviesB.ToString("0.00");
                }
                set
                {
                    LeviesB = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlIgnore()]
            public decimal CoyPremium
            {
                get;
                set;
            }

            [XmlElement("PLR_T_COYPREM")]
            public string CoyPremiumString
            {
                get
                {
                    return CoyPremium.ToString("0.00");
                }
                set
                {
                    CoyPremium = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal CEQuake
            {
                get;
                set;
            }

            [XmlElement("PLR_T_CEQUAKE")]
            public string CEQuakeString
            {
                get
                {
                    return CEQuake.ToString("0.00");
                }
                set
                {
                    CEQuake = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlIgnore()]
            public decimal BrokerAmountDue
            {
                get;
                set;
            }

            [XmlElement("PLR_T_BROKAMTDUE")]
            public string BrokerAmountDueString
            {
                get
                {
                    return BrokerAmountDue.ToString("0.00");
                }
                set
                {
                    BrokerAmountDue = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlIgnore]
            public decimal BrokerAmountRate
            {
                get;
                set;
            }

            [XmlIgnore()]
            public decimal BrokerCeqDue
            {
                get;
                set;
            }

            [XmlElement("PLR_T_BROKCEQDUE")]
            public string BrokerCeqDueString
            {
                get
                {
                    return BrokerCeqDue.ToString("0.00");
                }
                set
                {
                    BrokerCeqDue = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore]
            public decimal BrokerCeqDueRate
            {
                get;
                set;
            }

            [XmlIgnore()]
            public decimal GSTBrokerage
            {
                get;
                set;
            }

            [XmlElement("PLR_T_GSTBROK")]
            public string GSTBrokerageString
            {
                get
                {
                    return GSTBrokerage.ToString("0.00");
                }
                set
                {
                    GSTBrokerage = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlIgnore()]
            public decimal BscGST
            {
                get;
                set;
            }

            [XmlElement("PLR_T_BSCGST")]
            public string BscGSTString
            {
                get
                {
                    return this.BscGST.ToString("0.00");
                }
                set
                {
                    this.BscGST = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlIgnore()]
            public decimal BSCAmount
            {
                get;
                set;
            }

            [XmlElement("PLR_T_BSCAMOUNT")]
            public string BSCAmountString
            {
                get
                {
                    return BSCAmount.ToString("0.00");
                }
                set
                {
                    BSCAmount = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlElement("PLR_COVERNUMBER")]
            public string CoverNumber
            {
                get;
                set;
            }

            [XmlElement("PLR_VERNO")]
            public string VersionNumber
            {
                get;
                set;
            }
        }

        [XmlRoot("INSURER")]
        public class EBixInsurer
        {
            private IFormatProvider culture = new System.Globalization.CultureInfo("fr-FR", true);

            [XmlIgnore()]
            public int PolicyNumber
            {
                get;
                set;
            }

            [XmlElement("IAC_POLICYNUMBER")]
            public string PolicyNumberString
            {
                get
                {
                    return PolicyNumber.ToString();
                }
                set
                {
                    PolicyNumber = Convert.ToInt32(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlElement("IAC_ACCTINSBRANCH")]
            public string AccountInsurerBranch
            {
                get;
                set;
            }

            [XmlElement("IAC_ACCTINSCODE")]
            public string AccountInsurerCode
            {
                get;
                set;
            }

            [XmlElement("IAC_RISKCODE")]
            public string RiskCode
            {
                get;
                set;
            }

            [XmlIgnore()]
            public int SubCover
            {
                get;
                set;
            }

            [XmlElement("IAC_SUBCOVER")]
            public string SubCoverString
            {
                get
                {
                    return SubCover.ToString("00");
                }
                set
                {
                    SubCover = Convert.ToInt32(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            //public int InsurerProportion
            public decimal InsurerProportion
            {
                get;
                set;
            }

            [XmlElement("IAC_INSURERPROPORTION")]
            public string InsurerProportionString
            {
                get
                {
                    return InsurerProportion.ToString();
                }
                set
                {
                    //this.InsurerProportion = TC_Shared.CNullInt(value, -1);
                    InsurerProportion = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlElement("IAC_INSBRANCH")]
            public string InsurerBranch
            {
                get;
                set;
            }

            [XmlElement("IAC_INSCODE")]
            public string InsurerCode
            {
                get;
                set;
            }

            [XmlIgnore()]
            public int LeadInsurer
            {
                get;
                set;
            }

            [XmlElement("IAC_INSTYPE")]
            public string LeadInsurerString
            {
                get
                {
                    return LeadInsurer.ToString();
                }
                set
                {
                    LeadInsurer = Convert.ToInt32(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlIgnore()]
            public decimal BrokerAmountValue
            {
                get;
                set;
            }

            [XmlElement("IAC_BROKAMTVALUE")]
            public string BrokerAmountValueString
            {
                get
                {
                    return BrokerAmountValue.ToString("0.00");
                }
                set
                {
                    BrokerAmountValue = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal BrokerAmountDue_T
            {
                get;
                set;
            }

            [XmlElement("IAC_T_BROKAMTDUE")]
            public string BrokerAmountDueString_T
            {
                get
                {
                    return BrokerAmountDue_T.ToString("0.00");
                }
                set
                {
                    BrokerAmountDue_T = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal BrokerCEQDue_T
            {
                get;
                set;
            }

            [XmlElement("IAC_T_BROKCEQDUE")]
            public string BrokerCEQDueString_T
            {
                get
                {
                    return BrokerCEQDue_T.ToString("0.00");
                }
                set
                {
                    BrokerCEQDue_T = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal BrokerCEQRate
            {
                get;
                set;
            }

            [XmlElement("IAC_BROKCEQRATE")]
            public string BrokerCEQRateString
            {
                get
                {
                    return BrokerCEQRate.ToString("0.00");
                }
                set
                {
                    BrokerCEQRate = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal LeviesA
            {
                get;
                set;
            }

            [XmlElement("IAC_T_LEVIESA")]
            public string LeviesAString
            {
                get
                {
                    return LeviesA.ToString("0.00");
                }
                set
                {
                    LeviesA = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal LeviesB
            {
                get;
                set;
            }

            [XmlElement("IAC_T_LEVIESB")]
            public string LeviesBString
            {
                get
                {
                    return LeviesB.ToString("0.00");
                }
                set
                {
                    LeviesB = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal NetPay
            {
                get;
                set;
            }

            [XmlElement("IAC_T_NETTPAY")]
            public string NetPayString
            {
                get
                {
                    return NetPay.ToString("0.00");
                }
                set
                {
                    NetPay = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal CoyPremium
            {
                get;
                set;
            }

            [XmlElement("IAC_T_COYPREM")]
            public string CoyPremiumString
            {
                get
                {
                    return CoyPremium.ToString("0.00");
                }
                set
                {
                    CoyPremium = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal CEQuake
            {
                get;
                set;
            }

            [XmlElement("IAC_T_CEQUAKE")]
            public string CEQuakeString
            {
                get
                {
                    return CEQuake.ToString("0.00");
                }
                set
                {
                    CEQuake = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal GSTBrokerage
            {
                get;
                set;
            }

            [XmlElement("IAC_T_GSTBROK")]
            public string GSTBrokerageString
            {
                get
                {
                    return GSTBrokerage.ToString("0.00");
                }
                set
                {
                    GSTBrokerage = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal GSTPremium
            {
                get;
                set;
            }

            [XmlElement("IAC_T_GSTPREM")]
            public string GSTPremiumString
            {
                get
                {
                    return GSTPremium.ToString("0.00");
                }
                set
                {
                    GSTPremium = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlElement("IAC_TERMSOFTRADECODE")]
            public string TermsofTradeCode
            {
                get;
                set;
            }

            [XmlIgnore()]
            public DateTime TermsofTradeDate
            {
                get;
                set;
            }

            [XmlElement("IAC_TERMSOFTRADEDATE")]
            public string TermsofTradeDateString
            {
                get
                {
                    if (TermsofTradeDate == DateTime.MinValue)
                        return null;
                    else
                        return TermsofTradeDate.ToString("dd/MM/yyyy");
                }
                set
                {
                    TermsofTradeDate = DateTime.Parse(value, culture);
                }
            }

            [XmlIgnore()]
            public DateTime PremiumWarrentyDate
            {
                get;
                set;
            }

            [XmlElement("IAC_PREMIUMWARRANTYDATE")]
            public string PremiumWarrentyDateString
            {
                get
                {
                    return PremiumWarrentyDate.ToString("dd/MM/yyyy");
                }
                set
                {
                    PremiumWarrentyDate = DateTime.Parse(value, culture);
                }
            }

            [XmlElement("IAC_TRANIDENT")]
            public string TranIdent
            {
                get;
                set;
            }

            [XmlElement("IAC_COVERNUMBER")]
            public string CoverNumber
            {
                get;
                set;
            }

            [XmlElement("IAC_VERNO")]
            public string VersionNumber
            {
                get;
                set;
            }

            [XmlElement("IAC_BROKAMTRATE")]
            public string BrokerAmountRate
            {
                get;
                set;
            }

            [XmlElement("IAC_INSURERPERSON")]
            public string InsurerPerson
            {
                get;
                set;
            }

            [XmlElement("IAC_BROKER")]
            public string Broker
            {
                get;
                set;
            }

            [XmlIgnore()]
            public DateTime Placement
            {
                get;
                set;
            }

            [XmlElement("IAC_PLACEMENT")]
            public string PlacementString
            {
                get
                {
                    return this.Placement.ToString("dd/MM/yyyy");
                }
                set
                {
                    this.Placement = DateTime.Parse(value, culture);
                }
            }

            [XmlIgnore()]
            public decimal NonRTax
            {
                get;
                set;
            }

            [XmlElement("IAC_T_NONRTAX")]
            public string NonRTaxString
            {
                get
                {
                    return NonRTax.ToString("0.00");
                }
                set
                {
                    NonRTax = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlElement("IAC_INSURERINVOICE")]
            public string InsurerInvoice
            {
                get;
                set;
            }
        }

        [XmlRoot("QUEUE")]
        public class EBixQueue
        {
            [XmlElement("QUE_CUSTINV")]
            public int CustInv
            {
                get;
                set;
            }

            [XmlElement("QUE_USER")]
            public string User
            {
                get;
                set;
            }
        }

        [XmlRoot("SubAgent")]
        public class EBixSubAgent
        {
            [XmlIgnore()]
            public Guid ID
            {
                get;
                set;
            }

            [XmlIgnore()]
            public decimal AmountCeded
            {
                get;
                set;
            }

            [XmlIgnore()]
            public decimal CalcAmount
            {
                get;
                set;
            }

            [XmlElement("PLS_AMOUNTT")]
            public string AmountString
            {
                get
                {
                    return AmountCeded.ToString("0.00");
                }
                set
                {
                    AmountCeded = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlIgnore()]
            public decimal Percent
            {
                get;
                set;
            }

            [XmlElement("PLS_PERCENT")]
            public string PercentString
            {
                get
                {
                    return Percent.ToString("0.00");
                }
                set
                {
                    Percent = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlElement("PLS_SUBAGENT")]
            public string SubAgent
            {
                get;
                set;
            }

            [XmlIgnore()]
            public int SubCover
            {
                get;
                set;
            }

            [XmlElement("PLS_SUBCOVER")]
            public string SubCoverString
            {
                get
                {
                    return SubCover.ToString("00");
                }
                set
                {
                    SubCover = Convert.ToInt32(value);//TC_Shared.CNullDec(value, -1.0m);
            }
            }

            [XmlElement("PLS_GSTFLAG")]
            public int GSTFlag
            {
                get;
                set;
            }

            [XmlIgnore()]
            public decimal GSTCeded
            {
                get;
                set;
            }

            [XmlElement("PLS_GSTAMOUNTT")]
            public string GSTAmountString
            {
                get
                {
                    return GSTCeded.ToString("0.00");
                }
                set
                {
                    GSTCeded = Convert.ToDecimal(value);//TC_Shared.CNullDec(value, -1.0m);
                }
            }

            [XmlElement("PLS_COVERNUMBER")]
            public int CoverNumber
            {
                get;
                set;
            }

            [XmlElement("PLS_VERNO")]
            public int VerNo
            {
                get;
                set;
            }

        }

        [XmlRoot("PUF")]
        public class EBixPUF
        {
            [XmlElement("PUF1")]
            public EBixPUFElement Element1
            {
                get;
                set;
            }

            [XmlElement("PUF2")]
            public EBixPUFElement Element2
            {
                get;
                set;
            }
        }


        public class EBixPUFElement
        {
            [XmlElement("PUD_CAPTION")]
            public string Caption
            {
                get;
                set;
            }

            [XmlElement("PUD_VALUE")]
            public decimal Value
            {
                get;
                set;
            }

            [XmlElement("PUD_DESCRIPTION")]
            public string Description
            {
                get;
                set;
            }

            [XmlElement("PUD_SINO")]
            public int Sino
            {
                get;
                set;
            }
        }

        #endregion

    }

