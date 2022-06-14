using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
//using System.ServiceModel;
//using System.ServiceModel.Channels;
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
//using EServices.AccountProxy;
//using ServiceReference1;
using CoreWCF;
using CoreWCF.Channels;

namespace DealEngine.Services.Impl
{
    public class PolicyCenterService : IPolicyCenterService
    {
        IAppSettingService _appSettingService;
        IClientInformationService _clientInformationService;

        public PolicyCenterService()
        {

        }
        #region Old Code from attempt at implementing consumption of legacy SOAP APIs
        //public async Task<bool> GetAccount(Organisation organisation)
        //{
        //    bool success = false;
        //    AccountServiceClient client = null;

        //    GetAccountRequestTO request = new GetAccountRequestTO();
        //    request.Account = new AccountTO();
        //    request.Account.ProducerCode = "MarshMicro"; // MANDATORY

        //    /* One of: MarshMicroWB1, MarshMicroDUN, MarshMicroAKL, MarshMicroACB, MarshMicro
        //    This corresponds to the Branch in DE that would be in Organisational Unit */

        //    request.Account.ContactTO = await SetUpContact(organisation);
        //    request.Account.ExternalAccountID = organisation.Id.ToString();
        //    request.Account.BusOpsDesc = "Business Operations Description"; // Not allowed to be null - In Varchar table was just "
        //    request.Account.AccountOrgType = (AccountOrgType)Enum.Parse(typeof(AccountOrgType), "TC_company");

        //    EndpointAddress remoteAddressEndpoint = new EndpointAddress("https://testeservices.iag.co.nz:24443/AccountService.svc");

        //    // Binding (base) - https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.channels.binding?view=dotnet-plat-ext-6.0
        //    // Custom Binding - https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.channels.custombinding?view=dotnet-plat-ext-6.0
        //    BasicHttpBinding binding = new BasicHttpBinding();
        //    binding.Name = "InternetFacingServiceBinding";
        //    binding.Security.Mode = BasicHttpSecurityMode.None;
        //    binding.MaxReceivedMessageSize = 2147483647;
        //    binding.SendTimeout = TimeSpan.FromMinutes(5);
        //    binding.ReceiveTimeout = TimeSpan.FromMinutes(5);

        //    string requestID = organisation.Id.ToString();

        //    try
        //    {
        //        #region TODO Log and SystemUserUsername / SystemUserPassword values
        //        //TCInsuranceSystemUser objInsuranceSystemUser = objInsuranceSystemAccount.InsuranceSystem.InsuranceSystemUsers
        //        //    .FirstOrDefault(cw => cw.AccountID == objInsuranceSystemAccount.Proposal.BrokerUser.AccountID);

        //        //if (objInsuranceSystemUser == null)
        //        //{
        //        //    TC_Shared.LogEvent(TC_Shared.EventType.Warning, "Insurance System User is null.");
        //        //}
        //        //else
        //        //{
        //        //    TC_Shared.LogEvent(TC_Shared.EventType.Information, "AccountID: " + objInsuranceSystemUser.AccountID,
        //        //        "Username: " + objInsuranceSystemUser.Username + " | Password: " + objInsuranceSystemUser.Password);
        //        //}

        //        // objInsuranceSystemUser retrieval and potential values
        //        //TCInsuranceSystemUser objInsuranceSystemUser = objInsuranceSystemAccount.InsuranceSystem.InsuranceSystemUsers
        //        //    .FirstOrDefault(cw => cw.AccountID == objInsuranceSystemAccount.Proposal.BrokerUser.AccountID); // We want the Users Account ID mapped to Broker's accountID

        //        //id                                      accountid                               insurancesystemid                       username                password 
        //        //"b13006c5-a2a2-46c9-8bfd-131bc02e1042"  "4d10e0d3-8f96-461f-97e8-a0be6189014e"  "c68d136b-22a8-4df2-abc7-fcb3b1406c8b"  "svc_gw6_marshmicro"    "gw"
        //        //"e717a189-2510-48cc-b0b7-06f52eb5c989"  "c6b11984-3f41-41c2-ba8d-1cfc19a41e12"  "c68d136b-22a8-4df2-abc7-fcb3b1406c8b"  "svc_gw6_marshmicro_a"  "iV4L13vy59"
        //        //"e717a192-2510-48cc-b0b7-06f52eb5c989"  "6705086a-5c41-11e1-9f3c-3c0754251e27"  "c68d136b-22a8-4df2-abc7-fcb3b1406c8b"  "svc_gw6_marshmicro_a"  "iV4L13vy59"
        //        #endregion

        //        CustomBinding customBinding = new CustomBinding(binding);

        //        HttpTransportBindingElement transportElement = new HttpTransportBindingElement();
        //        customBinding.Elements.Find<HttpTransportBindingElement>();
        //        transportElement.KeepAliveEnabled = false; //Set Keep Alive to false

        //        client = new AccountServiceClient(binding, remoteAddressEndpoint);
        //        client.Endpoint.Binding = customBinding;  // Set Endpoint Binding to our new Binding
        //        client.ClientCredentials.UserName.UserName = "LDDMZDV\\St_eServicesUser"; // Actually > LDDMZDV\St_eServicesUser in database
        //        client.ClientCredentials.UserName.Password = "Usersrv!=";

        //        HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
        //        httpRequestProperty.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(client.ClientCredentials.UserName.UserName + ":" + client.ClientCredentials.UserName.Password));

        //        using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
        //        {
        //            // set the message in header
        //            // TCAPIAccount objAPIAccount = TCAPIAccount.GetAPIAccount(ID.ToString());

        //            string APIAccountURL = "https://eservicesdemo.proposalonline.com:4443/soap11";
        //            string APIAccountUsername = "MEISSSA_user";
        //            string APIAccountPassword = "@RXjMr|V=OTah~|Z2eTfM|7Em";
        //            string SystemUserUsername = "svc_gw6_marshmicro";       // was objInsuranceSystemUser.Username
        //            string SystemUserPassword = "gw";                       // was objInsuranceSystemUser.Password

        //            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("RequestID", NS, requestID, false, ACTOR));
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointAddress", NS, APIAccountURL, false, ACTOR));
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointUsername", NS, APIAccountUsername, false, ACTOR));
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointPassword", NS, APIAccountPassword, false, ACTOR));
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(USER_PROP, NS, SystemUserUsername, false, ACTOR)); // username for programme broker
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(PW_PROP, NS, SystemUserPassword, false, ACTOR));   // password for programme broker

        //            if (client.State == CommunicationState.Closed)
        //                client.Open();

        //            client.GetAccount(request);
        //            client.Close();

        //            // Log Console.Log("requestID, "Sent - GetAccount", request.XmlSerializeToString());"

        //            success = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Log
        //        client.Abort();
        //    }

        //    return success;

        //}
        //private async Task<ContactTO> SetUpContact(Organisation organisation)
        //{
        //    ContactTO contact = new ContactTO();
        //    //ClientInformationSheet sheet = await _clientInformationService.GetClientInformationSheetFromOrganisation(organisation);
        //    //Location location = sheet.Locations[0]; // First location added is correct one?

        //    contact.OrganizationName = organisation.Name;
        //    contact.AccountSubType = (ContactType)Enum.Parse(typeof(ContactType), "TC_company");
        //    contact.AddressLine1 = "Street";// MANDATORY
        //    //contact.Suburb = 
        //    contact.City = "Auckland";// MANDATORY 
        //    //contact.PostCode = 
        //    contact.Country = (ServiceReference1.Country)Enum.Parse(typeof(ServiceReference1.Country), "TC_NZ"); // MANDATORY

        //    return contact;
        //}

        //public async Task<bool> GetAccountWCFOld(Organisation organisation)
        //{
        //    bool success = false;

        //    AccountServiceClient client = null;

        //    GetAccountRequestTO request = new GetAccountRequestTO();
        //    request.Account = new AccountTO();
        //    request.Account.ProducerCode = "MarshMicro"; // MANDATORY

        //    /* One of: MarshMicroWB1, MarshMicroDUN, MarshMicroAKL, MarshMicroACB, MarshMicro
        //    This corresponds to the Branch in DE that would be in Organisational Unit */

        //    request.Account.ContactTO = await SetUpContact(organisation);
        //    request.Account.ExternalAccountID = organisation.Id.ToString();
        //    request.Account.BusOpsDesc = "Business Operations Description"; // Not allowed to be null - In Varchar table was just "
        //    request.Account.AccountOrgType = (AccountOrgType)Enum.Parse(typeof(AccountOrgType), "TC_company");

        //    EndpointAddress remoteAddressEndpoint = new EndpointAddress("https://testeservices.iag.co.nz:24443/AccountService.svc");

        //    // Binding (base) - https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.channels.binding?view=dotnet-plat-ext-6.0
        //    // Custom Binding - https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.channels.custombinding?view=dotnet-plat-ext-6.0
        //    BasicHttpBinding binding = new BasicHttpBinding();
        //    binding.Name = "InternetFacingServiceBinding";
        //    binding.Security.Mode = BasicHttpSecurityMode.None;
        //    binding.MaxReceivedMessageSize = 2147483647;
        //    binding.SendTimeout = TimeSpan.FromMinutes(5);
        //    binding.ReceiveTimeout = TimeSpan.FromMinutes(5);

        //    string requestID = organisation.Id.ToString();

        //    try
        //    {
        //        #region TODO Log and SystemUserUsername / SystemUserPassword values
        //        //TCInsuranceSystemUser objInsuranceSystemUser = objInsuranceSystemAccount.InsuranceSystem.InsuranceSystemUsers
        //        //    .FirstOrDefault(cw => cw.AccountID == objInsuranceSystemAccount.Proposal.BrokerUser.AccountID);

        //        //if (objInsuranceSystemUser == null)
        //        //{
        //        //    TC_Shared.LogEvent(TC_Shared.EventType.Warning, "Insurance System User is null.");
        //        //}
        //        //else
        //        //{
        //        //    TC_Shared.LogEvent(TC_Shared.EventType.Information, "AccountID: " + objInsuranceSystemUser.AccountID,
        //        //        "Username: " + objInsuranceSystemUser.Username + " | Password: " + objInsuranceSystemUser.Password);
        //        //}

        //        // objInsuranceSystemUser retrieval and potential values
        //        //TCInsuranceSystemUser objInsuranceSystemUser = objInsuranceSystemAccount.InsuranceSystem.InsuranceSystemUsers
        //        //    .FirstOrDefault(cw => cw.AccountID == objInsuranceSystemAccount.Proposal.BrokerUser.AccountID); // We want the Users Account ID mapped to Broker's accountID

        //        //id                                      accountid                               insurancesystemid                       username                password 
        //        //"b13006c5-a2a2-46c9-8bfd-131bc02e1042"  "4d10e0d3-8f96-461f-97e8-a0be6189014e"  "c68d136b-22a8-4df2-abc7-fcb3b1406c8b"  "svc_gw6_marshmicro"    "gw"
        //        //"e717a189-2510-48cc-b0b7-06f52eb5c989"  "c6b11984-3f41-41c2-ba8d-1cfc19a41e12"  "c68d136b-22a8-4df2-abc7-fcb3b1406c8b"  "svc_gw6_marshmicro_a"  "iV4L13vy59"
        //        //"e717a192-2510-48cc-b0b7-06f52eb5c989"  "6705086a-5c41-11e1-9f3c-3c0754251e27"  "c68d136b-22a8-4df2-abc7-fcb3b1406c8b"  "svc_gw6_marshmicro_a"  "iV4L13vy59"
        //        #endregion

        //        CustomBinding customBinding = new CustomBinding(binding);

        //        HttpTransportBindingElement transportElement = new HttpTransportBindingElement();
        //        customBinding.Elements.Find<HttpTransportBindingElement>();
        //        transportElement.KeepAliveEnabled = false; //Set Keep Alive to false

        //        client = new AccountServiceClient(binding, remoteAddressEndpoint);
        //        client.Endpoint.Binding = customBinding;  // Set Endpoint Binding to our new Binding
        //        client.ClientCredentials.UserName.UserName = "LDDMZDV\\St_eServicesUser"; // Actually > LDDMZDV\St_eServicesUser in database
        //        client.ClientCredentials.UserName.Password = "Usersrv!=";

        //        HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
        //        httpRequestProperty.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(client.ClientCredentials.UserName.UserName + ":" + client.ClientCredentials.UserName.Password));

        //        using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
        //        {
        //            // set the message in header
        //            // TCAPIAccount objAPIAccount = TCAPIAccount.GetAPIAccount(ID.ToString());

        //            string APIAccountURL = "https://eservicesdemo.proposalonline.com:4443/soap11";
        //            string APIAccountUsername = "MEISSSA_user";
        //            string APIAccountPassword = "@RXjMr|V=OTah~|Z2eTfM|7Em";
        //            string SystemUserUsername = "svc_gw6_marshmicro";       // was objInsuranceSystemUser.Username
        //            string SystemUserPassword = "gw";                       // was objInsuranceSystemUser.Password

        //            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("RequestID", NS, requestID, false, ACTOR));
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointAddress", NS, APIAccountURL, false, ACTOR));
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointUsername", NS, APIAccountUsername, false, ACTOR));
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("ResponseEndpointPassword", NS, APIAccountPassword, false, ACTOR));
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(USER_PROP, NS, SystemUserUsername, false, ACTOR)); // username for programme broker
        //            OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(PW_PROP, NS, SystemUserPassword, false, ACTOR));   // password for programme broker

        //            if (client.State == CommunicationState.Closed)
        //                client.Open();

        //            client.GetAccountAsync(request);
        //            //client.GetAccount(request);
        //            client.Close();

        //            // Log Console.Log("requestID, "Sent - GetAccount", request.XmlSerializeToString());"

        //            success = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Log
        //        client.Abort();
        //    }

        //    return success;
        //}
        //public async Task<bool> GetAccountWCF(Organisation organisation)
        //{
        //    EndpointAddress remoteAddressEndpoint = new EndpointAddress("https://testeservices.iag.co.nz:24443/AccountService.svc");
        //    Task result;

        //    BasicHttpBinding binding = new BasicHttpBinding();
        //    binding.Name = "InternetFacingServiceBinding";
        //    binding.Security.Mode = BasicHttpSecurityMode.Transport;
        //    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
        //    binding.OpenTimeout = TimeSpan.FromMinutes(10);
        //    binding.ReceiveTimeout = TimeSpan.FromMinutes(15);
        //    binding.SendTimeout = TimeSpan.FromMinutes(15);
        //    binding.CloseTimeout = TimeSpan.FromMinutes(15);
        //    binding.MaxReceivedMessageSize = int.MaxValue;
        //    binding.MaxBufferSize = int.MaxValue;
        //    //binding.MaxBufferPoolSize = int.MaxValue;
        //    binding.ReaderQuotas.MaxDepth = int.MaxValue;
        //    binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
        //    binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
        //    binding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
        //    binding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;

        //    CustomBinding customBinding = new CustomBinding(binding);
        //    HttpTransportBindingElement transportElement = customBinding.Elements.Find<HttpTransportBindingElement>();
        //    transportElement.KeepAliveEnabled = false;

        //    // System.ServiceModel.Channels.WebSocketTransportSettings.DisablePayloadMasking throws Exception, isn't available in .NET 5
        //    // See https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.channels.websockettransportsettings.disablepayloadmasking?view=dotnet-plat-ext-6.0
        //    // transportElement.WebSocketSettings = null; // Can't be null
        //    transportElement.WebSocketSettings.DisablePayloadMasking = true; // This attribute isn't available in .NET 5

        //    try
        //    {
        //        //AccountServiceClient client = new AccountServiceClient(customBinding, remoteAddressEndpoint); //
        //        //client.Open();
        //        //client.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new FaultException(ex.Message);
        //    }

        //    //AccountServiceClient client = new AccountServiceClient(AccountServiceClient.EndpointConfiguration.BasicHttpBinding_IAccountService, remoteAddressEndpoint);
        //    //AccountServiceClient client = new AccountServiceClient(binding, remoteAddressEndpoint);


        //    //client.ClientCredentials.UserName.UserName = "LDDMZDV\\St_eServicesUser";
        //    //client.ClientCredentials.UserName.Password = "Usersrv!=";

        //    ////https://stackoverflow.com/questions/2763592/the-communication-object-system-servicemodel-channels-servicechannel-cannot-be
        //    //client.ChannelFactory.Credentials.UserName.UserName = 

        //    //GetAccountRequestTO request = new GetAccountRequestTO();
        //    //request.Account = new AccountTO();
        //    //request.Account.ProducerCode = "MarshMicro"; // MANDATORY

        //    ///* One of: MarshMicroWB1, MarshMicroDUN, MarshMicroAKL, MarshMicroACB, MarshMicro
        //    //This corresponds to the Branch in DE that would be in Organisational Unit */

        //    //request.Account.ContactTO = await SetUpContact(organisation);
        //    //request.Account.ExternalAccountID = organisation.Id.ToString();
        //    //request.Account.BusOpsDesc = "Business Operations Description"; // Not allowed to be null - In Varchar table was just "
        //    //request.Account.AccountOrgType = (AccountOrgType)Enum.Parse(typeof(AccountOrgType), "TC_company");

        //    //if (client.InnerChannel.State != System.ServiceModel.CommunicationState.Faulted)
        //    //{
        //    //    // call service - everything's fine
        //    //    result = client.GetAccountAsync(request);
        //    //}
        //    //else
        //    //{
        //    //    // channel faulted - re-create your client and then try again
        //    //    client.Close();
        //    //    //result = GetAccountWCF(organisation);
        //    //}
        //    //client.Close();

        //    ////Console.WriteLine(result);

        //    return true;
        //}
        #endregion
    }
}
