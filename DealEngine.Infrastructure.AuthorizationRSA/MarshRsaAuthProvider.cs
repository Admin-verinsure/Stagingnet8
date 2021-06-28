
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using DealEngine.Services.Interfaces;

namespace DealEngine.Infrastructure.AuthorizationRSA
{
    public enum RsaStatus
	{
		Allow,
		Deny,
		RequiresOtp
	}

	public class MarshRsaAuthProvider
	{
		ILogger _logger;
        IHttpClientService _httpClientService;
        IEmailService _emailService;

        public MarshRsaAuthProvider (
            ILogger logger, 
            IHttpClientService httpClientService,
            IEmailService emailService
            )
		{
			_logger = logger;
            _httpClientService = httpClientService;
            _emailService = emailService;
        }

		public MarshRsaUser GetRsaUser(string email)
		{
			return new MarshRsaUser (email);
		}

		/// <summary>
		/// Calculates the hash of the given user Id. Copied from Marsh supplied MFAUtils.
		/// </summary>
		/// <returns>The hashed identifier.</returns>
		/// <param name="userId">User identifier.</param>
		public string GetHashedId (string userId)
		{
			// Copied from Marsh supplied MFAUtils.
			SHA256 sha256 = new SHA256Managed ();
			var sha256Bytes = Encoding.Default.GetBytes (userId);
			var cryString = sha256.ComputeHash (sha256Bytes);
			var sha256Str = string.Empty;
			for (var i = 0; i < cryString.Length; i++) {
				sha256Str += cryString [i].ToString ("X").PadLeft (2, '0');
			}

			return sha256Str.ToLower ();
		}

		public async Task<MarshRsaUser> Analyze (MarshRsaUser rsaUser)
		{
			/*
			 * Needs to be called at the server level, so needs to be converted into a Membership type object provider.
			 * This should only be controlling access to organisational content, and rejecting unauthorized access based
			 * on what RSA says.
			 */
            XmlDocument xDoc = new XmlDocument();
            UserStatus responseUserStatus = UserStatus.UNVERIFIED;
            ActionCode reponseActionCode = ActionCode.NONE;
            GenericResponse response = null;
            Analyze analyzeRequest = new Analyze();
            analyzeRequest.request = GetAnalyzeRequest(rsaUser);            
            string xml = SerializeRSARequest(analyzeRequest, "Analyze");
            
            var analyzeResponseXmlStr = await _httpClientService.Analyze(xml);

            //used for RSA analyze request and response log 
            await _emailService.RsaLogEmail("marshevents@proposalonline.com", rsaUser.Username, xml, analyzeResponseXmlStr);

            try
            {                
                xDoc.LoadXml(analyzeResponseXmlStr);

                var analyseResponse = await BuildAnalyzeResponse(xDoc);
                responseUserStatus = analyseResponse.identificationData.userStatus;
                reponseActionCode = analyseResponse.riskResult.triggeredRule.actionCode;
                response = analyseResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (responseUserStatus != UserStatus.LOCKOUT && responseUserStatus != UserStatus.DELETE)
			{				
				if (responseUserStatus == UserStatus.UNVERIFIED)
				{
                    // TODO - call updateUser here with analyzeResponse
                    UpdateRsaUserFromResponse(response, rsaUser);
                    UpdateUser updateUserRequest = new UpdateUser();
                    updateUserRequest.request = GetUpdateUserRequest(rsaUser);

                    xml = SerializeRSARequest(updateUserRequest, "UpdateUser");
                    var UpdateUserResponseXmlStr = await _httpClientService.UpdateUser(xml);

                    try
                    {
                        xDoc.LoadXml(UpdateUserResponseXmlStr);
                        var updateUserResponse = await BuildUpdateUserResponse(xDoc);
                        responseUserStatus = updateUserResponse.identificationData.userStatus;
                        reponseActionCode = updateUserResponse.riskResult.triggeredRule.actionCode;
                        response = updateUserResponse;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
				if (reponseActionCode == ActionCode.CHALLENGE)
				{
                    UpdateRsaUserFromResponse(response, rsaUser);
                    Challenge challengeRequest = new Challenge();
                    challengeRequest.request = GetChallengeRequest(rsaUser);
                    xml = SerializeRSARequest(challengeRequest, "Challenge");
                    var challengeResponseXmlStr = await _httpClientService.Challenge(xml);

                    try
                    {
                        xDoc.LoadXml(challengeResponseXmlStr);
                        var challengeResponse = await BuildChallengeResponse(xDoc);                                               
                        response = challengeResponse;
                        
                        rsaUser.Otp = ((OTPChallengeResponse)challengeResponse.credentialChallengeList.acspChallengeResponseData.payload).otp;
                        _emailService.MarshRsaOneTimePassword(rsaUser.Email, rsaUser.Otp);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }                    

                    rsaUser.RsaStatus = RsaStatus.RequiresOtp;

                    return rsaUser;									
				} else if (reponseActionCode == ActionCode.ALLOW)
				{
					// Need to save the deviceTokenCookie from analyzeReponse
					UpdateRsaUserFromResponse(response, rsaUser);
                    rsaUser.RsaStatus = RsaStatus.Allow;

                    return rsaUser;
				}
			}

			if (responseUserStatus == UserStatus.LOCKOUT || responseUserStatus == UserStatus.DELETE)
			{
				_logger.LogInformation("Marsh user failed to login");
				throw new Exception("unable to login user: "+ rsaUser.Username);
			}
			// user not allowed in if we get here.

			return rsaUser;
		}

        private async Task<ChallengeResponse> BuildChallengeResponse(XmlDocument xDoc)
        {
            ChallengeResponse response = new ChallengeResponse();

            CredentialChallengeList credentialChallengeList = new CredentialChallengeList();
            credentialChallengeList.acspChallengeResponseData = new AcspChallengeResponseData();            
            OTPChallengeResponse oTPChallengeResponse = new OTPChallengeResponse();
            credentialChallengeList.acspChallengeResponseData.payload = oTPChallengeResponse;
            var otpResults = xDoc.GetElementsByTagName("credentialChallengeList", "http://ws.csd.rsa.com");
            var responseData = otpResults[0];
            var responseNodes = responseData.ChildNodes;
            var callStatus = responseNodes.Item(0).ChildNodes;
            oTPChallengeResponse.otp = callStatus.Item(2).InnerText;

            IdentificationData identificationData = GetIdentificationDataResponse(xDoc);
            DeviceResult deviceData = GetDeviceResultResponse(xDoc);

            response.credentialChallengeList = credentialChallengeList;
            response.identificationData = identificationData;
            response.deviceResult = deviceData;

            return response;
        }

        private async Task<AuthenticateResponse> BuildAuthenticateResponse(XmlDocument xDoc)
        {
            AuthenticateResponse authenticateResponse = new AuthenticateResponse();
            CredentialAuthResultList credentialAuthResultList = new CredentialAuthResultList();
            credentialAuthResultList.acspAuthenticationResponseData = new AcspAuthenticationResponseData();            
            credentialAuthResultList.acspAuthenticationResponseData.callStatus = new CallStatus();
            

            var authResults = xDoc.GetElementsByTagName("credentialAuthResultList", "http://ws.csd.rsa.com");
            var responseData = authResults[0];
            var responseNodes = responseData.ChildNodes;
            var callStatus = responseNodes.Item(0).ChildNodes;
            var callStatusNodes = callStatus.Item(1);

            credentialAuthResultList.acspAuthenticationResponseData.callStatus.statusCode = callStatusNodes.FirstChild.InnerText;

            IdentificationData identificationData = GetIdentificationDataResponse(xDoc);
            DeviceResult deviceData = GetDeviceResultResponse(xDoc);

            authenticateResponse.identificationData = identificationData;
            authenticateResponse.credentialAuthResultList = credentialAuthResultList;
            authenticateResponse.deviceResult = deviceData;

            return authenticateResponse;
        }

        private async Task<UpdateUserResponse> BuildUpdateUserResponse(XmlDocument xDoc)
        {
            UpdateUserResponse response = new UpdateUserResponse();
            
            RiskResult riskResult = GetRiskResultResponse(xDoc);
            IdentificationData identificationData = GetIdentificationDataResponse(xDoc);
            DeviceResult deviceData = GetDeviceResultResponse(xDoc); 

            response.riskResult = riskResult;
            response.identificationData = identificationData;
            response.deviceResult = deviceData;

            return response;

        }

        private async Task<AnalyzeResponse> BuildAnalyzeResponse(XmlDocument xDoc)
        {
            AnalyzeResponse response = new AnalyzeResponse();
            
            RiskResult riskResult = GetRiskResultResponse(xDoc);
            IdentificationData identificationData = GetIdentificationDataResponse(xDoc);
            DeviceResult deviceData = GetDeviceResultResponse(xDoc);

            response.riskResult = riskResult;
            response.identificationData = identificationData;
            response.deviceResult = deviceData;

            return response;
        }

        public string SerializeRSARequest(GenericRequest request, string resquestTag)
        {
            string REPLACESTRING = @"<[requestTag] xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">";
            string ACTUALSTRING = @"<[requestTag] xmlns=""http://ws.csd.rsa.com"">";

            var serxml = new XmlSerializer(request.GetType());
            var ms = new MemoryStream();
            serxml.Serialize(ms, request);
            string xml = Encoding.UTF8.GetString(ms.ToArray());

            REPLACESTRING = REPLACESTRING.Replace("[requestTag]", resquestTag);
            ACTUALSTRING = ACTUALSTRING.Replace("[requestTag]", resquestTag);

            xml = xml.Replace(REPLACESTRING, ACTUALSTRING);

            var stringPayLoad = GetSoapEnvelopeHeaderString() + xml.Remove(0, 21) + GetSoapEnvelopeFooterString();

            return stringPayLoad;
        }

		public async Task<bool> Authenticate(MarshRsaUser rsaUser, IUserService _userService, string username)
		{            
            Authenticate authenticateRequest = new Authenticate();
            AuthenticateResponse authenticateResponse = new AuthenticateResponse();
            XmlDocument xDoc = new XmlDocument();
            //var user = await _userService.GetUser(rsaUser.Username);
            var user = await _userService.GetUser(username); //changed to use not hashed username to find user in application
            authenticateRequest.request = GetAuthenticateRequest(rsaUser);
            var xml = SerializeRSARequest(authenticateRequest, "Authenticate");
            var authenticateResponseXmlStr = await _httpClientService.Authenticate(xml);

            //used for RSA authenticate request and response log
            await _emailService.RsaLogEmail("marshevents@proposalonline.com", username, xml, authenticateResponseXmlStr);

            try
            {
                xDoc.LoadXml(authenticateResponseXmlStr);
                authenticateResponse = await BuildAuthenticateResponse(xDoc);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var userStatus = authenticateResponse.identificationData.userStatus;
            var statusCode = authenticateResponse.credentialAuthResultList.acspAuthenticationResponseData.callStatus.statusCode;
            user.DeviceTokenCookie = authenticateResponse.deviceResult.deviceData.deviceTokenCookie;
            if (userStatus == UserStatus.LOCKOUT || userStatus == UserStatus.DELETE)
            {                
                user.Lock();
                await _userService.Update(user);                
            }            
            else if (statusCode == "SUCCESS")
            {
                
                await _userService.Update(user);
                return true;
            }
            //invalid otp or user locked
            return false;
		}

        void UpdateRsaUserFromResponse(GenericResponse response, MarshRsaUser rsaUser)
        {
            rsaUser.CurrentSessionId = response.identificationData.sessionId;
            rsaUser.CurrentTransactionId = response.identificationData.transactionId;
            rsaUser.DeviceTokenCookie = response.deviceResult.deviceData.deviceTokenCookie;
        }

        #region Create Request Elements

        RiskResult GetRiskResultResponse(XmlDocument xDoc)
        {
            RiskResult riskResult = new RiskResult();
            var riskResults = xDoc.GetElementsByTagName("riskResult", "http://ws.csd.rsa.com");
            var riskResultsArray = riskResults[0];

            var listRisk = riskResultsArray.ChildNodes;
            riskResult.riskScore = int.Parse(listRisk.Item(0).InnerText);
            riskResult.riskScoreBand = listRisk.Item(1).InnerText;

            DeviceAssuranceLevels resposeDeviceAssuranceLevel;
            Enum.TryParse(listRisk.Item(3).InnerText, out resposeDeviceAssuranceLevel);
            riskResult.deviceAssuranceLevel = resposeDeviceAssuranceLevel;
            riskResult.triggeredRule = new TriggeredRule();

            var listTrigger = listRisk.Item(2).ChildNodes;
            ActionCode responseActionCode;
            Enum.TryParse(listTrigger.Item(0).InnerText, out responseActionCode);
            ActionApplyType responseActionApplyType;
            Enum.TryParse(listTrigger.Item(2).InnerText, out responseActionApplyType);

            riskResult.triggeredRule.actionType = responseActionApplyType;
            riskResult.triggeredRule.actionCode = responseActionCode;
            riskResult.triggeredRule.actionName = listTrigger.Item(1).InnerText;
            riskResult.triggeredRule.ruleId = listTrigger.Item(4).InnerText;
            riskResult.triggeredRule.ruleName = listTrigger.Item(5).InnerText;

            return riskResult;
        }

        DeviceResult GetDeviceResultResponse(XmlDocument xDoc)
        {
            DeviceResult deviceResult = new DeviceResult();
            var deviceDataResults = xDoc.GetElementsByTagName("deviceData", "http://ws.csd.rsa.com");
            var deviceDataArray = deviceDataResults[0];

            var listDeviceData = deviceDataArray.ChildNodes;
            deviceResult.deviceData = new DeviceData();
            deviceResult.deviceData.deviceTokenCookie = listDeviceData.Item(1).InnerText;

            return deviceResult;
        }

        IdentificationData GetIdentificationDataResponse(XmlDocument xDoc)
        {
            IdentificationData identificationData= new IdentificationData();
            UserStatus reponseUserStatus;
            WSUserType responseUserType;

            try
            {
                var identificationResults = xDoc.GetElementsByTagName("identificationData", "http://ws.csd.rsa.com");
                var identificationArray = identificationResults[0];
                var listIndentification = identificationArray.ChildNodes;

                Enum.TryParse(listIndentification.Item(6).InnerText, out reponseUserStatus);
                identificationData.delegated = bool.Parse(listIndentification.Item(0).InnerText);
                identificationData.groupName = listIndentification.Item(1).InnerText;
                identificationData.orgName = listIndentification.Item(2).InnerText;
                identificationData.sessionId = listIndentification.Item(3).InnerText;
                identificationData.transactionId = listIndentification.Item(4).InnerText;
                identificationData.userName = listIndentification.Item(5).InnerText;
                identificationData.userStatus = reponseUserStatus;
                Enum.TryParse(listIndentification.Item(7).InnerText, out responseUserType);
                identificationData.userType = responseUserType;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return identificationData;
        }

        GenericActionTypeList GetGenericActionTypes ()
		{
			return new GenericActionTypeList {
				genericActionTypes = new GenericActionType [] { GenericActionType.SET_USER_STATUS, GenericActionType.SET_USER_GROUP },
			};
		}

        private string  GetIP()
        {
            var strHostName = System.Net.Dns.GetHostName();
            System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
            System.Net.IPAddress[] addr = ipEntry.AddressList;
            return addr[addr.Length - 1].ToString();
        }

		DeviceRequest GetDeviceRequest (MarshRsaUser rsaUser)
		{
            if (string.IsNullOrEmpty(rsaUser.ClientGenCookie))
            {
                return new DeviceRequest
                {
                    devicePrint = rsaUser.DevicePrint,
                    deviceTokenCookie = rsaUser.DeviceTokenCookie,
                    // following fields required? if so, will need provide web request data - specialized web api?
                    httpAccept = "",
                    httpAcceptEncoding = "",
                    httpAcceptLanguage = "",
                    httpReferrer = rsaUser.HttpReferer,
                    ipAddress = GetIP(),
                    userAgent = rsaUser.UserAgent,
                };
            } else
            {
                return new DeviceRequest
                {
                    devicePrint = rsaUser.DevicePrint,
                    deviceTokenCookie = rsaUser.DeviceTokenCookie,
                    // following fields required? if so, will need provide web request data - specialized web api?
                    httpAccept = "",
                    httpAcceptEncoding = "",
                    httpAcceptLanguage = "",
                    httpReferrer = rsaUser.HttpReferer,
                    ipAddress = GetIP(),
                    userAgent = rsaUser.UserAgent,
                    deviceIdentifier = GetDeviceIdentifier(rsaUser), //added as Marsh request
                };
            }
            
		}

        DeviceIdentifier [] GetDeviceIdentifier(MarshRsaUser rsaUser)
        {
            return new DeviceIdentifier []
            {
                new ClientGenCookie {
                    clientGenCookie = rsaUser.ClientGenCookie,
                }
            };
        }

        IdentificationData GetIdentificationData (MarshRsaUser rsaUser)
		{
            return new IdentificationData {
                delegated = false,          // confirm
                delegatedSpecified = true,
                groupName = "Clients",              // Clients in sample, probably changes
                orgName = rsaUser.OrgName,  // confirm
                //userLoginName = rsaUser.Username,   // hased username from JS here
                userName = rsaUser.Username,        // see above
                //userEmailAddress = rsaUser.Email,
                userStatus = UserStatus.VERIFIED,   // doc default, will need to know how marsh expects these values
                userStatusSpecified = true,
                userType = WSUserType.PERSISTENT,   // doc default, will need to know how marsh expects these values
                userTypeSpecified = true,
                sessionId = rsaUser.CurrentSessionId,
                transactionId = rsaUser.CurrentTransactionId
			};
		}

		MessageHeader GetMessageHeader (RequestType requestType)
		{
			return new MessageHeader {
				apiType = APIType.DIRECT_SOAP_API,  // doc default, will need to know how marsh expects these values
				apiTypeSpecified = true,
				requestType = requestType,          // this looks obvious
				requestTypeSpecified = true,
				version = MessageVersion.Item70,    // Probably safe to use the default
				versionSpecified = true
			};
		}

		EventData [] GetEventData (MarshRsaUser rsaUser)
		{
			return new EventData [] {
				new EventData {
					clientDefinedAttributeList = new ClientDefinedFact[] {
						new ClientDefinedFact {
							//name = "FILTERED_TAM_GROUP",
							//value = "MFA",
							//dataType = DataType.STRING,
							//dataTypeSpecified = true,     // removed as Marsh request
						}
                    },
					eventType = EventType.SESSION_SIGNIN,
					eventTypeSpecified = true,
				}
			};
		}

		CredentialChallengeRequestList GetCredentialChallengeRequestList ()
		{
			return new CredentialChallengeRequestList {
				acspChallengeRequestData = new AcspChallengeRequestData {
					payload = new OTPChallengeRequest ()
				}
			};
		}

		CredentialDataList GetCredentialDataList ()
		{
			return new CredentialDataList {
				acspAuthenticationRequestData = new AcspAuthenticationRequestData ()
			};
		}

		CredentialDataList GetCredentialDataList (MarshRsaUser rsaUser)
		{
			return new CredentialDataList {
				acspAuthenticationRequestData = new AcspAuthenticationRequestData {
					payload = new OTPAuthenticationRequest {
						otp = rsaUser.Otp
					}
				}
			};
		}

		#endregion

		#region Create Requests

		AnalyzeRequest GetAnalyzeRequest (MarshRsaUser rsaUser)
		{
			return new AnalyzeRequest
            {
				actionTypeList = GetGenericActionTypes (),
				deviceRequest = GetDeviceRequest (rsaUser),
				identificationData = GetIdentificationData (rsaUser),
				messageHeader = GetMessageHeader (RequestType.ANALYZE),     // this looks obvious
				autoCreateUserFlag = true,                                  // confirm value
				autoCreateUserFlagSpecified = true,
				credentialDataList = GetCredentialDataList (),
				eventDataList = GetEventData (rsaUser),       
				runRiskType = RunRiskType.ALL,                  // confirm value
				channelIndicator = ChannelIndicatorType.WEB,    // fairly sure that this is supposed to be web
				channelIndicatorSpecified = true,
			};
		}

		UpdateUserRequest GetUpdateUserRequest (MarshRsaUser rsaUser)
		{
			return new UpdateUserRequest {
				actionTypeList = GetGenericActionTypes (),
				deviceRequest = GetDeviceRequest (rsaUser),
				identificationData = GetIdentificationData (rsaUser),
				messageHeader = GetMessageHeader (RequestType.UPDATEUSER)
			};
		}

		ChallengeRequest GetChallengeRequest (MarshRsaUser rsaUser)
		{
			return new ChallengeRequest {
				deviceRequest = GetDeviceRequest(rsaUser),
				identificationData = GetIdentificationData(rsaUser),
				messageHeader = GetMessageHeader(RequestType.CHALLENGE),
				credentialChallengeRequestList = GetCredentialChallengeRequestList()
			};
		}

		AuthenticateRequest GetAuthenticateRequest (MarshRsaUser rsaUser)
		{
			return new AuthenticateRequest {
				deviceRequest = GetDeviceRequest (rsaUser),
				identificationData = GetIdentificationData (rsaUser),
				messageHeader = GetMessageHeader (RequestType.AUTHENTICATE),     // this looks obvious
				credentialDataList = GetCredentialDataList (rsaUser)
			};
		}

        #endregion

        #region
		string GetSoapEnvelopeHeaderString()
		{
            //staging:MarNZ0sa$0Cap16us, production: Password@123456
            return @"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
    <soap:Header>
        <wsse:Security soap:mustUnderstand = ""1"" xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"">
            <wsse:UsernameToken wsu:Id=""UsernameToken-1d15e0d7-37fa-4de8-8bd9-758caa95112c"" xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
                <wsse:Username>MarshNZSOAPUser</wsse:Username>
                <wsse:Password Type = ""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">Password@123456</wsse:Password>
            </wsse:UsernameToken>
        </wsse:Security>
    </soap:Header>
    <soap:Body>";
		}

		string GetSoapEnvelopeFooterString()
		{
			return @"</soap:Body></soap:Envelope>";
		}

        #endregion

    }
} 


