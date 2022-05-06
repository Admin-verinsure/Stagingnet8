using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using EServices.AccountProxy;

namespace DealEngine.Infrastructure.PolicyCenter
{
    public class PolicyCenter
    {
        IClientInformationService _clientInformationService;

        protected readonly string NS = "http://www.guidewire.com/soap";
        protected readonly string ACTOR = "http://schemas.xmlsoap.org/soap/actor/next";
        protected readonly string USER_PROP = "gw_auth_user_prop";
        protected readonly string PW_PROP = "gw_auth_password_prop";
        public PolicyCenter(IClientInformationService clientInformationService) {
            _clientInformationService = clientInformationService;
        }

        public async Task<bool> GetAccount(Organisation organisation) 
        {
            bool success = false;

            GetAccountRequestTO request = new GetAccountRequestTO();
            request.Account = new AccountTO();
            request.Account.ProducerCode = "MarshMicro"; // objInsuranceSystemAccount.ProducerCode; MANDATORY
            // One of: MarshMicroWB1, MarshMicroDUN, MarshMicroAKL, MarshMicroACB, MarshMicro
            request.Account.ContactTO = await SetUpContact(organisation);
            request.Account.ExternalAccountID = "00000000-0000-0000-0000-000000000000"; 
            // objInsuranceSystemAccount.SchemeMemberId.ToString(); "public.scheme_tblmember" I think its the mapping of the members to their programme leave NULL?
            request.Account.BusOpsDesc = "Business Operations Description"; // objInsuranceSystemAccount.InsuranceSystem.GetDefaultData("BusOpsDesc", "AccountTO"); //Not allowed to be null
            request.Account.AccountOrgType = (AccountOrgType)Enum.Parse(typeof(AccountOrgType), "Account Organisation Type");

            AccountServiceClient client = null;
            string requestID = organisation.Id.ToString(); // "TCInsuranceSystemAccount.Id" 

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

                // objInsuranceSystemUser retrieval and potential values
                //TCInsuranceSystemUser objInsuranceSystemUser = objInsuranceSystemAccount.InsuranceSystem.InsuranceSystemUsers
                //    .FirstOrDefault(cw => cw.AccountID == objInsuranceSystemAccount.Proposal.BrokerUser.AccountID); // We want the Users Account ID mapped to Broker's accountID

                //id                                      accountid                               insurancesystemid                       username                password 
                //"b13006c5-a2a2-46c9-8bfd-131bc02e1042"  "4d10e0d3-8f96-461f-97e8-a0be6189014e"  "c68d136b-22a8-4df2-abc7-fcb3b1406c8b"  "svc_gw6_marshmicro"    "gw"
                //"e717a189-2510-48cc-b0b7-06f52eb5c989"  "c6b11984-3f41-41c2-ba8d-1cfc19a41e12"  "c68d136b-22a8-4df2-abc7-fcb3b1406c8b"  "svc_gw6_marshmicro_a"  "iV4L13vy59"
                //"e717a192-2510-48cc-b0b7-06f52eb5c989"  "6705086a-5c41-11e1-9f3c-3c0754251e27"  "c68d136b-22a8-4df2-abc7-fcb3b1406c8b"  "svc_gw6_marshmicro_a"  "iV4L13vy59"

                client = new AccountServiceClient();
                client.ClientCredentials.UserName.UserName = "LDDMZDV\\St_eServicesUser"; // Actually > LDDMZDV\St_eServicesUser in database objInsuranceSystemAccount.InsuranceSystem.Username;
                client.ClientCredentials.UserName.Password = "Usersrv!="; // objInsuranceSystemAccount.InsuranceSystem.Password;

                // Binding (base) - https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.channels.binding?view=dotnet-plat-ext-6.0
                // Custom Binding - https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.channels.custombinding?view=dotnet-plat-ext-6.0
                CustomBinding customBinding = new CustomBinding(client.Endpoint.Binding);

                // HttpTransportBindingElement - https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.channels.httpstransportbindingelement?view=dotnet-plat-ext-6.0
                HttpTransportBindingElement transportElement = new HttpTransportBindingElement();

                customBinding.Elements.Find<HttpTransportBindingElement>();
                transportElement.KeepAliveEnabled = false; //Set Keep Alive to false

                client.Endpoint.Binding = customBinding;  // Set Endpoint Binding to our new Binding

                HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(client.ClientCredentials.UserName.UserName + ":" + client.ClientCredentials.UserName.Password));

                using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
                {
                    // set the message in header
                    // TCAPIAccount objAPIAccount = TCAPIAccount.GetAPIAccount(ID.ToString());
                    
                    string APIAccountURL = "https://eservicesdemo.proposalonline.com:4443/soap11";
                    string APIAccountUsername = "MEISSSA_user";
                    string APIAccountPassword = "@RXjMr|V=OTah~|Z2eTfM|7Em";
                    string SystemUserUsername = "svc_gw6_marshmicro";     // was objInsuranceSystemUser.Username
                    string SystemUserPassword = "gw";     // was objInsuranceSystemUser.Password

                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("RequestID", NS, requestID, false, ACTOR));
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointAddress", NS, APIAccountURL, false, ACTOR));
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointUsername", NS, APIAccountUsername, false, ACTOR));
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointPassword", NS, APIAccountPassword, false, ACTOR));
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(USER_PROP, NS, SystemUserUsername, false, ACTOR)); // username for programme broker
                    OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(PW_PROP, NS, SystemUserPassword, false, ACTOR));   // password for programme broker

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
        private async Task<ContactTO> SetUpContact(Organisation organisation)
        {
            ContactTO contact = new ContactTO();
            ClientInformationSheet sheet = await _clientInformationService.GetClientInformationSheetFromOrganisation(organisation);
            Location location = sheet.Locations[0]; // First location added is correct one?

            contact.OrganizationName = organisation.Name; // objInsuranceSystemAccount.OrganisationName; MANDATORY

            if (sheet != null)
            {
                contact.AddressLine1 = location.Street; // MANDATORY
                contact.Suburb = location.Suburb;
                contact.City = location.City; // MANDATORY 
                contact.PostCode = location.Postcode;
                contact.Country = (EServices.AccountProxy.Country)Enum.Parse(typeof(EServices.AccountProxy.Country), location.Country); // MANDATORY
            }

            #region PO Account-Sub Types MANDATORY
            //Insurer
            //Broker
            //Scheme Manager
            //RE Plan
            //Reinsurer
            //TCCustomer
            //TechCertain
            //ClaimsManager
            //ReinsuranceBroker
            #endregion

            if (organisation.IsInsurer)
            {
                contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "Insurer");
            }
            else if (organisation.IsBroker)
            {
                contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "Broker");
            }
            else if (organisation.IsProgrammeManager)
            {
                contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "Scheme Manager");
            }

            #region Non-Existing Account Types
            //else if (organisation.) 
            //{
            //    contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "RE PLan");
            //}
            //else if (organisation.) 
            //{
            //    contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "Reinsurer");
            //}
            //else if (organisation.) 
            //{
            //    contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "TCCustomer");
            //}
            #endregion

            else if (organisation.IsTC)
            {
                contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "TechCertain");
            }

            #region Non-Existing Account Types 2
            //else if (organisation.)
            //{
            //    contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "ClaimsManager");
            //}
            //else if (organisation.)
            //{
            //    contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "ReinsuranceBroker");
            //}
            #endregion

            else
            {
                contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "");
            }

            return contact;
        }
    }
}
