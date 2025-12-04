using ComaxService;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using journeyService.Contract;
using journeyService.Models.flycard;
using journeyService.Models.howazit;
using journeyService.Models.hubspot;
using journeyService.Models.inforu;
using journeyService.Models.tafnit;
using journeyService.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Runtime.Intrinsics.X86;
using System.ServiceModel;
using System.Text;

namespace journeyService.Controllers.V1
{
    public class InforUController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public InforUController(IConfiguration configuration)
        {
            //by default IConfiguration is injected, not realy need to Inject
            _configuration = configuration;
        }
        /// <summary>
        /// call inforU Api to get list unsubscribe 
        /// </summary>
        /// <param name="codeAction"></param>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.InforUServ.GetUnsubscribeList)]
   
        public async Task<ActionResult> GetUnsubscribeList([FromRoute] int codeAction, [FromRoute] string propertyType)
        
        {
            
          




            //from program 
            //http://localhost:5279/api/V1/inforuserv/GetUnsubscribeList/2/Email  //avis + umi codeaction 2
            //http://localhost:5279/api/V1/inforuserv/GetUnsubscribeList/3/Phone //avis + umi codeaction 3


            //http://localhost:5279/api/V1/inforuserv/GetUnsubscribeList/6/Email //officedepots codeaction 6
            //http://localhost:5279/api/V1/inforuserv/GetUnsubscribeList/7/Phone  //officedepots codeaction 7


            //http://umi-appsites/journeyApp/api/V1/inforuserv/GetUnsubscribeList/2/Email 
            //http://umi-appsites/journeyApp/api/V1/inforuserv/GetUnsubscribeList/3/Phone 





            UnsubscribeListResponse? _UnsubscribeListResponse = null;

            int logID = -1;
            string AppNameToRemove = string.Empty;

            UnsubscribePayload payload = null;
            try
            {
           
                
                payload = getBodyJsonRequest(propertyType, 0, getFromDate(), DateTime.Now);
                
                


                //documentation  in HubSpotImportLog table, call to InforU
                logID = DataProvider.InsertHubSpotImportRequest(
                  _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                  Request.GetDisplayUrl(),
                  codeAction,
                  $"GetUnsubscribeListBy{propertyType}"
                  ,payload.FromDate, payload.ToDate
                  );




                //get response from InforU
                _UnsubscribeListResponse = await callRestGetUnsubscribeList(
                    _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                    , logID
                    , payload
                    , codeAction
                    );

                if(_UnsubscribeListResponse.Data !=null)
                {
                    if (codeAction == 2 || codeAction == 3)
                    {
                        AppNameToRemove = "tafnit";
                    }
                    else if (codeAction == 6 || codeAction == 7)
                    {
                        AppNameToRemove = "comax";
                    }

                        // if inforU return records
                        while (_UnsubscribeListResponse.Data.LastFetchedId > 0)
                        {
                            
                                tafnitRemoveDivurResponse? _tafnitRemoveDivurResponse;
                                //loop over the records from InforU 
                                for (int i = 0; i < _UnsubscribeListResponse.Data.Count; i++)
                                {
                                    string propertyValue = _UnsubscribeListResponse.Data.List[i].Value; //email or phone from inforU
                                    #region Remove Divur from Tafnit
                                    if (AppNameToRemove == "tafnit")
                                            {
                                                //unsubscribe from tafnitAvis And Tafnit Umi
                                                for (int j = 0; j < 2; j++)
                                                {
                                                    //remove from tafnit
                                            
                                                    _tafnitRemoveDivurResponse = callRemoveDivurByPhoneOrEmail(codeAction,
                                                        propertyType, propertyValue, logID, (j == 0 ? "tafnitAvis" : "tafnitUmi"));
                                                }
                                            }

                                    #endregion
                                    #region Remove Divur from Comax
                                    if (AppNameToRemove == "comax")
                                    {
                                           await ComaxClubCustomersService(codeAction, propertyType, propertyValue, logID);
                                           
                            }
                                    #endregion
                          }

                            
                            


                            //documentation  in HubSpotImportLog table, call to InforU
                            payload = getBodyJsonRequest(propertyType, _UnsubscribeListResponse.Data.LastFetchedId, getFromDate(), DateTime.Now);



                            logID = DataProvider.InsertHubSpotImportRequest(
                               _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                               "Rereading-GetUnsubscribeList",
                               codeAction,
                               $"GetUnsubscribeListBy{propertyType}",
                               payload.FromDate, payload.ToDate

                               );




                            //get response from InforU

                            _UnsubscribeListResponse = await callRestGetUnsubscribeList(
                                _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                                , logID
                                , payload
                                , codeAction
                                );

                        }
                }
                

            }
            catch (Exception ex)
            {
                _UnsubscribeListResponse = new UnsubscribeListResponse()
                {
                    StatusId = -1,
                    StatusDescription = ex.Message
                };

                DataProvider.UpdateHubSpotImportResponse(
                     _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                    , logID
                    , _UnsubscribeListResponse
                    );

            }

           

            return Ok(_UnsubscribeListResponse);
  

        }

        public async Task<UnsubscribeListResponse> callRestGetUnsubscribeList(
              string ConnectionString, int logID
            , UnsubscribePayload payload,int codeAction)
        {
            //convert object to string
            string jsonData = JsonConvert.SerializeObject(payload);
            
            HttpResponseMessage response;
            UnsubscribeListResponse? result = null;
            string jsonResponseContent = string.Empty;
            string UrlExternal = _configuration.GetValue<string>("InforU:ContactsEndPoint") + "GetUnsubscribeList" ?? "";
            string username;
            string token;
            try
            {

                if (codeAction == 2 || codeAction == 3)
                {
                    username = _configuration.GetValue<string>("InforU:UserAvisEndPoint") ?? "";
                    token = _configuration.GetValue<string>("InforU:TokenAvisEndPoint") ?? "";
                }
                else if (codeAction == 6 || codeAction == 7)
                {
                    username = _configuration.GetValue<string>("InforU:UserOfficeDepotsEndPoint") ?? "";
                    token = _configuration.GetValue<string>("InforU:TokenOfficeDepotsEndPoint") ?? "";
                }
                else
                {
                    username = "";
                    token = "";
                }
                
                using (HttpClient client = new HttpClient())
                {
                    //string basicAuthorizationString =
                    //    System.Convert.ToBase64String(
                    //        Encoding.UTF8.GetBytes(
                    //              (_configuration.GetValue<string>("InforU:UserAvisEndPoint") ?? "")
                    //              + ":"
                    //              + (_configuration.GetValue<string>("InforU:TokenAvisEndPoint") ?? "")
                    //        )
                    //);

                    string basicAuthorizationString =
                        Convert.ToBase64String(
                            Encoding.UTF8.GetBytes($"{username}:{token}")
                        );

                    HttpRequestMessage request = new HttpRequestMessage(
                        HttpMethod.Post,
                        UrlExternal
                        );
                    
                    request.Headers.Add(
                        "Authorization",
                        "Basic " + basicAuthorizationString);

                    var content = new StringContent(jsonData,
                        Encoding.UTF8,
                        "application/json");

                    request.Content = content;

                    DataProvider.UpdateHubSpotImportRequest(ConnectionString
                        ,logID
                        , jsonData
                        , UrlExternal);

                    response = await client.SendAsync(request);

                    jsonResponseContent = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<UnsubscribeListResponse>(jsonResponseContent);


                    DataProvider.UpdateHubSpotImportResponse(
                        ConnectionString
                       , logID
                       , result

                       );

                    
                }
            }
            catch (Exception ex)
            {
                result = new UnsubscribeListResponse()
                {
                    StatusId = -1,
                    StatusDescription = ex.Message
                };

            }


            return result;
        }

        public DateTime getFromDate()
        {
            //if sunday takes 2 days(sunday &  friday & saturday)
            DateTime fromdate = DateTime.Now;
            fromdate = DateTime.Now.AddDays(-1);
            if (DateTime.Now.DayOfWeek.ToString() == "Sunday")
                fromdate = DateTime.Now.AddDays(-2);

            return fromdate;
        }
        public UnsubscribePayload getBodyJsonRequest(string Type,long LastFetchedId
            ,DateTime fromdate,DateTime todate)
        {

            //  return JsonConvert.SerializeObject(new
            //     {
            //        //FromDate = fromdate.ToString("yyyy/MM/dd"), //"2024/12/31"
            //        //ToDate = todate.ToString("yyyy/MM/dd"), //"2024/12/31"
            //        FromDate = "2025/01/05",
            //        ToDate =  "2025/01/05",
            //        Type = Type,
            //        LastFetchedId = LastFetchedId
            //});

            UnsubscribePayload _UnsubscribePayload = new UnsubscribePayload();
            _UnsubscribePayload.Type = Type;
            _UnsubscribePayload.LastFetchedId = LastFetchedId;
            _UnsubscribePayload.FromDate = fromdate;
            _UnsubscribePayload.ToDate = DateTime.Now;


            //_UnsubscribePayload.FromDate = DateTime.Parse("2025/04/01");
            //_UnsubscribePayload.ToDate = DateTime.Parse("2025/10/26");

            return _UnsubscribePayload;
        }
        /// <summary>
        /// unsubscribe from tafnit
        /// </summary>
        /// <param name="codeAction"></param>
        /// <param name="propertyType"></param>
        /// <param name="propertyValue"></param>
        /// <param name="logID_HubSpotImportLog"></param>
        /// <param name="tafnitUnit"></param>
        /// <returns></returns>
        public tafnitRemoveDivurResponse callRemoveDivurByPhoneOrEmail(int codeAction ,string propertyType, 
            string propertyValue,int logID_HubSpotImportLog,string tafnitUnit)
        {
            tafnitRemoveDivurResponse? _tafnitRemoveDivurResponse;
            //http://localhost:5279/
            var baseurl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}{_configuration.GetValue<string>("General:websitename") ?? ""}";

            
            var locationuri = baseurl + "/" + 
                ApiRoutes.tafnitServ.RemoveDivurByPhoneOrEmail
                .Replace("{codeaction}", codeAction.ToString())
                .Replace("{propertyType}", propertyType)
                .Replace("{propertyValue}", propertyValue)
                .Replace("{logID_HubSpotImportLog}", logID_HubSpotImportLog.ToString())
                .Replace("{tafnitUnit}", tafnitUnit
                );

            


            var jsonString = "";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(locationuri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                jsonString = reader.ReadToEnd();
            }
            _tafnitRemoveDivurResponse = JsonConvert.DeserializeObject<tafnitRemoveDivurResponse>(jsonString);
            return _tafnitRemoveDivurResponse;

        }

        public async Task ComaxClubCustomersService(int codeAction, string propertyType,
            string propertyValue, int logID_HubSpotImportLog)
        {
            int logID = -1;
            
            tafnitRemoveDivurResponse? _tafnitRemoveDivurResponse = null;

            //int logID11 = -1;


            try
            {
                var client = new ClubCustomers_ServiceSoapClient(
                    ClubCustomers_ServiceSoapClient.EndpointConfiguration.ClubCustomers_ServiceSoap
                );
                //client.Endpoint.Address = new System.ServiceModel.EndpointAddress(
                //                          client.Endpoint.Address.Uri.ToString().Replace("http://", "https://")
                //);
                // מתקנים את הכתובת ל-https
                client.Endpoint.Address = new EndpointAddress(
                    "https://ws.comax.co.il/Comax_WebServices/ClubCustomers_Service.asmx"
                );


                // מתקנים את ה-binding ל-HTTPS
                var b = (BasicHttpBinding)client.Endpoint.Binding;
                b.Security.Mode = BasicHttpSecurityMode.Transport;
                b.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;


                var search = new ClsClubCustomersSearch();
                if (propertyType == "Email")
                {
                    search.Email = propertyValue; //"drorch11@gmail.com";
                }
                else if (propertyType == "Phone")
                {
                    search.PhoneOrMobile = propertyValue; //"0548054858";

                }

                var request = new Get_ClubCustomerDetailsBySearchRequest
                {
                    ClubCustomersSearch = search,
                    LoginID = _configuration.GetValue<string>("ApiComax:LoginID") ?? "",
                    LoginPassword = _configuration.GetValue<string>("ApiComax:LoginPassword") ?? "" 
                };

                //logID11 = DataProvider.InsertTafnitUnsubscribeRequet(
                //         _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                //         codeAction,
                //         logID_HubSpotImportLog,
                //         client.Endpoint.Address.ToString(),
                //         "Unsubscribe_" + propertyType,
                //         propertyValue,
                //         "comax"
                //    );


                //Get ID from Comax By propertyType
                var response = await client.Get_ClubCustomerDetailsBySearch1Async(request);
                if (response.Get_ClubCustomerDetailsBySearchResult != null)
                {
                    _tafnitRemoveDivurResponse = new tafnitRemoveDivurResponse()
                    {
                        StatusCode = -1,
                        StatusDesc = ""

                    };

                    var customers = response.Get_ClubCustomerDetailsBySearchResult;
                    foreach (var c in customers)
                    {
                        logID = DataProvider.InsertTafnitUnsubscribeRequet(
                             _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                             codeAction,
                             logID_HubSpotImportLog,
                             client.Endpoint.Address.ToString(),
                             "Unsubscribe_" + propertyType,
                             propertyValue,
                             "comax"
                        );

                        string expString = c.ExpirationDate; // למשל "23/02/2022"

                     

                        // מנסים להמיר לתאריך אמיתי לפי הפורמט dd/MM/yyyy
                        if (DateTime.TryParseExact(expString, "dd/MM/yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime expDate))
                        {
                            // עכשיו expDate הוא DateTime אמיתי
                            if (expDate > DateTime.Now.Date)
                            {
                                //cant use regular soap like Get_ClubCustomerDetailsBySearch1Async in Set_ClubCustomerDetailsAsync
                                //בגלל שקומקס הוסיפו את פרמטר תאריך בכתובת ולא ב  BODY
                                //לכן נשתמש ב httpclient
                                using (HttpClient _httpClient = new HttpClient())
                                {

                                    string expirationStr = DateTime.Now.ToString("dd/MM/yyyy");

                                    //client.Endpoint.Address = new System.ServiceModel.EndpointAddress(
                                    //        client.Endpoint.Address.Uri.ToString().Replace("http://", "https://")
                                    //);

                                    //string url = $"https://ws.comax.co.il/Comax_WebServices/ClubCustomers_Service.asmx?op=Set_ClubCustomerDetails_Simple&ExpirationDate={Uri.EscapeDataString(expirationStr)}";

                                    string url = $"{client.Endpoint.Address}?op=Set_ClubCustomerDetails_Simple&ExpirationDate={Uri.EscapeDataString(expirationStr)}";

                                    var soapEnvelope = $@"
                                    <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                      <soap:Body>
                                        <Set_ClubCustomerDetails_Simple xmlns=""http://ws.comax.co.il/Comax_WebServices/"">
                                          <ID>{c.ID}</ID>
                                          <LoginID>{_configuration.GetValue<string>("ApiComax:LoginID") ?? ""}</LoginID> 
                                          <LoginPassword>{_configuration.GetValue<string>("ApiComax:LoginPassword") ?? ""}</LoginPassword>
                                        </Set_ClubCustomerDetails_Simple>
                                      </soap:Body>
                                    </soap:Envelope>";


                                    var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                                    string action = "http://ws.comax.co.il/Comax_WebServices/Set_ClubCustomerDetails_Simple";
                                    // חובה להוסיף את SOAPAction כדי שהשרת יבין איזו פעולה לקרוא
                                    content.Headers.Add("SOAPAction", $"\"{action}\"");

                                    // שליחת הבקשה
                                    var response1 = await _httpClient.PostAsync(url, content);

                                    _tafnitRemoveDivurResponse.StatusCode = (int)response1.StatusCode;
                                    _tafnitRemoveDivurResponse.StatusDesc = response1.ReasonPhrase ?? "";
                                    response1.EnsureSuccessStatusCode();

                                    DataProvider.UpdateTafnitUnsubscribeResponse(
                                       _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                                      , logID
                                      , _tafnitRemoveDivurResponse
                                      );



                                }
                            }
                            else
                            {
                                _tafnitRemoveDivurResponse = new tafnitRemoveDivurResponse()
                                {
                                    StatusCode = 0,
                                    StatusDesc = $"תאריך של הלקוח פג תוקף:  {expString}"

                                };

                                DataProvider.UpdateTafnitUnsubscribeResponse(
                                   _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                                  , logID
                                  , _tafnitRemoveDivurResponse
                                  );
                            }
                        }
                        else if (string.IsNullOrEmpty(c.ExpirationDate))
                        {
                            //Console.WriteLine("תאריך לא תקין: " + expString);
                            _tafnitRemoveDivurResponse = new tafnitRemoveDivurResponse()
                            {
                                StatusCode = 0,
                                StatusDesc = $"לא נמצא תאריך:  {expString}"

                            };

                            DataProvider.UpdateTafnitUnsubscribeResponse(
                               _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                              , logID
                              , _tafnitRemoveDivurResponse
                              );
                        }
                        else
                        {
                            //Console.WriteLine("תאריך לא תקין: " + expString);
                            _tafnitRemoveDivurResponse = new tafnitRemoveDivurResponse()
                            {
                                StatusCode = 0,
                                StatusDesc = $"תאריך לא תקין:  {expString}"

                            };

                            DataProvider.UpdateTafnitUnsubscribeResponse(
                               _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                              , logID
                              , _tafnitRemoveDivurResponse
                              );
                        }

                        //c.ExpirationDate

                        /*
                        var _updateParam = new ClsClubCustomers
                        {
                            ID = c.ID,
                            ExpirationDate = DateTime.Now.ToString("dd/MM/yyyy")

                        };
                        var _setAllupdateParam = new Set_ClubCustomerDetailsRequest
                        {
                            CustomerDetails = _updateParam,
                            LoginID = _configuration.GetValue<string>("ApiComax:LoginID") ?? "",//"officetest12",
                            LoginPassword = _configuration.GetValue<string>("ApiComax:LoginPassword") ?? "" //"officetest21"
                        };
                        var responseUpdate = await client.Set_ClubCustomerDetailsAsync(_setAllupdateParam);
                        */


                        
                            


                    }
                }
                else
                {
                    _tafnitRemoveDivurResponse = new tafnitRemoveDivurResponse()
                    {
                        StatusCode = 0,
                        StatusDesc = $" לא נמצא לקוח עבור {propertyValue}"

                    };

                    DataProvider.UpdateTafnitUnsubscribeResponse(
                       _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                      , logID
                      , _tafnitRemoveDivurResponse
                      );
                }

            }
            catch (Exception ex)
            {
                _tafnitRemoveDivurResponse = new tafnitRemoveDivurResponse()
                {
                    StatusCode = -1,
                    StatusDesc = ex.Message
                };

                DataProvider.UpdateTafnitUnsubscribeResponse(
                     _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                    , logID
                    , _tafnitRemoveDivurResponse
                    );

                //DataProvider.UpdateTafnitUnsubscribeResponse(
                //     _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                //    , logID11
                //    , _tafnitRemoveDivurResponse
                //    );

            }
        
        }
    }
}
