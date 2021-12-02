using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Infrastructure.Payment.EGlobalAPI.BaseClasses;

namespace DealEngine.Infrastructure.Payment.EGlobalAPI
{
    public class EGlobalPolicyAPI
    {
        private EGlobalPolicy EGlobalPolicy;
        private User CurrentUser;
        private string EbixUser;
        private string EbixDepartment; //branchcode

        //New = 1,
        //Endorse = 2,
        //Update = 21,
        //Reverse = 22,
        //Renewal = 3,
        //Lapse = 5,
        //Cancel = 6,
        private int gv_transactionType;
        private string gv_strUISReference;
        private string gv_strMasterAgreementReference;
        private string gv_strOriginalAgreementReference;
        private decimal gv_decPaymentDirection = 1;

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

        public CultureInfo UserCulture
        {
            get { return CultureInfo.CreateSpecificCulture("en-NZ"); }
        }

        public EGlobalPolicyAPI(EGlobalPolicy _EGlobalPolicy, User _CurrentUser)
        {
            EGlobalPolicy = _EGlobalPolicy;
            CurrentUser = _CurrentUser;
        }

        #region Invoice Creation

        /// <summary>
        /// Creates the policy invoice with specified Policy Risks and autodetects transaction type
        /// </summary>
        /// <param name="risks">Risks.</param>
        public void CreatePolicyInvoice()
        {
            ClientAgreement objClientAgreement = null;
            foreach (ClientAgreement clientAgreement in EGlobalPolicy.ClientProgramme.Agreements)
            {
                if (objClientAgreement == null && clientAgreement.MasterAgreement)
                {
                    objClientAgreement = clientAgreement;
                }
            }

            AutoDetectTransactionType(objClientAgreement);

            // Create the PolicyRisks
            GetPolicyRisks();

            // Create the Insurers
            GetInsurers();

            // Create the Policy
            GetPolicy(objClientAgreement, false);

            // Create the SubAgents
            GetSubAgents();

            // Update Policy fields
            EGlobalPolicy.Policy.PolicyDateTime = DateTime.Now;
            EGlobalPolicy.Policy.EffectiveDate = DateTime.Now;
        }

        #endregion

        #region Reverse Invoice

        public void CreateReversePolicyInvoice(EGlobalSubmission eglobalsubmission)
        {
            //SetTransactionType(TransactionType.Reverse);

            gv_decPaymentDirection = -1;
            EGlobalPolicy.PaymentDirection = -1;
            gv_transactionType = 2; //endorse

            LoadTransaction(eglobalsubmission);
            //CreateReversePolicyInvoice(GetReversePolicyRisks());

            ReversePolicy();

            GetReversePolicyRisks();

            GetInsurers();

            GetReverseSubAgents();

            // Update Policy fields
            EGlobalPolicy.Policy.PolicyDateTime = DateTime.Now;
            EGlobalPolicy.Policy.EffectiveDate = DateTime.Now;
        }

        /// <summary>
        /// Creates the reverse policy invoice from the specified policy risks
        /// </summary>
        /// <param name="risks">Risks.</param>
        public void CreateReversePolicyInvoice(List<EBixPolicyRisk> risks)
        {
            EGlobalPolicy.PaymentDirection = -1;

            //PolicyRisks = risks;

            GetInsurers();

            // Create the Policy
            //EGlobalPolicy.PolicyRisks = GetPolicy();

            GetReverseSubAgents();

            // Update Policy fields
            EGlobalPolicy.Policy.PolicyDateTime = DateTime.Now;
            EGlobalPolicy.Policy.EffectiveDate = DateTime.Now;
        }

        protected virtual void ReversePolicy()
        {
            EGlobalPolicy.Policy.BSCAmount *= EGlobalPolicy.PaymentDirection;
            EGlobalPolicy.Policy.BscGST *= EGlobalPolicy.PaymentDirection;
            EGlobalPolicy.Policy.DueByClient *= EGlobalPolicy.PaymentDirection;
            EGlobalPolicy.Policy.SPCFee *= EGlobalPolicy.PaymentDirection;
            EGlobalPolicy.Policy.BrokerAmountDue *= EGlobalPolicy.PaymentDirection;
            EGlobalPolicy.Policy.GSTPremium *= EGlobalPolicy.PaymentDirection;
            EGlobalPolicy.Policy.LeviesA *= EGlobalPolicy.PaymentDirection;
            EGlobalPolicy.Policy.LeviesB *= EGlobalPolicy.PaymentDirection;
            EGlobalPolicy.Policy.CoyPremium *= EGlobalPolicy.PaymentDirection;
            EGlobalPolicy.Policy.GSTBrokerage *= EGlobalPolicy.PaymentDirection;

        }

        protected virtual void GetReversePolicyRisks()
        {
            List<EBixPolicyRisk> risks = EGlobalPolicy.PolicyRisks;

            foreach (EBixPolicyRisk risk in risks)
            {
                risk.CoyPremium *= EGlobalPolicy.PaymentDirection;
                risk.CEQuake *= EGlobalPolicy.PaymentDirection;
                risk.LeviesA *= EGlobalPolicy.PaymentDirection;
                risk.LeviesB *= EGlobalPolicy.PaymentDirection;
                risk.GSTPremium *= EGlobalPolicy.PaymentDirection;

                risk.BrokerAmountDue *= EGlobalPolicy.PaymentDirection;
                risk.BrokerCeqDue *= EGlobalPolicy.PaymentDirection;
                risk.GSTBrokerage *= EGlobalPolicy.PaymentDirection;

                risk.DueByClient *= EGlobalPolicy.PaymentDirection;
            }

            EGlobalPolicy.PolicyRisks = risks;
        }

        protected virtual void GetReverseSubAgents()
        {
            List<EBixSubAgent> subAgents = EGlobalPolicy.SubAgents;

            foreach (EBixSubAgent agent in subAgents)
            {
                agent.CalcAmount *= EGlobalPolicy.PaymentDirection;
                agent.AmountCeded *= EGlobalPolicy.PaymentDirection;
                agent.GSTCeded *= EGlobalPolicy.PaymentDirection;
            }

            EGlobalPolicy.SubAgents = subAgents;
        }

        #endregion

        #region Cancel Invoice

        /// <summary>
        /// Creates the cancel policy invoice.
        /// </summary>
        public void CreateCancelPolicyInvoice(Package package, ClientProgramme programme)
        {
            //AutoDetectTransactionType();
            gv_transactionType = 6; //cancel
            GetPolicyRisks();
            //SetTransactionType(TransactionType.Cancel);
            CreateCancelPolicyInvoice(GetCancelPolicyRisks(package, programme));
        }

        /// <summary>
        /// Creates the cancel policy invoice from the specified policy risks
        /// </summary>
        /// <param name="risks">Risks.</param>
        public virtual void CreateCancelPolicyInvoice(List<EBixPolicyRisk> risks)
        {
            ClientAgreement objClientAgreement = null;
            foreach (ClientAgreement clientAgreement in EGlobalPolicy.ClientProgramme.Agreements)
            {
                if (objClientAgreement == null && clientAgreement.MasterAgreement)
                {
                    objClientAgreement = clientAgreement;
                }
            }

            //PolicyRisks = risks;
            EGlobalPolicy.PolicyRisks = risks;

            // Create the Policy
            GetPolicy(objClientAgreement, true);

            GetInsurers();

            // Update Policy fields
            EGlobalPolicy.PaymentDirection = -1;
            EGlobalPolicy.Policy.PolicyDateTime = DateTime.Now;

            //            if (TCPolicy.CancelledEffectiveDate.Date == TCPolicy.InceptionDate.GetValueOrDefault().Date)
            //                CalculateInvoiceSummary(Policy, -TCPolicy.BrokerFee.GetValueOrDefault());
            //            else if (TCPolicy.CancelledEffectiveDate.Date > TCPolicy.InceptionDate.GetValueOrDefault().Date)
            //                CalculateInvoiceSummary(Policy, 0M);
        }

        protected virtual List<EBixPolicyRisk> GetCancelPolicyRisks(Package package, ClientProgramme programme)
        {
            ClientAgreement objClientAgreement = null;
            foreach (ClientAgreement clientAgreement in EGlobalPolicy.ClientProgramme.Agreements)
            {
                if (objClientAgreement == null && clientAgreement.MasterAgreement)
                {
                    objClientAgreement = clientAgreement;
                }
            }

            ClientAgreementTermCancel canterm = objClientAgreement.ClientAgreementTermsCancel.FirstOrDefault();
            List<EBixPolicyRisk> risks = new List<EBixPolicyRisk>();

            risks.Add(GetCancelPolicyRisk(objClientAgreement, canterm, package));

            return risks;

            //return new List<EBixPolicyRisk>();
        }

        protected EBixPolicyRisk GetCancelPolicyRisk(ClientAgreement clientAgreement, ClientAgreementTermCancel canterm, Package package)
        {
            decimal taxRate = clientAgreement.ClientInformationSheet.Programme.BaseProgramme.TaxRate;

            EBixPolicyRisk pr = new EBixPolicyRisk();

            pr.TCMergeCode = canterm.MergeCodeCan;
            pr.TCClassOfBusiness = canterm.Id;
            pr.RiskCode = package.PackageProducts.FirstOrDefault().PackageProductRiskCode;
            pr.SubCoverString = package.PackageProducts.FirstOrDefault().PackageProductSubCover;

            pr.CoyPremium = canterm.PremiumCan;
            pr.CEQuake = 0M;
            pr.GSTPremium = (pr.CoyPremium + pr.CEQuake) * taxRate;

            pr.BrokerAmountRate = canterm.BrokerageRateCan;
            pr.BrokerCeqDueRate = canterm.NDBrokerageRateCan;

            pr.BrokerAmountDue = (pr.BrokerAmountRate / 100M) * pr.CoyPremium;
            pr.BrokerCeqDue = (pr.BrokerCeqDueRate / 100M) * pr.CEQuake;
            pr.GSTBrokerage = (pr.BrokerAmountDue + pr.BrokerCeqDue) * taxRate;

            pr.BSCAmount = 0m;
            pr.BscGST = pr.BSCAmount * taxRate;

            pr.LeviesA = 0M; //eqc
            pr.LeviesB = canterm.FSLCan; //fsl

            pr.GSTPremium = (pr.CoyPremium + pr.CEQuake + pr.LeviesA + pr.LeviesB) * taxRate;

            pr.DueByClient = Math.Round(pr.CoyPremium + pr.CEQuake + pr.GSTPremium + pr.LeviesA + pr.LeviesB, 2);

            return pr;
        }

        #endregion

        #region Lapse Invoice

        public void CreateLapseInvoice()
        {
            //AutoDetectTransactionType();

            //SetTransactionType(TransactionType.Lapse);
            //CreatePolicyInvoice(GetLapsePolicyRisks());

            GetLapseSubAgents();

            EGlobalPolicy.Policy.StatementDescription = "";
            EGlobalPolicy.Policy.Description = "";
            // When a policy is lapsed the effective date is expected to be the expiry of the previous policy.
            //Policy.EffectiveDate = Policy.RenewalDate;
            EGlobalPolicy.Policy.EffectiveDate = EGlobalPolicy.Policy.InceptionDate;
        }

        /*protected virtual List<EBixPolicyRisk> GetLapsePolicyRisks()
        {
            List<EBixPolicyRisk> risks = GetPolicyRisks();

            foreach (EBixPolicyRisk risk in risks)
            {
                risk.CoyPremium = 0;
                risk.CEQuake = 0;
                risk.LeviesA = 0;
                risk.LeviesB = 0;
                risk.GSTPremium = 0;

                risk.BrokerAmountDue = 0;
                risk.BrokerCeqDue = 0;
                risk.GSTBrokerage = 0;

                risk.DueByClient = 0;
            }

            return risks;
        }*/

        protected virtual void GetLapseSubAgents()
        {
            List<EBixSubAgent> subAgents = EGlobalPolicy.SubAgents;

            foreach (EBixSubAgent agent in subAgents)
            {
                agent.AmountCeded = 0m;
                agent.GSTCeded = 0m;
                agent.Percent = 0m;
            }

            EGlobalPolicy.SubAgents = subAgents;
        }

        #endregion


        #region Loading/Saving

        public Guid LoadLatestTransaction()
        {
            throw new Exception("LoadLatestTransaction - Not yet implemented");
            /*Guid responseID = Guid.Empty;
            using (NpgsqlConnection conn = TC_Shared.GetSqlConnection())
            {
                string strCmd = "SELECT sub.invoiceid, sub.responseid FROM tbleglobalinvoicesubmission sub, tbleglobalinvoiceresponse res " +
                                "WHERE sub.quoteid = @QuoteID AND sub.responseid IS NOT NULL AND sub.responseid = res.responseid " +
                                "AND res.responsetype = 'update' ORDER BY datesubmitted DESC;";
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(strCmd, conn))
                {
                    sqlCmd.Parameters.Add("@QuoteID", NpgsqlDbType.Uuid).Value = TCPolicy.ID;

                    conn.Open();
                    using (NpgsqlDataReader ndr = sqlCmd.ExecuteReader())
                    {
                        if (ndr.Read())
                        {
                            Guid id = TC_Shared.CNullGuid(ndr["invoiceid"]);
                            responseID = TC_Shared.CNullGuid(ndr["responseid"]);

                            LoadTransaction(id);
                        }
                    }
                    conn.Close();
                }
            }
            return responseID;*/
        }

        /// <summary>
        /// Loads an existing transaction for viewing
        /// </summary>
        /// <returns><c>true</c>, if transaction was loaded, <c>false</c> otherwise.</returns>
        /// <param name="invoiceID">ID of the invoice.</param>
        public bool LoadTransaction(EGlobalSubmission eglobalsubmission)
        {
            return LoadTransaction(eglobalsubmission, true);
        }

        /// <summary>
        /// Loads the transaction for submission
        /// </summary>
        /// <returns><c>true</c>, if transaction was loaded, <c>false</c> otherwise.</returns>
        /// <param name="invoiceID">ID of the invoice.</param>
        /// <param name="view">If set to <c>true</c> view.</param>
        public bool LoadTransaction(EGlobalSubmission eglobalsubmission, bool view)
        {
            bool result = false;
            if (eglobalsubmission != null)
            {
                GetPolicyRisks();

                GetPolicy(eglobalsubmission.EGlobalSubmissionClientProgramme.Agreements.Where(cpam => cpam.MasterAgreement).FirstOrDefault(), false);

                EGlobalPolicy.Policy.PolicyDateTime = eglobalsubmission.DateCreated.Value;
                EGlobalPolicy.Policy.EffectiveDate = eglobalsubmission.EGlobalSubmissionClientProgramme.Agreements.Where(cpam => cpam.MasterAgreement).FirstOrDefault().InceptionDate;

                result = LoadTransactionTerms(eglobalsubmission);
            }

            return result;

            //throw new Exception("LoadTransaction - Not yet implemented");
            /*bool result = false;
            using (NpgsqlConnection conn = TC_Shared.GetSqlConnection())
            {
                string strCmd = "SELECT * FROM tbleglobalinvoicesubmission WHERE invoiceID = @InvoiceID;";
                using (NpgsqlCommand cmd = new NpgsqlCommand(strCmd, conn))
                {
                    cmd.Parameters.Add("@InvoiceID", NpgsqlDbType.Uuid).Value = invoiceID;
                    try
                    {
                        conn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                SetPaymentType((PaymentType)TC_Shared.CNullInt(dr["paymenttype"], 0));
                                SetTransactionType((TransactionType)TC_Shared.CNullInt(dr["trancode"], 0));

                                result = LoadTransactionTerms(invoiceID);

                                if (view)
                                {
                                    Policy.ExternalSystemContractID = TC_Shared.CNullStr(dr["contractno"]);
                                    Policy.PolicyDateTime = TC_Shared.CNullableDate(dr["datesubmitted"]).GetValueOrDefault();
                                    Policy.ExternalSystemVersionNumberString = TC_Shared.CNullStr(dr["versionno"]);
                                }

                                CalculateInvoiceSummary(Policy, TC_Shared.CNullDec(dr["brokerfee"], 0m));
                                Policy.EffectiveDate = TC_Shared.CNullableDate(dr["effectivedate"]).GetValueOrDefault();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Error loading EGlobal Transaction", ex.ToString());
                    }
                    finally
                    {
                        if (conn.State != ConnectionState.Closed)
                            conn.Close();
                    }
                }
            }
            return result;*/
        }

        bool LoadTransactionTerms(EGlobalSubmission eglobalsubmission)
        {
            bool result = false;

            var PolicyRisks = new List<EBixPolicyRisk>();
            List<EBixSubAgent> subAgents = new List<EBixSubAgent>();

            EBixPolicyRisk risk = new EBixPolicyRisk();
            // load from db
            //risk.TCClassOfBusiness = TC_Shared.CNullGuid(dr["classOfBusinessID"]);
            risk.CoyPremium = eglobalsubmission.EGlobalSubmissionTerms.FirstOrDefault().ESTPremium;
            risk.CEQuake = eglobalsubmission.EGlobalSubmissionTerms.FirstOrDefault().ESTNDPremium;
            risk.BrokerAmountDue = eglobalsubmission.EGlobalSubmissionTerms.FirstOrDefault().ESTBrokerage;
            risk.BrokerAmountRate = eglobalsubmission.EGlobalSubmissionTerms.FirstOrDefault().ESTBrokerageRate;
            risk.BrokerCeqDue = eglobalsubmission.EGlobalSubmissionTerms.FirstOrDefault().ESTNDBrokerage;
            risk.BrokerCeqDueRate = eglobalsubmission.EGlobalSubmissionTerms.FirstOrDefault().ESTNDBrokerageRate;
            risk.LeviesA = eglobalsubmission.EGlobalSubmissionTerms.FirstOrDefault().ESTEQC;
            risk.LeviesB = eglobalsubmission.EGlobalSubmissionTerms.FirstOrDefault().ESTFSL;
            // calculate remaining values
            risk.GSTPremium = (risk.CoyPremium + risk.CEQuake + risk.LeviesA + risk.LeviesB) * EGlobalPolicy.ClientProgramme.BaseProgramme.TaxRate;
            //TCPolicy.Product.TaxRate.GetValueOrDefault();
            risk.GSTBrokerage = (risk.BrokerAmountDue + risk.BrokerCeqDue) * EGlobalPolicy.ClientProgramme.BaseProgramme.TaxRate;
            //TCPolicy.Product.TaxRate.GetValueOrDefault();
            risk.BSCAmount = 0m;
            risk.BscGST = 0m;
            risk.DueByClient = Math.Round(risk.CoyPremium + risk.GSTPremium + risk.CEQuake + risk.LeviesA + risk.LeviesB, 2);
            // find subcover and riskcodes
            //EGlobalPolicyRiskConfig config = EGlobalPolicy.EGlobalPolicyRiskConfig.FirstOrDefault();
            //gv_objPolicyRisksConfigs.FirstOrDefault(c => c.ClassOfBusinessID == risk.TCClassOfBusiness);
            risk.RiskCode = eglobalsubmission.EGlobalSubmissionPackage.PackageProducts.FirstOrDefault().PackageProductRiskCode;
            risk.SubCoverString = eglobalsubmission.EGlobalSubmissionPackage.PackageProducts.FirstOrDefault().PackageProductSubCover;

            PolicyRisks.Add(risk);

            subAgents.AddRange(LoadTransactionSubAgents(eglobalsubmission, risk));

            result = true;

            // Create the Insurers
            GetInsurers();

            //GetPolicy(eglobalsubmission.EGlobalSubmissionClientProgramme.Agreements.Where(cpam => cpam.MasterAgreement).FirstOrDefault());

            // Create the SubAgents
            GetSubAgents();

            EGlobalPolicy.SubAgents = subAgents;

            return result;

            //throw new Exception("LoadTransactionTerms - Not yet implemented");
            /*bool result = false;

            using (NpgsqlConnection conn = TC_Shared.GetSqlConnection())
            {
                string strCmd = "SELECT * FROM tbleglobalinvoicesubmissionterm WHERE invoiceID = @InvoiceID;";
                using (NpgsqlCommand cmd = new NpgsqlCommand(strCmd, conn))
                {
                    cmd.Parameters.Add("@InvoiceID", NpgsqlDbType.Uuid).Value = invoiceID;
                    try
                    {
                        conn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            PolicyRisks = new List<EBixPolicyRisk>();
                            List<EBixSubAgent> subAgents = new List<EBixSubAgent>();

                            while (dr.Read())
                            {
                                result = false;
                                EBixPolicyRisk risk = new EBixPolicyRisk();
                                // load from db
                                risk.TCClassOfBusiness = TC_Shared.CNullGuid(dr["classOfBusinessID"]);
                                risk.CoyPremium = TC_Shared.CNullDec(dr["premium"], 0m);
                                risk.CEQuake = TC_Shared.CNullDec(dr["ndpremium"], 0m);
                                risk.BrokerAmountDue = TC_Shared.CNullDec(dr["brokerage"], 0m);
                                risk.BrokerAmountRate = TC_Shared.CNullDec(dr["brokeragerate"], 0m);
                                risk.BrokerCeqDue = TC_Shared.CNullDec(dr["ndbrokerage"], 0m);
                                risk.BrokerCeqDueRate = TC_Shared.CNullDec(dr["ndbrokeragerate"], 0m);
                                risk.LeviesA = TC_Shared.CNullDec(dr["eqc"], 0m);
                                risk.LeviesB = TC_Shared.CNullDec(dr["fsl"], 0m);
                                // calculate remaining values
                                risk.GSTPremium = (risk.CoyPremium + risk.CEQuake + risk.LeviesA + risk.LeviesB) *
                                TCPolicy.Product.TaxRate.GetValueOrDefault();
                                risk.GSTBrokerage = (risk.BrokerAmountDue + risk.BrokerCeqDue) *
                                TCPolicy.Product.TaxRate.GetValueOrDefault();
                                risk.BSCAmount = 0m;
                                risk.BscGST = 0m;
                                risk.DueByClient = TC_Shared.RoundDecimal(risk.CoyPremium + risk.GSTPremium + risk.CEQuake +
                                risk.LeviesA + risk.LeviesB);
                                // find subcover and riskcodes
                                EGlobalPolicyRiskConfig config =
                                    gv_objPolicyRisksConfigs.FirstOrDefault(c => c.ClassOfBusinessID == risk.TCClassOfBusiness);
                                risk.RiskCode = config.RiskCode;
                                risk.SubCoverString = config.SubCover;

                                PolicyRisks.Add(risk);

                                subAgents.AddRange(LoadTransactionSubAgents(TC_Shared.CNullGuid(dr["termid"]), risk));

                                result = true;
                            }

                            CreatePolicyInvoice(PolicyRisks);

                            SubAgents = subAgents;
                        }
                    }
                    catch (Exception ex)
                    {
                        TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Error loading EGlobal Transaction Term", ex.ToString());
                    }
                    finally
                    {
                        if (conn.State != ConnectionState.Closed)
                            conn.Close();
                    }
                }
            }
            return result;*/
        }

        List<EBixSubAgent> LoadTransactionSubAgents(EGlobalSubmission eglobalsubmission, EBixPolicyRisk risk)
        {

            List<EBixSubAgent> subAgents = new List<EBixSubAgent>();

            decimal amount = risk.CoyPremium + risk.CEQuake + risk.LeviesA + risk.LeviesB;
            EBixSubAgent agent = new EBixSubAgent();
            agent.ID = eglobalsubmission.EGlobalSubmissionSubagentTerms.FirstOrDefault().Id;
            agent.CoverNumber = 0;
            agent.VerNo = 0;
            agent.SubCover = risk.SubCover;
            agent.Percent = eglobalsubmission.EGlobalSubmissionSubagentTerms.FirstOrDefault().ESSubagentTSubPercentComm;
            agent.GSTFlag = eglobalsubmission.EGlobalSubmissionSubagentTerms.FirstOrDefault().ESSubagentTGSTRegistered;
            agent.SubAgent = eglobalsubmission.EGlobalSubmissionSubagentTerms.FirstOrDefault().ESSubagentTSubCode;
            agent.CalcAmount = eglobalsubmission.EGlobalSubmissionSubagentTerms.FirstOrDefault().ESSubagentTSubAmount / (agent.Percent / 100);
            agent.AmountCeded = eglobalsubmission.EGlobalSubmissionSubagentTerms.FirstOrDefault().ESSubagentTSubAmount; // agent.CalcAmount * (agent.Percent / 100)
            if (agent.GSTFlag == -1)
                agent.GSTCeded = agent.AmountCeded * EGlobalPolicy.ClientProgramme.BaseProgramme.TaxRate;
            subAgents.Add(agent);

            return subAgents;
            //throw new Exception("LoadTransactionSubAgents - Not yet implemented");
            /*List<EBixSubAgent> subAgents = new List<EBixSubAgent>();

            using (NpgsqlConnection conn = TC_Shared.GetSqlConnection())
            {
                string strCmd = "SELECT * FROM tbleglobalinvoicesubmissionsubagentterm WHERE risktermid = @RiskTermID;";
                using (NpgsqlCommand cmd = new NpgsqlCommand(strCmd, conn))
                {
                    cmd.Parameters.Add("@RiskTermID", NpgsqlDbType.Uuid).Value = riskid;

                    conn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            decimal amount = risk.CoyPremium + risk.CEQuake + risk.LeviesA + risk.LeviesB;
                            EBixSubAgent agent = new EBixSubAgent();
                            agent.ID = TC_Shared.CNullGuid(dr["subagenttermid"]);
                            agent.CoverNumber = 0;
                            agent.VerNo = 0;
                            agent.SubCover = risk.SubCover;
                            agent.Percent = TC_Shared.CNullDec(dr["subpercentcomm"], 0m);
                            agent.GSTFlag = TC_Shared.CNullInt(dr["gstregistered"], 0);
                            agent.SubAgent = TC_Shared.CNullStr(dr["subcode"]);
                            agent.CalcAmount = TC_Shared.CNullDec(dr["subamount"], 0m);
                            agent.AmountCeded = agent.CalcAmount * (agent.Percent / 100);
                            if (agent.GSTFlag == -1)
                                agent.GSTCeded = agent.AmountCeded * TCPolicy.Product.TaxRate.GetValueOrDefault();
                            subAgents.Add(agent);
                        }
                    }

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }
            }

            return subAgents;*/
        }

        /// <summary>
        /// Saves the current policy transaction details
        /// </summary>
        public Guid SaveTransaction()
        {
            throw new Exception("SaveTransaction - Not yet implemented");
            /*Guid id = Guid.NewGuid();
            int result = 0;
            using (NpgsqlConnection conn = TC_Shared.GetSqlConnection())
            {
                string strCmd = "INSERT INTO tbleglobalinvoicesubmission (invoiceID, quoteID, datesubmitted, contractno, " +
                                "versionno, trancode, transactiondetail, paymenttype, brokerfee, effectivedate) VALUES (@InvoiceID, " +
                                "@QuoteID, @DateSubmitted, @ContractNo, @VersionNo, @TranCode, @TransactionDetail, @PaymentType, @BrokerFee, " +
                                "@EffectiveDate);";
                using (NpgsqlCommand sqlcmd = new NpgsqlCommand(strCmd, conn))
                {
                    sqlcmd.Parameters.Add("@InvoiceID", NpgsqlDbType.Uuid).Value = id;
                    sqlcmd.Parameters.Add("@QuoteID", NpgsqlDbType.Uuid).Value = TCPolicy.ID;
                    sqlcmd.Parameters.Add("@DateSubmitted", NpgsqlDbType.Timestamp).Value = Policy.PolicyDateTime;
                    sqlcmd.Parameters.Add("@ContractNo", NpgsqlDbType.Varchar).Value = Policy.ExternalSystemContractID;
                    sqlcmd.Parameters.Add("@VersionNo", NpgsqlDbType.Integer).Value = Policy.ExternalSystemVersionNumber;
                    sqlcmd.Parameters.Add("@TranCode", NpgsqlDbType.Varchar).Value = Policy.TransactionType.ToString();
                    sqlcmd.Parameters.Add("@TransactionDetail", NpgsqlDbType.Varchar).Value = "";   // TODO
                    sqlcmd.Parameters.Add("@PaymentType", NpgsqlDbType.Integer).Value = (int)gv_paymentType;
                    sqlcmd.Parameters.Add("@BrokerFee", NpgsqlDbType.Integer).Value = Policy.BSCAmount;
                    sqlcmd.Parameters.Add("@EffectiveDate", NpgsqlDbType.Timestamp).Value = Policy.EffectiveDate;
                    try
                    {
                        conn.Open();
                        result = sqlcmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Error saving EGlobal Transaction", ex.ToString());
                        id = Guid.Empty;
                    }
                    finally
                    {
                        if (conn.State != ConnectionState.Closed)
                            conn.Close();
                    }
                }
            }
            if (result == 1)
                SaveTransactionTerms(id);
            return id;*/
        }

        public void SaveTransactionTerms(EGlobalSubmission eglobalSubmission, IUnitOfWork _unitOfWork)
        {
            foreach (EBixPolicyRisk pr in EGlobalPolicy.PolicyRisks)
            {
                try
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        EGlobalSubmissionTerm eGlobalSubmissionTerm = new EGlobalSubmissionTerm(CurrentUser);
                        eGlobalSubmissionTerm.ESTEGlobalSubmission = eglobalSubmission;
                        eGlobalSubmissionTerm.ESTPremium = pr.CoyPremium;
                        eGlobalSubmissionTerm.ESTNDPremium = pr.CEQuake;
                        eGlobalSubmissionTerm.ESTBrokerage = pr.BrokerAmountDue;
                        eGlobalSubmissionTerm.ESTNDBrokerage = pr.BrokerCeqDue;
                        eGlobalSubmissionTerm.ESTEQC = pr.LeviesA;
                        eGlobalSubmissionTerm.ESTFSL = pr.LeviesB;
                        eGlobalSubmissionTerm.ESTEGlobalSubmissionPackage = eglobalSubmission.EGlobalSubmissionPackage;
                        eglobalSubmission.EGlobalSubmissionTerms.Add(eGlobalSubmissionTerm);

                        if (EGlobalPolicy.SubAgents != null && EGlobalPolicy.SubAgents.Count > 0)
                            SaveTransactionSubAgents(pr, eglobalSubmission, _unitOfWork, eGlobalSubmissionTerm);

                        uow.Commit();

                    }

                }
                catch (Exception ex)
                {
                    //TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Error saving EGlobal Transaction Terms", ex.ToString());
                } 
                
            }

            //throw new Exception("SaveTransactionTerms - Not yet implemented");
            /*using (NpgsqlConnection conn = TC_Shared.GetSqlConnection())
            {
                string strCmd = "INSERT INTO tbleglobalinvoicesubmissionterm (termID, invoiceID, classOfBusinessID, " +
                                "premium, ndpremium, brokerage, ndbrokerage, eqc, fsl) VALUES (@TermID, @InvoiceID, @ClassOfBusinessID, " +
                                "@Premium, @NDPremium, @Brokerage, @NDBrokerage, @EQC, @FSL);";
                using (NpgsqlCommand sqlcmd = new NpgsqlCommand(strCmd, conn))
                {
                    sqlcmd.Parameters.Add("@InvoiceID", NpgsqlDbType.Uuid).Value = transactionID;
                    foreach (EBixPolicyRisk pr in PolicyRisks)
                    {
                        pr.ID = Guid.NewGuid();
                        sqlcmd.Parameters.Add("@TermID", NpgsqlDbType.Uuid).Value = pr.ID;
                        sqlcmd.Parameters.Add("@ClassOfBusinessID", NpgsqlDbType.Uuid).Value = pr.TCClassOfBusiness;
                        sqlcmd.Parameters.Add("@Premium", NpgsqlDbType.Numeric).Value = pr.CoyPremium;
                        sqlcmd.Parameters.Add("@NDPremium", NpgsqlDbType.Numeric).Value = pr.CEQuake;
                        sqlcmd.Parameters.Add("@Brokerage", NpgsqlDbType.Numeric).Value = pr.BrokerAmountDue;
                        sqlcmd.Parameters.Add("@NDBrokerage", NpgsqlDbType.Numeric).Value = pr.BrokerCeqDue;
                        sqlcmd.Parameters.Add("@EQC", NpgsqlDbType.Numeric).Value = pr.LeviesA;
                        sqlcmd.Parameters.Add("@FSL", NpgsqlDbType.Numeric).Value = pr.LeviesB;
                        try
                        {
                            conn.Open();
                            sqlcmd.ExecuteNonQuery();

                            if (SubAgents != null && SubAgents.Count > 0)
                                SaveTransactionSubAgents(pr, transactionID);
                        }
                        catch (Exception ex)
                        {
                            TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Error saving EGlobal Transaction Terms", ex.ToString());
                        }
                        finally
                        {
                            if (conn.State != ConnectionState.Closed)
                                conn.Close();
                        }
                    }
                }
            }*/
        }

        void SaveTransactionSubAgents(EBixPolicyRisk risk, EGlobalSubmission eglobalSubmission, IUnitOfWork _unitOfWork, EGlobalSubmissionTerm eglobalSubmissionTerm)
        {
            foreach (EBixSubAgent agent in EGlobalPolicy.SubAgents.Where(suba => suba.SubCover == risk.SubCover))
            {
                try
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        EGlobalSubmissionSubagentTerm eGlobalSubmissionSubagentTerm = new EGlobalSubmissionSubagentTerm(CurrentUser);
                        eGlobalSubmissionSubagentTerm.ESSubagentTEGlobalSubmission = eglobalSubmission;
                        eGlobalSubmissionSubagentTerm.ESSubagentTSubCode = agent.SubAgent;
                        eGlobalSubmissionSubagentTerm.ESSubagentTEGlobalSubmissionTerm = eglobalSubmissionTerm;
                        eGlobalSubmissionSubagentTerm.ESSubagentTGSTRegistered = agent.GSTFlag;
                        eGlobalSubmissionSubagentTerm.ESSubagentTSubPercentComm = agent.Percent;
                        eGlobalSubmissionSubagentTerm.ESSubagentTSubAmount = agent.AmountCeded; //CalcAmount
                        eglobalSubmission.EGlobalSubmissionSubagentTerms.Add(eGlobalSubmissionSubagentTerm);
                        uow.Commit();

                    }
                }
                catch (Exception ex)
                {
                    //TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Error saving EGlobal Transaction Subagent", ex.ToString());
                }
            }

            //throw new Exception("SaveTransactionSubAgents - Not yet implemented");
            /*string strCmd = "INSERT INTO tbleglobalinvoicesubmissionsubagentterm (subagenttermid, subcode, invoiceid, " +
                            "risktermid, gstregistered, subpercentcomm, subamount) VALUES (@ID, @SubCode, @InvoiceID, @RiskID, @GSTFlag, " +
                "@PercentComm, @SubAmount);";
            using (NpgsqlConnection conn = TC_Shared.GetSqlConnection())
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(strCmd, conn))
                {
                    sqlCmd.Parameters.Add("@RiskID", NpgsqlDbType.Uuid).Value = risk.ID;
                    foreach (EBixSubAgent agent in SubAgents.Where(a => a.SubCover == risk.SubCover))
                    {
                        sqlCmd.Parameters.Add("@ID", NpgsqlDbType.Uuid).Value = Guid.NewGuid();
                        sqlCmd.Parameters.Add("@SubCode", NpgsqlDbType.Varchar).Value = agent.SubAgent;
                        sqlCmd.Parameters.Add("@InvoiceID", NpgsqlDbType.Uuid).Value = invoiceID;
                        sqlCmd.Parameters.Add("@GSTFlag", NpgsqlDbType.Integer).Value = agent.GSTFlag;
                        sqlCmd.Parameters.Add("@PercentComm", NpgsqlDbType.Numeric).Value = agent.Percent;
                        sqlCmd.Parameters.Add("@SubAmount", NpgsqlDbType.Numeric).Value = agent.CalcAmount;

                        try
                        {
                            conn.Open();
                            sqlCmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Error saving EGlobal Transaction Subagent", ex.ToString());
                        }
                        finally
                        {
                            if (conn.State != ConnectionState.Closed)
                                conn.Close();
                        }
                    }
                }
            }*/
        }

        #endregion


        #region Utility functions

        void GetPolicyRisks()
        {
            List<EBixPolicyRisk> risks = new List<EBixPolicyRisk>();

            foreach (PackageProduct packageProduct in EGlobalPolicy.Package.PackageProducts)
            {
                EGlobalPolicy.EGlobalPolicyRiskConfig.Add(new EGlobalPolicyRiskConfig(packageProduct));
            }
                foreach(ClientAgreement clientAgreement in EGlobalPolicy.ClientProgramme.Agreements)
                {
                    foreach (PackageProduct packageProducts in EGlobalPolicy.Package.PackageProducts)
                    {
                        if (clientAgreement.Product.Id == packageProducts.PackageProductProduct.Id)
                        {
                            foreach (ClientAgreementTerm clientAgreementTerm in clientAgreement.ClientAgreementTerms.Where(agree => agree.Bound == true))
                            {
                                risks.Add(CreatePolicyRisk(packageProducts, clientAgreementTerm));
                            }
                        }
                    }
                    
                }
           

            EGlobalPolicy.PolicyRisks = risks;

            /*   List<EGlobalPolicy.EGlobalPolicyConfig> riskConfigsInclude = gv_objPolicyRisksConfigs.Where(c => c.AlwaysInclude).ToList<EGlobalPolicy.EGlobalPolicyConfig>();
               List<EGlobalPolicyRiskConfig> riskConfigsExclude = gv_objPolicyRisksConfigs.Where(c => !c.AlwaysInclude).ToList<EGlobalPolicyRiskConfig>();
               foreach (EGlobalPolicyRiskConfig config in riskConfigsInclude)
               {
                   term = terms.FirstOrDefault(t => t.ClassOfBusinessID == config.ClassOfBusinessID);
                   if (term != null)
                       risks.Add(CreatePolicyRisk(policy, term, config));
               }

               foreach (EGlobalPolicyRiskConfig config in riskConfigsExclude)
               {
                   term = terms.FirstOrDefault(t => t.ClassOfBusinessID == config.ClassOfBusinessID);
                   if (term != null)
                   {
                       if (config.MergeWithCOBID != Guid.Empty)
                           MergePolicyRisks(risks, policy, term, new TCClassOfBusiness(config.MergeWithCOBID).MergeCode);
                       else
                           risks.Add(CreatePolicyRisk(policy, term, config));
                   }
               }
           }

           /*List<EBixPolicyRisk> risks = new List<EBixPolicyRisk>();

           foreach (TCQuote quote in gv_objPolicies)
           {
               risks.AddRange(GetPolicyRisks(quote));
           }*/
        }

        void AutoDetectTransactionType(ClientAgreement objClientAgreement)
        {
            ClientInformationSheet ClientUIS = EGlobalPolicy.ClientProgramme.InformationSheet;
            ClientInformationSheet previousClientUIS = null;
            // assume new transaction by default
            gv_transactionType = 1;

            bool bolTransactionTypeCalculated = false;

            do
            {
                if (ClientUIS.PreviousInformationSheet == null && ClientUIS.IsRenewawl)
                {
                    gv_transactionType = 3;//renew
                    break;
                }
                // try and find the original policy
                else if (ClientUIS.PreviousInformationSheet != null)
                {
                    previousClientUIS = ClientUIS.PreviousInformationSheet;

                    if (previousClientUIS == null)
                        break;

                    // if the previous policy's scheme project isn't using EGlobal, then stop 
                    if (!previousClientUIS.Programme.BaseProgramme.UsesEGlobal)
                        break;


                    if (previousClientUIS.Status != "Bound and invoiced")
                        break;

                    if (!bolTransactionTypeCalculated)
                    {
                        if (ClientUIS.PreviousInformationSheet.IsRenewawl)
                            gv_transactionType = 3;
                        else
                            gv_transactionType = 2;

                        bolTransactionTypeCalculated = true;
                    }

                    // get its reference id
                    gv_strUISReference = ClientUIS.ReferenceId;
                    gv_strOriginalAgreementReference = previousClientUIS.Programme.Agreements.Where(cpam => cpam.MasterAgreement).FirstOrDefault().ReferenceId;
                    
                    // otherise check to see if there if the current policy is the same as the original policy
                    if (ClientUIS.Id == previousClientUIS.Id)
                    {
                        // and if so, set the transaction type to true (since any existing policies haven't been submitted)
                        gv_transactionType = 1;
                        break;
                    }

                    // if there is a policy attached the the proposal's parent, set it as the current policy
                    ClientUIS = previousClientUIS;
                    
                }
                else
                    ClientUIS = null;
            } while (ClientUIS != null);
        }

        /*/// <summary>
        /// Sets the type of the transaction.
        /// </summary>
        /// <param name="type">Enum transaction type</param>
        public void SetTransactionType(TransactionType transType)
        {
            // if we are reversing, set type to endorse, and set the payment direction to be negative
            if (transType == TransactionType.Reverse)
            {
                gv_decPaymentDirection = -1;
                transType = TransactionType.Endorse;
            }
            gv_transactionType = transType;
        }*/

        public void Setup()
        {

            EGlobalPolicy.EGlobalPolicyConfig = new EGlobalPolicyConfig()
            {

                RiskCode = EGlobalPolicy.Package.RiskCode,
                Branch = EGlobalPolicy.Package.Branch,
                DescriptionNew = EGlobalPolicy.Package.DescriptionNew,
                DescriptionChange = EGlobalPolicy.Package.DescriptionChange,
                DescriptionRenew = EGlobalPolicy.Package.DescriptionRenew,
                DescriptionCancel = EGlobalPolicy.Package.DescriptionCancel,
                DescriptionLapse = EGlobalPolicy.Package.DescriptionLapse,
                StatementNew = EGlobalPolicy.Package.StatementNew,
                StatementChange = EGlobalPolicy.Package.StatementChange,
                StatementRenew = EGlobalPolicy.Package.StatementRenew,
                StatementCancel = EGlobalPolicy.Package.StatementCancel,
                ContractCode = EGlobalPolicy.Package.ContractCode,
                HasInvoicePayment = EGlobalPolicy.Package.HasInvoicePayment,
                HasCCPayment = EGlobalPolicy.Package.HasCCPayment,
                HasPremiumPayment = EGlobalPolicy.Package.HasPremiumPayment,
                FTPFolder = EGlobalPolicy.Package.FTPFolder,
            };

            // set discount multipler to 1 - no discount
            EGlobalPolicy.DiscountRate = 1m;

            // Get Broker Info
            EbixUser = EGlobalPolicy.ClientProgramme.BrokerContactUser.SalesPersonUserName;
            EbixDepartment = EGlobalPolicy.ClientProgramme.BrokerContactUser.DefaultOU.EbixDepartmentCode;

            // Create the Queue
            EGlobalPolicy.Queue = new EBixQueue()
            {
                CustInv = 1,
                User = EbixUser
            };

            // Create the PUF fields
            EGlobalPolicy.PUF = new EBixPUF()
            {
                Element1 = new EBixPUFElement()
                {
                    Caption = "ISOType",
                    Description = "ISOType",
                    Sino = 1
                },
                Element2 = new EBixPUFElement()
                {
                    Caption = "ISOType",
                    Description = "ISOType",
                    Sino = 1
                }
            };

            // Create the client
            EGlobalPolicy.Client = new EBixClient()
            {
                ExtSystemFlag = "Q"
            };
        }

        /// <summary>
        /// Gets the invoice ID associated with the specified contractNo and version number
        /// </summary>
        /// <returns>The invoice ID.</returns>
        /// <param name="contractNo">Contract no.</param>
        /// <param name="version">Version.</param>
        public Guid GetInvoiceID(string contractNo, int version)
        {
            if (EGlobalPolicy == null)
            {
                throw new Exception("EGlobalPolicy is empty");
            }
            return EGlobalPolicy.ClientProgramme.InformationSheet.Product.Id;

            /*Guid invoiceGuid = Guid.Empty;
            using (NpgsqlConnection conn = TC_Shared.GetSqlConnection())
            {
                string strCmd = "SELECT invoiceID FROM tbleglobalinvoicesubmission WHERE contractno = @ContractNo AND " +
                                "versionno = @VersionNo ORDER BY datesubmitted DESC;";   // AND trancode = @TranCode;";
                using (NpgsqlCommand sqlcmd = new NpgsqlCommand(strCmd, conn))
                {
                    sqlcmd.Parameters.Add("@ContractNo", NpgsqlDbType.Varchar).Value = contractNo;
                    sqlcmd.Parameters.Add("@VersionNo", NpgsqlDbType.Integer).Value = version;
                    //sqlcmd.Parameters.Add("@TranCode", NpgsqlDbType.Integer).Value = trancode;
                    try
                    {
                        conn.Open();
                        invoiceGuid = TC_Shared.CNullGuid(sqlcmd.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Error while getting EGlobal Transaction ID", ex.ToString());
                    }
                    finally
                    {
                        if (conn.State != ConnectionState.Closed)
                            conn.Close();
                    }
                }
            }
            return invoiceGuid;*/
        }

        public Guid GetQuoteID()
        {
            if (EGlobalPolicy == null)
            {
                throw new Exception("EGlobalPolicy is empty");
            }
            return EGlobalPolicy.ClientProgramme.InformationSheet.Product.Id;
            /*Guid quoteGuid = Guid.Empty;
            using (NpgsqlConnection conn = TC_Shared.GetSqlConnection())
            {
                string strCmd = "SELECT quoteid FROM tbleglobalinvoicesubmission WHERE contractno = @ContractNo AND " +
                                "versionno = @VersionNo ORDER BY datesubmitted DESC;";   // AND trancode = @TranCode;";
                using (NpgsqlCommand sqlcmd = new NpgsqlCommand(strCmd, conn))
                {
                    sqlcmd.Parameters.Add("@ContractNo", NpgsqlDbType.Varchar).Value = contractNo;
                    sqlcmd.Parameters.Add("@VersionNo", NpgsqlDbType.Integer).Value = version;
                    //sqlcmd.Parameters.Add("@TranCode", NpgsqlDbType.Integer).Value = trancode;
                    try
                    {
                        conn.Open();
                        quoteGuid = TC_Shared.CNullGuid(sqlcmd.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Error while getting Quote ID from EGbolal Transaction", ex.ToString());
                    }
                    finally
                    {
                        if (conn.State != ConnectionState.Closed)
                            conn.Close();
                    }
                }
            }
            return quoteGuid;*/
        }

        /// <summary>
        /// Gets the current transaction version number for this policy
        /// </summary>
        /// <returns>The version count.</returns>
        public int GetVersionCount()
        {
            if (EGlobalPolicy == null)
            {
                throw new Exception("EGlobalPolicy is empty");
            }

            int version = 0;
            if (EGlobalPolicy.ClientProgramme.ClientAgreementEGlobalResponses.Count > 0)
            {
                foreach (EGlobalResponse eGlobalResponse in EGlobalPolicy.ClientProgramme.ClientAgreementEGlobalResponses)
                {
                    if (eGlobalResponse.MasterAgreementReferenceID == gv_strMasterAgreementReference)
                    {
                        version = eGlobalResponse.VersionNumber + 1;
                    }
                }
            }
            
            return version;
        }

        public static void SetResponseID(Guid responseID, Guid invoiceID)
        {
            throw new Exception("SetResponseID - Not yet implemented");
            /*using (NpgsqlConnection conn = TC_Shared.GetSqlConnection())
            {
                string strCmd = "UPDATE tbleglobalinvoicesubmission SET responseid = @ResponseID WHERE invoiceID = @InvoiceID";
                using (NpgsqlCommand sqlcmd = new NpgsqlCommand(strCmd, conn))
                {
                    sqlcmd.Parameters.Add("@ResponseID", NpgsqlDbType.Uuid).Value = responseID;
                    sqlcmd.Parameters.Add("@InvoiceID", NpgsqlDbType.Uuid).Value = invoiceID;
                    try
                    {
                        conn.Open();
                        sqlcmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Error while getting EGlobal Transaction ID", ex.ToString());
                    }
                    finally
                    {
                        if (conn.State != ConnectionState.Closed)
                            conn.Close();
                    }
                }
            }*/
        }

        #endregion


        #region Section Assembly

        protected virtual void GetPolicy(ClientAgreement objClientAgreement, bool canceltran)
        {
            EBixPolicy EBixPolicy = new EBixPolicy();

            // Fill in its fields
            gv_strUISReference = EGlobalPolicy.ClientProgramme.InformationSheet.ReferenceId;
            if (objClientAgreement.ClientInformationSheet.IsRenewawl && objClientAgreement.ClientInformationSheet.PreviousInformationSheet == null)
            {
                gv_strMasterAgreementReference = objClientAgreement.ClientInformationSheet.Programme.EGlobalExternalContactNumber;
            }
            else if (objClientAgreement.ClientInformationSheet.IsChange && objClientAgreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                gv_strMasterAgreementReference = gv_strOriginalAgreementReference;
            } else { 
                gv_strMasterAgreementReference = objClientAgreement.ReferenceId; 
            }
            
            // Dates
            EBixPolicy.PolicyDateTime = DateTime.Now;
            if (canceltran)
            {
                EBixPolicy.EffectiveDate = TimeZoneInfo.ConvertTimeFromUtc(objClientAgreement.CancelledEffectiveDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone));
            } else
            {
                EBixPolicy.EffectiveDate = TimeZoneInfo.ConvertTimeFromUtc(objClientAgreement.InceptionDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone));
            }
            EBixPolicy.ExpiryDate = TimeZoneInfo.ConvertTimeFromUtc(objClientAgreement.ExpiryDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone));
            EBixPolicy.InceptionDate = TimeZoneInfo.ConvertTimeFromUtc(objClientAgreement.InceptionDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)); 
            EBixPolicy.LastRenewalDate = TimeZoneInfo.ConvertTimeFromUtc(objClientAgreement.InceptionDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)); 
            EBixPolicy.PolicyTime = TimeZoneInfo.ConvertTimeFromUtc(objClientAgreement.InceptionDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone));
            EBixPolicy.TermsofTradeDate = TimeZoneInfo.ConvertTimeFromUtc(objClientAgreement.InceptionDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone));
            EBixPolicy.RenewalDate = TimeZoneInfo.ConvertTimeFromUtc(objClientAgreement.ExpiryDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone));

            // Set variables
            EBixPolicy.Branch = EGlobalPolicy.ClientProgramme.EGlobalBranchCode;
            EBixPolicy.IncomeClass = EGlobalPolicy.IncomeClass;

            EBixPolicy.Department = EbixDepartment;
            EBixPolicy.RiskCode = EGlobalPolicy.Package.RiskCode;
            EBixPolicy.UserIDServ = EbixUser;
            EBixPolicy.CreatedByUser = EbixUser;
            EBixPolicy.StatementDescription = EGlobalPolicy.GetDescription2;    // Invoice description
            EBixPolicy.MultiRisk = EGlobalPolicy.MultiRisk;                                   // set multisk flag
            EBixPolicy.ExternalSystemContractID = String.Format("TCDE-{0}-{1}", gv_strMasterAgreementReference, EGlobalPolicy.ExtensionCode);
            if (objClientAgreement.ClientInformationSheet.IsRenewawl && objClientAgreement.ClientInformationSheet.PreviousInformationSheet == null &&
                !string.IsNullOrEmpty(objClientAgreement.ClientInformationSheet.Programme.EGlobalExternalContactNumber))
            {
                EBixPolicy.ExternalSystemContractID = objClientAgreement.ClientInformationSheet.Programme.EGlobalExternalContactNumber;
            }
            EBixPolicy.Description = EGlobalPolicy.GetDescription1;
            EBixPolicy.ClientNumber = EGlobalPolicy.ClientProgramme.EGlobalClientNumber;
            EBixPolicy.TransactionType = gv_transactionType;
            EBixPolicy.PremiumFunded = (int)EGlobalPolicy.PremiumFunding;

            // Set defaults
            EBixPolicy.Internal = "N";
            EBixPolicy.Company = "NZL";
            EBixPolicy.CountryCode = "NZL";
            EBixPolicy.DirectCredit = 0;
            EBixPolicy.BillingCurrencyRate = 1;
            EBixPolicy.ExternalSystem = -1;
            EBixPolicy.ExternalSystemVersionNumber = GetVersionCount();
            EBixPolicy.VersionNumber = EBixPolicy.BillingCurrencyRate.ToString();
            EBixPolicy.LastMajorTrans = "N";
            EBixPolicy.TermsofTradeCode = "NRML";
            EBixPolicy.Renewable = -1;
            //ep.TransactionType = (int)TransactionTypes.New;

            var BrokerFeeTotal = 0M;
            // Sum all of the policy risks up
            foreach (EBixPolicyRisk risk in EGlobalPolicy.PolicyRisks)
            {
                EBixPolicy.DueByClient += risk.DueByClient;
                EBixPolicy.BrokerAmountDue += risk.BrokerAmountDue;
                EBixPolicy.BrokerCeqDue += risk.BrokerCeqDue;
                EBixPolicy.BSCAmount += risk.BSCAmount;
                //BrokerFeeTotal += EBixPolicy.BSCAmount;
                EBixPolicy.CEQuake += risk.CEQuake;
                EBixPolicy.BscGST += risk.BscGST;
                EBixPolicy.GSTPremium += risk.GSTPremium;
                EBixPolicy.LeviesA += risk.LeviesA;
                EBixPolicy.LeviesB += risk.LeviesB;
                EBixPolicy.CoyPremium += (risk.CoyPremium - risk.LeviesA - risk.LeviesB - risk.CEQuake);
                EBixPolicy.GSTBrokerage += risk.GSTBrokerage;
            }

            if (canceltran)
            {
                BrokerFeeTotal = 0M;
            } else
            {
                BrokerFeeTotal = objClientAgreement.BrokerFee;
            }
            
            CalculateInvoiceSummary(EBixPolicy, BrokerFeeTotal);

            EGlobalPolicy.Policy = EBixPolicy;
        }

        public void CalculateInvoiceSummary(EBixPolicy ep, decimal brokerFee)
        {
            // Caculate the final few fields, and update where appropriate
            EGlobalPolicy.SurchargeRate = EGlobalPolicy.ClientProgramme.BaseProgramme.SurchargeRate;
            decimal taxRate = EGlobalPolicy.ClientProgramme.BaseProgramme.TaxRate;
            decimal surchargeGST = EGlobalPolicy.SurchargeRate * (1 + taxRate);

            ep.BSCAmount = brokerFee;

            ep.BscGST = Math.Round(ep.BSCAmount * taxRate, 2);
            //ep.DueByClient = ep.CoyPremium + ep.BSCAmount + ep.GSTPremium + ep.BscGST;
            ep.DueByClient = ep.CoyPremium + ep.CEQuake + ep.LeviesA + ep.LeviesB + ep.BSCAmount + ep.GSTPremium + ep.BscGST;
            ep.SPCFee = Math.Round(ep.DueByClient * EGlobalPolicy.SurchargeRate, 2);

            decimal spcGST = Math.Round(ep.SPCFee * taxRate, 2);

            ep.BscGST += spcGST;
            ep.DueByClient += ep.SPCFee;
        }

        protected virtual EBixPolicyRisk CreatePolicyRisk(PackageProduct packageProduct, ClientAgreementTerm clientAgreementTerm)
        {
            //string riskCode, subCover;
            // Get the risk and subcover codes
            //GetRiskAndSubCoverCodes(term.ClassOfBusiness.MergeCode, out riskCode, out subCover);
            // Create a new Policy Risk
            EBixPolicyRisk pr = new EBixPolicyRisk();
            pr.TCMergeCode = clientAgreementTerm.MergeCode;
            pr.TCClassOfBusiness = clientAgreementTerm.Id;
            pr.RiskCode = packageProduct.PackageProductRiskCode;
            pr.SubCoverString = packageProduct.PackageProductSubCover;
            
            if (clientAgreementTerm.ClientAgreement.ClientInformationSheet.IsChange && clientAgreementTerm.ClientAgreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                pr.CoyPremium = (clientAgreementTerm.PremiumDiffer * EGlobalPolicy.DiscountRate);
                pr.LeviesB = clientAgreementTerm.FSLDiffer;     //fsl;
                //pr.BrokerAmountDue = clientAgreementTerm.BrokerageDiffer;
                pr.BrokerAmountDue = clientAgreementTerm.PremiumDiffer * clientAgreementTerm.ClientAgreement.Brokerage / 100;
            } else
            {
                pr.CoyPremium = (clientAgreementTerm.Premium * EGlobalPolicy.DiscountRate);
                pr.LeviesB = clientAgreementTerm.FSL;     //fsl;
                //pr.BrokerAmountDue = clientAgreementTerm.Brokerage;
                pr.BrokerAmountDue = clientAgreementTerm.Premium * clientAgreementTerm.ClientAgreement.Brokerage / 100;
            }

            foreach(ClientAgreementTermExtension extension in clientAgreementTerm.ClientAgreement.ClientAgreementTermExtensions.Where(ext => ext.Bound  == true))
            {
                pr.CoyPremium += (extension.Premium * EGlobalPolicy.DiscountRate);
                pr.BrokerAmountDue += extension.Premium * clientAgreementTerm.ClientAgreement.Brokerage / 100;
            }

            pr.GSTPremium = (pr.CoyPremium * packageProduct.PackageProductProduct.TaxRate);
            pr.GSTBrokerage = (pr.BrokerAmountDue * packageProduct.PackageProductProduct.TaxRate);
            pr.DueByClient = (pr.CoyPremium + pr.GSTPremium);
            pr.BrokerCeqDue = 0m;
            pr.BSCAmount = 0m;
            pr.CEQuake = 0m;
            pr.BscGST = 0m;
            pr.LeviesA = 0m;     //eqc;
            pr.BrokerAmountRate = pr.BrokerCeqDueRate = clientAgreementTerm.Brokerage;
            /************************/

            return pr;
        }

        /// <summary>
        /// Merges a given policy risk into another policy risk.
        /// </summary>
        /// <param name="risks">The current policy risks</param>
        /// <param name="policy">The current policy</param>
        /// <param name="term">The quote term to be changed into a policy risk and merged</param>
        /// <param name="code">The merge code of the line to </param>
        /*protected virtual void MergePolicyRisks(List<EBixPolicyRisk> risks, TCQuote policy, TCQuoteTerm term, string code)
        {
            EBixPolicyRisk pr = risks.FirstOrDefault(risk => risk.TCMergeCode == code);
            decimal premium = TC_Shared.RoundDecimal(gv_decPaymentDirection * term.Premium.GetValueOrDefault());
            decimal gstPremium = TC_Shared.RoundDecimal(premium * policy.Product.TaxRate.GetValueOrDefault());
            decimal brokerAmount = TC_Shared.RoundDecimal((policy.Brokerage.GetValueOrDefault() / 100m) * premium);
            pr.CoyPremium += premium;
            pr.GSTPremium += gstPremium;
            pr.BrokerAmountDue += brokerAmount;
            pr.GSTBrokerage += TC_Shared.RoundDecimal(brokerAmount * policy.Product.TaxRate.GetValueOrDefault());
            pr.DueByClient += TC_Shared.RoundDecimal(premium + gstPremium);
        }*/

        /// <summary>
        /// Gets a list of all of the insurers for this policy
        /// </summary>
        /// <returns>The insurers.</returns>
        protected void GetInsurers()
        {
            List<EBixInsurer> Insurers = new List<EBixInsurer>();
            foreach (EBixPolicyRisk risk in EGlobalPolicy.PolicyRisks)
            {
                foreach(EGlobalPolicyRiskConfig eGlobalPolicyRiskConfig in EGlobalPolicy.EGlobalPolicyRiskConfig)
                {
                    if (eGlobalPolicyRiskConfig.RiskCode == risk.RiskCode)
                    {
                        foreach (EGlobalInsurerConfig eGlobalInsurerConfig in eGlobalPolicyRiskConfig.Insurers)
                        {
                            Insurers.Add(GetInsurer(eGlobalInsurerConfig, risk));
                        }
                    }
                    
                }
            }


            //foreach (EBixPolicyRisk risk in EGlobalPolicy.PolicyRisks)
            //{
            //    foreach (EGlobalPolicyRiskConfig eGlobalPolicyRiskConfig in EGlobalPolicy.EGlobalPolicyRiskConfig)
            //    {
            //        foreach (EGlobalInsurerConfig eGlobalInsurerConfig in eGlobalPolicyRiskConfig.Insurers)
            //        {
            //            Insurers.Add(GetInsurer(eGlobalInsurerConfig, risk));
            //        }

            //    }
            //}
            EGlobalPolicy.Insurers = Insurers;

            /*List<EBixInsurer> insurers = new List<EBixInsurer>();
            EGlobalPolicyRiskConfig policyRisk;
            foreach (EBixPolicyRisk risk in PolicyRisks)
            {
                policyRisk = gv_objPolicyRisksConfigs.FirstOrDefault(config => config.ClassOfBusinessID == risk.TCClassOfBusiness);
                if (policyRisk != null)
                {
                    foreach (EGlobalInsurerConfig insurerConfig in policyRisk.InsurerConfigs)
                    {
                        insurers.Add(GetInsurer(insurerConfig, risk));
                    }
                }
            }*/
        }

        /// <summary>
        /// Gets the insurer for a given risk
        /// </summary>
        /// <returns>The insurer.</returns>
        /// <param name="config">The insurer configuration and data for a given risk</param>
        /// <param name="risk">The Risk to insure against</param>
        protected virtual EBixInsurer GetInsurer(EGlobalInsurerConfig config, EBixPolicyRisk risk)
        {
            string riskCode, subCover;
            // Get the risk and subcover codes
            //GetRiskAndSubCoverCodes (code, out riskCode, out subCover);
            // create a new insurer
            EBixInsurer ir = new EBixInsurer();
            // Fill in its feilds
            //ir.PolicyNumber = TCPolicy.ReferenceID;
            ir.AccountInsurerBranch = config.EGlobalInsurerConfig_Branch;
            ir.AccountInsurerCode = config.EGlobalInsurerConfig_Code;
            ir.RiskCode = risk.RiskCode;
            ir.SubCoverString = risk.SubCoverString; 
            ir.InsurerProportion = Convert.ToDecimal(config.EGlobalInsurerConfig_InsurerProportion * 100);            
            ir.InsurerBranch = config.EGlobalInsurerConfig_Branch;
            ir.InsurerCode = config.EGlobalInsurerConfig_Code;
            ir.LeadInsurer = config.EGlobalInsurerConfig_tLeadOrder;
            // TODO - Fill in IAC_T_ fields here once updated prop is released
            /************************/
            ir.CoyPremium = risk.CoyPremium * config.EGlobalInsurerConfig_InsurerProportion;
            ir.GSTPremium = risk.GSTPremium * config.EGlobalInsurerConfig_InsurerProportion;
            ir.BrokerAmountDue_T = risk.BrokerAmountDue * config.EGlobalInsurerConfig_InsurerProportion;
            ir.GSTBrokerage = risk.GSTBrokerage * config.EGlobalInsurerConfig_InsurerProportion;
            ir.BrokerCEQDue_T = risk.BrokerCeqDue * config.EGlobalInsurerConfig_InsurerProportion;
            ir.CEQuake = 0m;//risk.CEQuake * config.InsurerProportion;
            ir.LeviesA = 0m;//risk.LeviesA * config.InsurerProportion;
            ir.LeviesB = risk.LeviesB * config.EGlobalInsurerConfig_InsurerProportion;
            ir.BrokerCEQRate = 0m;
            ir.NetPay = (ir.CoyPremium + ir.GSTPremium) -
                           (ir.BrokerAmountDue_T + ir.GSTBrokerage);
            /************************/
            ir.TermsofTradeCode = "80MF";   // Determine
            ir.InsurerPerson = "Policy";    // Determine
            ir.Broker = EbixUser;
            ir.Placement = DateTime.Now; //placeholder
            ir.NonRTax = 0m;
            return ir;
        }

        protected void GetSubAgents()
        {
            List<EBixSubAgent> agents = new List<EBixSubAgent>();
            agents.AddRange(GetDefaultSubAgents());
            //agents.AddRange(GetQuoteSubAgents());
            EGlobalPolicy.SubAgents = agents;            
        }

        List<EBixSubAgent> GetDefaultSubAgents()
        {
            List<EBixSubAgent> subAgents = new List<EBixSubAgent>();
            EGlobalPolicyRiskConfig policyRisk;
            foreach (EBixPolicyRisk risk in EGlobalPolicy.PolicyRisks)
            {
                foreach (EGlobalPolicyRiskConfig eGlobalPolicyRiskConfig in EGlobalPolicy.EGlobalPolicyRiskConfig)
                {
                    if (eGlobalPolicyRiskConfig.RiskCode == risk.RiskCode)
                    {
                        foreach (EGlobalSubAgent eGlobalSubAgent in eGlobalPolicyRiskConfig.SubAgents)
                        {
                            subAgents.Add(eGlobalSubAgent.CalculateSubAgent(EGlobalPolicy.ClientProgramme.InformationSheet, risk));
                        }
                    }

                }              
            }
            return subAgents;
        }

        List<EBixSubAgent> GetQuoteSubAgents()
        {
            List<EBixSubAgent> subAgents = new List<EBixSubAgent>();
            EBixSubAgent subAgent;
            EBixPolicyRisk risk;

            throw new Exception("GetQuoteSubAgents() yet to be implemented");
            /*using (NpgsqlConnection sqlConnection1 = TC_Shared.GetSqlConnection())
            {
                NpgsqlCommand sqlcmd = new NpgsqlCommand(@"SELECT * FROM tblsubagentquote WHERE quoteid = @QuoteID 
                    AND datedeleted IS NULL", sqlConnection1);
                sqlcmd.Parameters.Add("@QuoteID", NpgsqlDbType.Uuid).Value = TCPolicy.ID;
                Guid classOfBusinessID;
                EGlobalPolicyRiskConfig config;
                string subcover;
                try
                {
                    sqlConnection1.Open();
                    using (var dr = sqlcmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            classOfBusinessID = new Guid(TC_Shared.CNullStr(dr["classofbusinessid"]));
                            risk = PolicyRisks.FirstOrDefault(r => r.TCClassOfBusiness == classOfBusinessID);
                            config = gv_objPolicyRisksConfigs.FirstOrDefault(c => c.MergeWithCOBID != Guid.Empty
                            && c.ClassOfBusinessID == classOfBusinessID);
                            subcover = (risk != null) ? risk.SubCover.ToString() : ((config != null) ? config.SubCover : "");
                            if (risk != null)
                            {
                                subAgent = subAgents.FirstOrDefault(s => s.SubCover.ToString() == subcover);
                                if (subAgent == null)
                                {
                                    subAgent = new EBixSubAgent();
                                    subAgent.CalcAmount = 0m;
                                    subAgent.AmountCeded = 0m;
                                    subAgent.CoverNumber = 0;
                                    subAgent.GSTCeded = 0m;
                                    subAgent.GSTFlag = TC_Shared.CNullInt(dr["subgstregistered"], 0);
                                    subAgent.ID = TC_Shared.CNullGuid(dr["subagentquoteid"]);
                                    subAgent.Percent = TC_Shared.CNullDec(dr["subpercentcomm"], 0m);
                                    subAgent.SubAgent = TC_Shared.CNullStr(dr["subcode"]);
                                    subAgent.SubCover = TC_Shared.CNullInt(subcover, 0);
                                    subAgent.VerNo = 0;
                                    subAgents.Add(subAgent);
                                }
                                subAgent.CalcAmount += (risk != null) ? risk.BrokerAmountDue + risk.BrokerCeqDue : 0m;
                                subAgent.AmountCeded += TC_Shared.CNullDec(dr["subamount"], 0m) * gv_decPaymentDirection;
                                subAgent.GSTCeded += TC_Shared.RoundDecimal(TC_Shared.CNullDec(dr["subgstamount"], 0m)) *
                                gv_decPaymentDirection;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Could not set up subagent for quote " + TCPolicy.ID, e.ToString());
                }
                finally
                {
                    sqlConnection1.Close();
                }
            }
            return subAgents;*/
        }

        #endregion            

    }
}

