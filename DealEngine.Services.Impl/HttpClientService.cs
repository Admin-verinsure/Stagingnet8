using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;

namespace DealEngine.Services.Impl
{
    public class HttpClientService : IHttpClientService
    {
        IMapperSession<LogInfo> _logInfoMapperSession;
        IAppSettingService _appSettingService;
        public HttpClientService(
            IMapperSession<LogInfo> logInfoMapperSession, 
            IAppSettingService appSettingService
            )
        {
            _appSettingService = appSettingService;
            _logInfoMapperSession = logInfoMapperSession;
        }

        public async Task<string> Analyze(string analyzeRequest)
        {
            var responseMessage = "";            
            string service = "https://" + _appSettingService.MarshRSAEndPoint;
            string SOAPAction = "rsa:analyze:Analyze";

            SocketsHttpHandler _socketsHttpHandler;
            HttpRequestMessage _httpRequestMessage;
            HttpResponseMessage response;

            _socketsHttpHandler = new SocketsHttpHandler()
            {
                Credentials = new NetworkCredential("MarshNZSOAPUser", _appSettingService.MarshRSACredentials),
            };

            _httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(service),
                Method = HttpMethod.Post,
                Content = new StringContent(analyzeRequest, Encoding.UTF8, "text/xml"),
            };
            _httpRequestMessage.Headers.Add("SOAPAction", SOAPAction);

            try
            {               
                HttpClient client = new HttpClient(_socketsHttpHandler);
                response = await client.SendAsync(_httpRequestMessage);
                response.EnsureSuccessStatusCode();
                responseMessage = await response.Content.ReadAsStringAsync();                

                client.Dispose();
            }
            catch (HttpRequestException e)
            {                
                responseMessage = e.Message + " status code not 200";
            }
            catch (Exception ex)
            {
                responseMessage = ex.Message;
            }
            _socketsHttpHandler.Dispose();
            _httpRequestMessage.Dispose();
            
            return responseMessage;
        }

        public async Task<string> UpdateUser(string updateRequest)
        {
            var responseMessage = "";            
            string service = "https://" + _appSettingService.MarshRSAEndPoint;
            string SOAPAction = "rsa:udpateuser:UpdateUser";

            SocketsHttpHandler _socketsHttpHandler;
            HttpRequestMessage _httpRequestMessage;
            HttpResponseMessage response;

            _socketsHttpHandler = new SocketsHttpHandler()
            {
                Credentials = new NetworkCredential("MarshNZSOAPUser", _appSettingService.MarshRSACredentials),
            };

            _httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(service),
                Method = HttpMethod.Post,
                Content = new StringContent(updateRequest, Encoding.UTF8, "text/xml"),
            };
            _httpRequestMessage.Headers.Add("SOAPAction", SOAPAction);

            try
            {
                HttpClient client = new HttpClient(_socketsHttpHandler);
                response = await client.SendAsync(_httpRequestMessage);
                response.EnsureSuccessStatusCode();
                responseMessage = await response.Content.ReadAsStringAsync();                

                client.Dispose();
            }
            catch (HttpRequestException e)
            {
                responseMessage = e.Message + " status code not 200";
            }
            catch (Exception ex)
            {
                responseMessage = ex.Message;
            }
            _socketsHttpHandler.Dispose();
            _httpRequestMessage.Dispose();
            
            return responseMessage;
        }

        public async Task<string> Challenge(string challengeRequest)
        {
            var responseMessage = "";
            string service = "https://" + _appSettingService.MarshRSAEndPoint;
            string SOAPAction = "rsa:challenge:Challenge";

            SocketsHttpHandler _socketsHttpHandler;
            HttpRequestMessage _httpRequestMessage;
            HttpResponseMessage response;

            _socketsHttpHandler = new SocketsHttpHandler()
            {
                Credentials = new NetworkCredential("MarshNZSOAPUser", _appSettingService.MarshRSACredentials),
            };

            _httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(service),
                Method = HttpMethod.Post,
                Content = new StringContent(challengeRequest, Encoding.UTF8, "text/xml"),
            };
            _httpRequestMessage.Headers.Add("SOAPAction", SOAPAction);

            try
            {
                HttpClient client = new HttpClient(_socketsHttpHandler);
                response = await client.SendAsync(_httpRequestMessage);
                response.EnsureSuccessStatusCode();
                responseMessage = await response.Content.ReadAsStringAsync();                

                client.Dispose();
            }
            catch (HttpRequestException e)
            {
                responseMessage = e.Message + " status code not 200";
            }
            catch (Exception ex)
            {
                responseMessage = ex.Message;
            }

            _socketsHttpHandler.Dispose();
            _httpRequestMessage.Dispose();

            return responseMessage;
        }

        public async Task<string> Authenticate(string authenticateRequest)
        {
            var responseMessage = "";
            string service = "https://" + _appSettingService.MarshRSAEndPoint;
            string SOAPAction = "rsa:authenticate:Authenticate";

            SocketsHttpHandler _socketsHttpHandler;
            HttpRequestMessage _httpRequestMessage;
            HttpResponseMessage response;

            _socketsHttpHandler = new SocketsHttpHandler()
            {
                Credentials = new NetworkCredential("MarshNZSOAPUser", _appSettingService.MarshRSACredentials),
            };

            _httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(service),
                Method = HttpMethod.Post,
                Content = new StringContent(authenticateRequest, Encoding.UTF8, "text/xml"),
            };
            _httpRequestMessage.Headers.Add("SOAPAction", SOAPAction);

            try
            {
                HttpClient client = new HttpClient(_socketsHttpHandler);
                response = await client.SendAsync(_httpRequestMessage);
                response.EnsureSuccessStatusCode();
                responseMessage = await response.Content.ReadAsStringAsync();

                client.Dispose();
            }
            catch (HttpRequestException e)
            {
                responseMessage = e.Message + " status code not 200";
            }
            catch (Exception ex)
            {
                responseMessage = ex.Message;
            }

            _socketsHttpHandler.Dispose();
            _httpRequestMessage.Dispose();

            return responseMessage;
        }

        public async Task<string> CreateEGlobalInvoice(string xmlPayload)
        {
            var responseMessage ="";            
            var SOAPAction = @"http://www.example.org/invoice-service/createInvoice";
            var service = "https://" + _appSettingService.MarshEglobalEndpoint; //"https://stg.eglobalinvp.marsh.com/services/invoice/service"; //"https://staging.ap.marsh.com:19443/services/invoice/service"; old staging end point
            var body = generateBody(xmlPayload);
            HttpResponseMessage response;
            SocketsHttpHandler _socketsHttpHandler;
            HttpRequestMessage _httpRequestMessage;

            _socketsHttpHandler = new SocketsHttpHandler()
            {
                Credentials = new NetworkCredential(_appSettingService.MarshEglobalUsername, _appSettingService.MarshEglobalPassword),
            };

            _httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(service),
                Method = HttpMethod.Post,
                Content = new StringContent(body, Encoding.UTF8, "text/xml"),
            };
            _httpRequestMessage.Headers.Add("SOAPAction", SOAPAction);

            try
            {
                HttpClient client = new HttpClient(_socketsHttpHandler);
                //client.Timeout = TimeSpan.FromSeconds(300);
                response = await client.SendAsync(_httpRequestMessage);
                //response = await client.SendAsync(_httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);
                Thread.Sleep(1000);
                response.EnsureSuccessStatusCode();
                responseMessage = await response.Content.ReadAsStringAsync();
                client.Dispose();
            }
            catch (HttpRequestException e)
            {
               responseMessage = e.Message + " status code not 200";
            }
            catch (Exception ex)
            {
               responseMessage = ex.Message + ex.InnerException + ex.StackTrace;
            }
            _socketsHttpHandler.Dispose();
            _httpRequestMessage.Dispose();

            return responseMessage;
        }

        public async Task<string> GetEglobalStatus()
        {
            var responseMessage = "";
            var SOAPAction = "http://www.example.org/invoice-service/getEGlobalSiteStatus";
            var service = "https://" + _appSettingService.MarshEglobalEndpoint; //"https://stg.eglobalinvp.marsh.com/services/invoice/service"; //"https://staging.ap.marsh.com:19443/services/invoice/service"; old staging end point
            var body = GenerateGetSiteActiveSoapBody();
            Envelope result = new Envelope();
            HttpResponseMessage response;
            SocketsHttpHandler _socketsHttpHandler;
            HttpRequestMessage _httpRequestMessage;
            XmlSerializer serializer; 
            StringReader rdr;           

            _socketsHttpHandler = new SocketsHttpHandler()
            {
                Credentials = new NetworkCredential(_appSettingService.MarshEglobalUsername, _appSettingService.MarshEglobalPassword),                
            };

            _httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(service),
                Method = HttpMethod.Post,
                Content = new StringContent(body, Encoding.UTF8, "text/xml"),
            };
            _httpRequestMessage.Headers.Add("SOAPAction", SOAPAction);

            try
            {
                HttpClient client = new HttpClient(_socketsHttpHandler);
                response = await client.SendAsync(_httpRequestMessage);
                response.EnsureSuccessStatusCode();
                responseMessage = await response.Content.ReadAsStringAsync();
                serializer = new XmlSerializer(typeof(Envelope));
                rdr = new StringReader(responseMessage);
                result = (Envelope)serializer.Deserialize(rdr);
                client.Dispose();
            }
            catch (HttpRequestException e)
            {
                responseMessage = e.Message + " status code not 200";
            }
            catch (Exception ex)
            {
                responseMessage = ex.Message;
            }

            _socketsHttpHandler.Dispose();
            _httpRequestMessage.Dispose();

            return result.Body.getEGlobalSiteStatusResponse;
        }

        private string generateBody(string xmlPayload)
        {
            //var formattedString = xmlPayload.Remove(0, 22);
            string htmlEncodedString = HttpUtility.HtmlEncode(xmlPayload);
            string body = @"<?xml version=""1.0"" encoding=""utf-8""?><soapenv:Envelope xmlns:soapenv = ""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:inv = ""http://www.example.org/invoice-service/"">    
                            <soapenv:Header/>     
                                <soapenv:Body>      
                                    <inv:createInvoice>       
                                        <xmlStr>{0}</xmlStr>       
                                        <site>NZL</site>       
                                    </inv:createInvoice>       
                                </soapenv:Body>
                            </soapenv:Envelope>";
            string strxml = string.Format(body, htmlEncodedString);
            
            return strxml;
        }
        private string GenerateGetSiteActiveSoapBody()
        {
            string body = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:inv=""http://www.example.org/invoice-service/"">
                <soapenv:Header/>
                    <soapenv:Body>
                        <inv:getEGlobalSiteStatus>
                            <site>NZL</site>
                        </inv:getEGlobalSiteStatus>    
                    </soapenv:Body>
                </soapenv:Envelope>";

            return body;
        }
                
        #region GetEGlobalResponse
        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
        public partial class Envelope
        {

            private EnvelopeBody bodyField;

            /// <remarks/>
            public EnvelopeBody Body
            {
                get
                {
                    return this.bodyField;
                }
                set
                {
                    this.bodyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public partial class EnvelopeBody
        {

            private string getEGlobalSiteStatusResponseField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.example.org/invoice-service/")]
            public string getEGlobalSiteStatusResponse
            {
                get
                {
                    return this.getEGlobalSiteStatusResponseField;
                }
                set
                {
                    this.getEGlobalSiteStatusResponseField = value;
                }
            }
        }
        #endregion
    }
}