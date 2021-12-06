using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;

namespace DealEngine.Infrastructure.Payment.EGlobalAPI
{
    public class EGlobalAPI
    {

        #region API fields
      
        /// <summary>
        /// Processes the Async result.
        /// </summary>
        /// <param name="result">The Async result.</param>
        public string ProcessAsyncResult(string res, ClientProgramme programme, User CurrentUser, IUnitOfWork _unitOfWork, EGlobalSubmission eglobalsubmission)
        {
            try
            {
                // adjust the response
                Envelope processingxml = GetPreResponseClass(res);
                // decode string to Base64
                byte[] byteStream = Convert.FromBase64String(processingxml.Body.CreateInvoiceResponse.Text);
                ASyncInvoice = Encoding.UTF8.GetString(byteStream, 0, byteStream.Length);

                // process response
                EGlobalXmlResponse xo = GetResponseClass(ASyncInvoice);
                ProcessResponse(xo, programme, CurrentUser, _unitOfWork, ASyncInvoice, eglobalsubmission);

                // indicate we have received a response
                ASyncInvoiceRecieved = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ASyncInvoice;
        }

        public bool ASyncInvoiceRecieved
        {
            get;
            set;
        }

        public string ASyncInvoice
        {
            get;
            set;
        }

        /// <summary>
        /// Deserializes the returned XML into a C# object
        /// </summary>
        /// <returns>The equivalent C# class</returns>
        /// <param name="xml">The response xml</param>
        public EGlobalXmlResponse GetResponseClass(string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(EGlobalXmlResponse));
            StringReader sr = new StringReader(xml);
            EGlobalXmlResponse xo = (EGlobalXmlResponse)xs.Deserialize(sr);
            return xo;
        }

        public Envelope GetPreResponseClass(string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Envelope));
            StringReader sr = new StringReader(xml);
            Envelope xo = (Envelope)xs.Deserialize(sr);
            return xo;
        }

        #endregion

        private void ProcessResponse(EGlobalXmlResponse xo, ClientProgramme programme, User CurrentUser, IUnitOfWork _unitOfWork, string xmltext, EGlobalSubmission eglobalsubmission)
        {

            try
            {
                // process the response
                string key = "";
                if (xo.Update != null)
                    key = xo.Update.UpdExtSysKey;
                else if (xo.Error != null)
                {
                    key = xo.Error.ExtSysKey;
                    // check for API failure/crash errors
                    //TC_Shared.LogEvent(TC_Shared.EventType.API_Bug, "ELink API billing failure", xo.XmlSerializeToString());
                }
                string[] sysKeys = key.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                key = sysKeys[1].Split(new char[] { '-' }, StringSplitOptions.None)[1];

                string masterAgreementReferenceID = key;

                // save the response
                Guid responseID;
                SaveInvoiceData(xo, masterAgreementReferenceID, programme, CurrentUser, _unitOfWork, xmltext, eglobalsubmission, out responseID);
                //// determine the original transaction
                //Guid invoiceID = Base_EGlobalPolicy.GetInvoiceID(sysKeys[1], TC_Shared.CNullInt(sysKeys[2], 0));
                //Base_EGlobalPolicy.SetResponseID(responseID, invoiceID);

                // store the invoice number if it is an update
                if (xo.Update != null)
                {
                    //product = new Product(Base_EGlobalPolicy.GetQuoteID(sysKeys[1], TC_Shared.CNullInt(sysKeys[2], 0)));

                    //product.SetExternalInvoiceNumber(xo.Update.InvoiceNo.ToString());

                    ////Set quote status to bound and allow it gets updated laster
                    //product.SetWorkflowID(10);

                    //// also check to see if it is a cancel
                    //if (xo.Update.UpdTranCode == "C" && product.Product.UsesInsuranceSystemAPI)
                    //{
                    //    TCInsuranceSystemPolicyCancel objInsuranceSystemPolicyCancel = TCInsuranceSystemPolicyCancel.GetCancelPolicy(product.Id);
                    //    TCInsuranceSystem objInsuranceSystem = TCInsuranceSystem.GetInsuranceSystem(product.SchemeProject.Product.InsuranceSystemID);
                    //    objInsuranceSystem.SubmitConfirmPolicyCancelation(objInsuranceSystemPolicyCancel);

                    //    quote.SchemeProject.LogSchemeProjectHistory(quote.BrokerUser.ID, "Comfirm a Policy Cancellation.", quote.Proposal.CompanyName);
                    //    quote.SetWorkflowID(25);
                    //}

                    //// if guidewire, send bind request
                    //if (product.Product.UsesInsuranceSystemAPI && (xo.Update.UpdTranCode == "N" || xo.Update.UpdTranCode == "E" || xo.Update.UpdTranCode == "R"))
                    //{
                    //    try
                    //    {
                    //        if (!product.ExternalPolicy)
                    //        {
                    //            product.SendExternalBindQuoteRequest();

                    //            product.SetWorkflowID((int)TCQuote.TCWorkflowStatus.Invoiced_Pending_Bind);

                    //            new Thread(new ThreadStart(delegate ()
                    //            {
                    //                Thread.Sleep(new TimeSpan(0, 3, 0));
                    //                TCQuote objTQuote = new TCQuote(quote.ID);

                    //                if (!objTQuote.ExternalPolicy)
                    //                {
                    //                    //Send Emails
                    //                    objTQuote.SendExternalBindFailedToAgency();
                    //                    objTQuote.SendExternalBindFailedToBroker();
                    //                }
                    //            })).Start();
                    //        }

                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        //TC_Shared.LogEvent(TC_Shared.EventType.API_Bug, "Something went wrong trying to bind a policy, try binding it manually.");
                    //    }
                    //}

                    //// determine the paymemnt type
                    //string type = quote.Proposal.GetPropData("PaymentMethod");
                    //// if HPF
                    //if (type == "2")
                    //{
                    //    HunterPremiumFunding.HunterPremium hpf = new HunterPremiumFunding.HunterPremium(quote.ProductID);
                    //    hpf.LoadHunterRecord(quote.ID);
                    //    hpf.SetInvoiceNumber(quote.ExternalInvoiceNumber);
                    //    hpf.SaveHunterRecord();
                    //}

                    //// produce policy documents
                    //if (!quote.Product.UsesInsuranceSystemAPI)
                    //{
                    //    quote.DeleteFilesNotInBoundCOBs(quote.Proposal.ContactUser);
                    //    //Render policy documents
                    //    try
                    //    {
                    //        quote.RenderPolicyDocuments(quote.Proposal.ContactUser);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        TC_Shared.LogEvent(TC_Shared.EventType.Bug, String.Format("Error while rendering documents for quote reference {0}", quote.ReferenceID.ToString()), ex.GetBaseException().ToString());
                    //    }

                    //    //Remove policy documents for custom products
                    //    quote.RemoveDocuments();

                    //    //Send the Policy documents
                    //    if (quote.SchemeProject.MailTemplate_PolicyDocumentsCoveringText.IsValid)
                    //    {
                    //        if (!quote.SchemeProject.SendPolicyDocuments(quote.Proposal.ContactUser, quote))
                    //        {
                    //            TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Unable to send documents for quote: " + quote.ReferenceID.ToString());
                    //        }
                    //    }
                    //    else
                    //    {
                    //        TC_Shared.LogEvent(TC_Shared.EventType.Warning, "Policy Documents Email not configured - automatic issue on acceptance failed", quote.ID.ToString());
                    //    }

                    //    subProducts = quote.GetSecondaryQuotes();

                    //    foreach (TCQuote q in subProducts)
                    //    {
                    //        if (q != null)
                    //        {
                    //            q.SetExternalInvoiceNumber(xo.Update.InvoiceNo.ToString());

                    //            q.RenderPolicyDocuments(q.Proposal.ContactUser);

                    //            if (q.SchemeProject.MailTemplate_PolicyDocumentsCoveringText.IsValid)
                    //            {
                    //                if (!q.SchemeProject.SendPolicyDocuments(q.Proposal.ContactUser, q))
                    //                {
                    //                    TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Unable to send documents for quote: " + q.ReferenceID.ToString());
                    //                }
                    //            }
                    //            else
                    //            {
                    //                TC_Shared.LogEvent(TC_Shared.EventType.Warning, "Policy Documents Email not configured - automatic issue on acceptance failed", q.ID.ToString());
                    //            }
                    //        }
                    //    }
                    //}
                }
                //else
                //{
                //    // Eglobal Invoice Failed 
                //    // Unbund quote
                //    // Send notification to client

                //    if (quote.Policy && string.IsNullOrEmpty(quote.ExternalInvoiceNumber))
                //    {
                //        quote.UnBindQuote(quote.BrokerUser);


                //    }

                //}
            }
            catch (Exception ex)
            {
                //TC_Shared.LogEvent(TC_Shared.EventType.API_Bug, "Error in EGlobal API response", ex.GetBaseException().ToString());
            }
        }

        //        static EGlobalXmlResponse ProcessXMLResponseObject(NpgsqlDataReader dr, EGlobalXmlResponse xo)
        //        {
        //            bool boolError = TC_Shared.CNullStr(dr["responsetype"]) == "error";
        //            if (boolError && (xo == null || xo.Update == null))
        //            {
        //                xo.Text = TC_Shared.CNullStr(dr["message"]);
        //                EGlobalResponseError objError = new EGlobalResponseError();
        //                objError.ExtSysKey = TC_Shared.CNullStr(dr["extsyskey"]);
        //                objError.ExtSysRef = TC_Shared.CNullStr(dr["extsysref"]);
        //                objError.Key = TC_Shared.CNullStr(dr["key"]);
        //                objError.TranCode = TC_Shared.CNullStr(dr["trancode"]);
        //                objError.Code = TC_Shared.CNullStr(dr["code"]);
        //                objError.Desc = TC_Shared.CNullStr(dr["description"]);
        //                objError.ExtSysInput = TC_Shared.CNullStr(dr["extsysinput"]);
        //                xo.Error = objError;
        //            }
        //            else if (!boolError)
        //            {
        //                EGlobalResponseUpdate objUpdate = new EGlobalResponseUpdate();
        //                objUpdate.UpdExtSysKey = TC_Shared.CNullStr(dr["extsyskey"]);
        //                objUpdate.UpdExtSysRef = TC_Shared.CNullStr(dr["extsysref"]);
        //                objUpdate.UpdKey = TC_Shared.CNullStr(dr["key"]);
        //                objUpdate.UpdTranCode = TC_Shared.CNullStr(dr["trancode"]);
        //                objUpdate.UpdCode = TC_Shared.CNullStr(dr["code"]);
        //                objUpdate.UpdDesc = TC_Shared.CNullStr(dr["description"]);
        //                objUpdate.UpdExtSysInput = TC_Shared.CNullStr(dr["extsysinput"]);
        //                objUpdate.Company = TC_Shared.CNullStr(dr["company"]);
        //                objUpdate.Branch = TC_Shared.CNullStr(dr["branch"]);
        //                objUpdate.ClientNo = TC_Shared.CNullInt(dr["clientnumber"], 0);
        //                objUpdate.CoverNo = TC_Shared.CNullInt(dr["covernumber"], 0);
        //                objUpdate.VersionNo = TC_Shared.CNullInt(dr["versionnumber"], 0);
        //                objUpdate.Tranident = TC_Shared.CNullStr(dr["tranident"]);
        //                objUpdate.InvoiceNo = TC_Shared.CNullInt(dr["invoicenumber"], 0);
        //                xo.Update = objUpdate;
        //            }
        //            return xo;
        //        }

        //        #region Save/Load

        //        private EGlobalXmlResponse GetInvoiceDataByAttribute(Guid ID, string attribute, bool latest)
        //        {
        //            EGlobalXmlResponse xo = new EGlobalXmlResponse();

        //            string strCmd = "";
        //            strCmd = @"SELECT * FROM tbleglobalinvoiceresponse WHERE " + attribute + " = @Id";
        //            if (latest)
        //                strCmd += " order by datecreated desc";

        //            using (NpgsqlConnection sqlConnection1 = TC_Shared.GetSqlConnection())
        //            {
        //                NpgsqlCommand sqlcmd = new NpgsqlCommand(strCmd, sqlConnection1);
        //                sqlcmd.Parameters.Add("@Id", NpgsqlTypes.NpgsqlDbType.Uuid).Value = ID;

        //                try
        //                {
        //                    sqlConnection1.Open();

        //                    using (var dr = sqlcmd.ExecuteReader())
        //                    {
        //                        if (latest && dr.Read())
        //                        {
        //                            xo = ProcessXMLResponseObject(dr, xo);
        //                        }
        //                        else
        //                        {
        //                            while (dr.Read())
        //                                xo = ProcessXMLResponseObject(dr, xo);
        //                        }
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    TC_Shared.LogEvent(TC_Shared.EventType.Bug, "Failed to load EGlobal response record with " + attribute +
        //                    ": " + ID.ToString(), e.ToString());
        //                }
        //                finally
        //                {
        //                    sqlConnection1.Close();
        //                }
        //            }
        //            return xo;
        //        }

        //        public EGlobalXmlResponse GetResponseInvoiceData(Guid responseID)
        //        {
        //            return GetInvoiceDataByAttribute(responseID, "responseID", false);
        //        }

        //        public EGlobalXmlResponse LoadInvoiceData(Guid quoteID)
        //        {
        //            return GetInvoiceDataByAttribute(quoteID, "quoteID", false);
        //        }

        //        public EGlobalXmlResponse GetLatestInvoiceData(Guid quoteID)
        //        {
        //            return GetInvoiceDataByAttribute(quoteID, "quoteID", true);
        //        }

        public bool SaveInvoiceData(EGlobalXmlResponse xo, string masterAgreementReferenceID, ClientProgramme programme, User CurrentUser, IUnitOfWork _unitOfWork, string xmltext, EGlobalSubmission eglobalsubmission, out Guid responseID)
        {
            bool success = false;

            responseID = Guid.Empty;

            if (xo == null)
                return success;

            bool objError = xo.Error != null;

            if (objError)
            {
                // set error object paramaters
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    EGlobalResponse eGlobalResponse = new EGlobalResponse(CurrentUser);
                    eGlobalResponse.ResponseType = "error";
                    eGlobalResponse.ExtSysKey = xo.Error.ExtSysKey;
                    eGlobalResponse.ExtSysRef = xo.Error.ExtSysRef;
                    eGlobalResponse.Key = xo.Error.Key;
                    eGlobalResponse.TranCode = xo.Error.TranCode;
                    eGlobalResponse.Code = xo.Error.Code;
                    eGlobalResponse.Description = xo.Error.Desc;
                    eGlobalResponse.ExtSysInput = xo.Error.ExtSysInput;
                    eGlobalResponse.ResponseText = xo.Text;
                    eGlobalResponse.ResponseXML = xmltext;
                    eGlobalResponse.MasterAgreementReferenceID = masterAgreementReferenceID;
                    if (eglobalsubmission != null)
                    {
                        eGlobalResponse.EGlobalSubmission = eglobalsubmission;
                    }
                    programme.ClientAgreementEGlobalResponses.Add(eGlobalResponse);
                    eglobalsubmission.EGlobalResponse = eGlobalResponse;

                    uow.Commit();

                    success = true;
                }
            } else {
                // set update object parameters
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    EGlobalResponse eGlobalResponse = new EGlobalResponse(CurrentUser);
                    eGlobalResponse.ResponseType = "update";
                    eGlobalResponse.ExtSysKey = xo.Update.UpdExtSysKey;
                    eGlobalResponse.ExtSysRef = xo.Update.UpdExtSysRef;
                    eGlobalResponse.Key = xo.Update.UpdKey;
                    eGlobalResponse.TranCode = xo.Update.UpdTranCode;
                    eGlobalResponse.Code = xo.Update.UpdCode;
                    eGlobalResponse.Description = xo.Update.UpdDesc;
                    eGlobalResponse.ExtSysInput = xo.Update.UpdExtSysInput;
                    eGlobalResponse.ResponseText = xo.Text;
                    eGlobalResponse.ResponseXML = xmltext;
                    eGlobalResponse.MasterAgreementReferenceID = masterAgreementReferenceID;
                    eGlobalResponse.Company = xo.Update.Company;
                    eGlobalResponse.Branch = xo.Update.Branch;
                    eGlobalResponse.ClientNumber = xo.Update.ClientNo;
                    eGlobalResponse.CoverNumber = xo.Update.CoverNo;
                    eGlobalResponse.VersionNumber = xo.Update.VersionNo;
                    eGlobalResponse.Tranident = xo.Update.Tranident;
                    eGlobalResponse.InvoiceNumber = xo.Update.InvoiceNo;
                    if (eglobalsubmission != null)
                    {
                        eGlobalResponse.EGlobalSubmission = eglobalsubmission;
                    }
                    programme.ClientAgreementEGlobalResponses.Add(eGlobalResponse);
                    eglobalsubmission.EGlobalResponse = eGlobalResponse;

                    uow.Commit();

                    success = true;

                }
            }

            return success;
        }

        //        #endregion

        //        #region Testing

        //        public string GetSampleXml()
        //        {
        //            return GetSampleXml("test.xml");
        //        }

        //        public string GetSampleXml(string filename)
        //        {
        //            string file;
        //            string loc = "./Components/TCShared/EGlobal/EBIX/Testing/" + filename;
        //            //FileStream fs = new FileStream(loc, FileMode.Open);
        //            //BinaryReader reader = new BinaryReader(fs);
        //            //StreamReader reader = new StreamReader(loc, false);
        //            //file = reader.ReadString();
        //            file = File.ReadAllText(loc);
        //            return file;
        //        }

        //        //		public string UpdateInvoiceXML (string xml)
        //        //		{
        //        //			return service.updateInvoice (xml);
        //        //		}

        //        #endregion

        #region XML Serialization Objects

        [XmlRoot("XmlOutput")]
        public class EGlobalXmlResponse
        {
            [XmlAttribute("Version")]
            public string Version
            { get; set; }

            [XmlElement("Error")]
            public EGlobalResponseError Error
            { get; set; }

            [XmlElement("Update")]
            public EGlobalResponseUpdate Update
            { get; set; }

            [XmlText]
            public string Text
            { get; set; }

        }

        [XmlRoot(ElementName = "createInvoiceResponse", Namespace = "http://www.example.org/invoice-service/")]
        public class CreateInvoiceResponse
        {
            [XmlAttribute(AttributeName = "ns3", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Ns3 { get; set; }
            [XmlAttribute(AttributeName = "ns2", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Ns2 { get; set; }
            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public class Body
        {
            [XmlElement(ElementName = "createInvoiceResponse", Namespace = "http://www.example.org/invoice-service/")]
            public CreateInvoiceResponse CreateInvoiceResponse { get; set; }
        }

        [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public class Envelope
        {
            [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public Body Body { get; set; }
            [XmlAttribute(AttributeName = "S", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string S { get; set; }
        }

        [XmlRoot("Error")]
        public class EGlobalResponseError
        {
            [XmlElement("ExtSysKey")]
            public string ExtSysKey
            { get; set; }

            [XmlElement("ExtSysRef")]
            public string ExtSysRef
            { get; set; }

            [XmlElement("Key")]
            public string Key
            { get; set; }

            [XmlElement("TranCode")]
            public string TranCode
            { get; set; }

            [XmlElement("Code")]
            public string Code
            { get; set; }

            [XmlElement("Desc")]
            public string Desc
            { get; set; }

            [XmlElement("ExtSysInput")]
            public string ExtSysInput
            { get; set; }
        }

        [XmlRoot("Update")]
        public class EGlobalResponseUpdate
        {
            [XmlElement("UpdExtSysKey")]
            public string UpdExtSysKey
            { get; set; }

            [XmlElement("UpdExtSysRef")]
            public string UpdExtSysRef
            { get; set; }

            [XmlElement("UpdKey")]
            public string UpdKey
            { get; set; }

            [XmlElement("UpdTranCode")]
            public string UpdTranCode
            { get; set; }

            [XmlElement("UpdCode")]
            public string UpdCode
            { get; set; }

            [XmlElement("UpdDesc")]
            public string UpdDesc
            { get; set; }

            [XmlElement("UpdExtSysInput")]
            public string UpdExtSysInput
            { get; set; }

            [XmlElement("_Company")]
            public string Company
            { get; set; }

            [XmlElement("_Branch")]
            public string Branch
            { get; set; }

            [XmlElement("_ClientNo")]
            public string ClientNo
            { get; set; }

            [XmlElement("_CoverNo")]
            public int CoverNo
            { get; set; }

            [XmlElement("_VersionNo")]
            public int VersionNo
            { get; set; }

            [XmlElement("_Tranident")]
            public string Tranident
            { get; set; }

            [XmlElement("_InvoiceNo")]
            public int InvoiceNo
            { get; set; }
        }

        #endregion
    }
}

