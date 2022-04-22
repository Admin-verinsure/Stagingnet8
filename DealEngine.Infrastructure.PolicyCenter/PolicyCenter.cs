using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using EServices.AccountProxy;

namespace DealEngine.Infrastructure.PolicyCenter
{
    public class PolicyCenter
    {
        protected readonly string NS = "http://www.guidewire.com/soap";
        protected readonly string ACTOR = "http://schemas.xmlsoap.org/soap/actor/next";
        protected readonly string USER_PROP = "gw_auth_user_prop";
        protected readonly string PW_PROP = "gw_auth_password_prop";
        public PolicyCenter() { }

        public bool GetAccount(Guid policyCenterAccountId) 
        {
            bool success = false;

            GetAccountRequestTO request = new GetAccountRequestTO();
            request.Account = new AccountTO();
            request.Account.ProducerCode = "";
            request.Account.ContactTO = new ContactTO();
            request.Account.ExternalAccountID = "";
            request.Account.BusOpsDesc = "";
            request.Account.AccountOrgType = new AccountOrgType();

            AccountServiceClient client = null;
            string requestID = ""; // "TCInsuranceSystemAccount.Id" 

            //TC_Shared.LogEvent(TC_Shared.EventType.Information, "Request ID: " + requestID, request.XmlSerializeToString());

            try
            {
                #region TODO Log
                //TCInsuranceSystemUser objInsuranceSystemUser = objInsuranceSystemAccount.InsuranceSystem.InsuranceSystemUsers
                //    .FirstOrDefault(cw => cw.AccountID == objInsuranceSystemAccount.Proposal.BrokerUser.AccountID);

                //if (objInsuranceSystemUser == null)
                //{
                //    TC_Shared.LogEvent(TC_Shared.EventType.Warning, "Insurance System User is null.");
                //}
                //else
                //{
                //    TC_Shared.LogEvent(TC_Shared.EventType.Information, "AccountID: " + objInsuranceSystemUser.AccountID,
                //        "Username: " + objInsuranceSystemUser.Username + " | Password: " + objInsuranceSystemUser.Password);
                //}
                #endregion

                client = new AccountServiceClient();
                client.ClientCredentials.UserName.UserName = "";//objInsuranceSystemAccount.InsuranceSystem.Username;
                client.ClientCredentials.UserName.Password = "";//objInsuranceSystemAccount.InsuranceSystem.Password;

                //Set Keep Alive to false
                CustomBinding objCustomBinding = new CustomBinding(client.Endpoint.Binding);
                HttpTransportBindingElement objTransportElement = new HttpTransportBindingElement();

                objCustomBinding.Elements.Find<HttpTransportBindingElement>();
                objTransportElement.KeepAliveEnabled = false;
                client.Endpoint.Binding = objCustomBinding;

                HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(client.ClientCredentials.UserName.UserName + ":" + client.ClientCredentials.UserName.Password));

                using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
                {
                    // set the message in header
                    // TCAPIAccount objAPIAccount = TCAPIAccount.GetAPIAccount(ID.ToString());
                    
                    string APIAccountURL = "";          // was objAPIAccount.URL
                    string APIAccountUsername = "";     // was objAPIAccount.Username
                    string APIAccountPassword = "";     // was objAPIAccount.Password
                    string SystemUserUsername = "";     // was objInsuranceSystemUser.Username
                    string SystemUserPassword = "";     // was objInsuranceSystemUser.Password

                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("RequestID", NS, requestID, false, ACTOR));
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointAddress", NS, APIAccountURL, false, ACTOR));
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointUsername", NS, APIAccountUsername, false, ACTOR));
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointPassword", NS, APIAccountPassword, false, ACTOR));
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(USER_PROP, NS, SystemUserUsername, false, ACTOR));
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(PW_PROP, NS, SystemUserPassword, false, ACTOR));

                    if (client.State == CommunicationState.Closed)
                        client.Open();

                    client.GetAccount(request);
                    client.Close();

                    // Log Console.Log("requestID, "Sent - GetAccount", request.XmlSerializeToString());"

                    success = true;
                }
            }
            catch (Exception ex)
            {
                //Log
                client.Abort();
            }
            
            #region Hide
            //return success;

            // get the PO object, we can't do that as we are on DE

            // looks like PO implements SOAP API differently to how James did for EGlobal, the question is can I get a GetAccount request out making an XML string and HTTP client

            // Create the XML request with objects or hardcode the string


            /* 
            <? xml version="1.0" encoding="utf-16"?>
            <GetAccountRequestTO xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
              <Account>
                <ExternalAccountID>c93e0dfc-69aa-4d6e-ade9-8b0651f36a14</ExternalAccountID>
                <ContactTO>
                  <AccountSubType>TC_company</AccountSubType>
                  <OrganizationName>0TCMEISTestCompany01</OrganizationName>
                  <City>Auckland</City>
                  <AddressLine1>1 Queen Street</AddressLine1>
                  <Suburb />
                  <Country>TC_NZ</Country>
                  <AddressType xsi:nil="true" />
                  <PostCode>1111</PostCode>
                  <PrimaryPhoneChoice xsi:nil="true" />
                  <State xsi:nil="true" />
                </ContactTO>
                <ProducerCode>MarshMicroWB1</ProducerCode>
                <AccountOrgType>TC_company</AccountOrgType>
                <BusOpsDesc />
              </Account>
            </GetAccountRequestTO>
            */


            //string xml = "<?xml version=\"1.0\" encoding=\"utf - 16\"?>" +
            //                    "<GetAccountRequestTO xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            //                        "<Account>" +
            //                            "<ExternalAccountID>c93e0dfc-69aa-4d6e-ade9-8b0651f36a14</ExternalAccountID>" +
            //                            "<ContactTO>" +
            //                                "<AccountSubType>TC_company</AccountSubType>" +
            //                                "<OrganizationName>0TCMEISTestCompany01</OrganizationName>" +
            //                                "<City>Auckland</City>" +
            //                                "<AddressLine1>1 Queen Street</AddressLine1>" +
            //                                "<Suburb />" +
            //                                "<Country>TC_NZ</Country>" +
            //                                "<AddressType xsi:nil=\"true\" />" +
            //                                "<PostCode>1111</PostCode>" +
            //                                "<PrimaryPhoneChoice xsi:nil=\"true\" />" +
            //                                "<State xsi:nil=\"true\" />" +
            //                            "</ContactTO>" +
            //                            "<ProducerCode>MarshMicroWB1</ProducerCode>" +
            //                            "<AccountOrgType>TC_company</AccountOrgType>" +
            //                            "<BusOpsDesc />" +
            //                        "</Account>" +
            //                    "</GetAccountRequestTO>";

            // send the request

            // can't call service from here - test in admin console
            //var byteResponse = await _httpClientService.CreateEGlobalInvoice(xmlPayload); 


            //return false;

            #endregion

            return success;
        
        }
    }
}
