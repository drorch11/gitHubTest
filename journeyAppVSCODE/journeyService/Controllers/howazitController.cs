using journeyService.Models;
using journeyService.Models.howazit;
using journeyService.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.X86;
using System.Text;
// Remark: API endpoints for Howazit integrations.

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace journeyService.Controllers
{
    [Route("api/howazit")]
    [ApiController]
    public class howazitController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IOptions<ParamsSetting> _paramssetting;
        private string ConnectionString = string.Empty;

        #region ctor
        public howazitController(IConfiguration configuration, 
            IOptions<ParamsSetting> paramssetting)
        {
            //by default IConfiguration is injected, not realy need to Inject
            _config = configuration;
            _paramssetting = paramssetting;

        }
        #endregion
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            //http://localhost/journeyApp/api/howazit
            //http://localhost:5279/api/howazit
            return Ok();
        }

        [HttpGet("add/{first}/{second}")]
        public ActionResult<int> AddTwoNumbers(int first, int second)
        {
            return Ok(first + second);
        }
        #region DeliveryCarAvis
        [HttpPost("DeliveryCarAvis/{codeAction}")]
        public async Task<ActionResult> DeliveryCarAvis([FromRoute] int codeAction,
                [FromBody] howazitorderAvis _howazitorderAvis)
        {

            //from site,

            //http://localhost/journeyApp/api/howazit/DeliveryCarAvis/5
            //from program 
            //http://localhost:5279/api/howazit/DeliveryCarAvis/5

            //http://umi-appsites/journeyApp/api/howazit/DeliveryCarAvis
              

            bool istestEnvironment = General.IsTestEnvironment(Request.GetDisplayUrl());
            //istestEnvironment = true;
            int logID = -1;

            logID = DataProvider.InsertinforULogUrlRequet(
                    _config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "",
                    Request.GetDisplayUrl(),
                    codeAction,
                    "howazit-DeliveryCarAvis"
                    );

            #region fill howazitHeader class from howazitorderAvis class
            howazitHeader payload = DataProvider.DeliveryCarAvis(
                   _howazitorderAvis
                   , istestEnvironment
                   , _config
                   , _paramssetting
                   );

            
            #endregion
            #region complete data for howazitHeader class
            payload.ExternalSource = "UMI";
            payload.ExternalBusinessId = "UMI";
            payload.EndDate = DateTime.Now.ToString("u");
            payload.EntryCode = _howazitorderAvis.entrycode;
            payload.IsTest = istestEnvironment;
            #endregion

            howazitResponse howazitResponse;
            if (String.IsNullOrEmpty(payload.Phone))
            {
                howazitResponse = await callhowazitIllegalPayLoad(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "",
                    logID,
                    "phone is empty");

                DataProvider.UpdateinforULogResponse(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? ""
                            , logID
                            , ""
                            , -1
                            , howazitResponse.Accepted
                            , howazitResponse.ErrorDescription
                            );
            }
            else

                howazitResponse = await callhowazitEndPoint(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "",
                    logID,
                    payload,
                    "howazit-DeliveryCarAvis"
                    );


            return Ok(howazitResponse);
        }
        #endregion
        #region DeliveryCarUmi
        [HttpGet("DeliveryCarUmi/{codeAction}/{OrderNumber}")]
        public async Task<ActionResult> DeliveryCarUmi([FromRoute] int codeAction, [FromRoute] string OrderNumber)
        {

            
            //from site,
            //http://localhost/journeyApp/api/howazit/DeliveryCarUmi/50/324303
            //from program 
            //http://localhost:5279/api/howazit/DeliveryCarUmi/50/324303

            //http://umi-appsites/journeyApp/api/howazit/DeliveryCarUmi/50/324303

            bool istestEnvironment = General.IsTestEnvironment(Request.GetDisplayUrl());
            //istestEnvironment = true;




            int logID = -1;

            logID = DataProvider.InsertinforULogUrlRequet(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "",
                    Request.GetDisplayUrl(),
                    codeAction,
                    "howazit-DeliveryCarUmi"
                    );
            
                
            #region fill howazitHeader class from DB
            howazitHeader payload = DataProvider.DeliveryCarUmi(_config.GetValue<string>("ConnectionStrings:odbcUmi") ?? ""
                   , codeAction
                   , OrderNumber
                   , istestEnvironment
                   , _config
                   , _paramssetting
                   );


            #endregion

            #region complete data for howazitHeader class
            payload.ExternalSource = "UMI";
            payload.ExternalBusinessId = "UMI";
            payload.EndDate = DateTime.Now.ToString("u");
            payload.EntryCode = "מסירה";
            payload.IsTest = istestEnvironment;
            #endregion

            howazitResponse howazitResponse;
            if (String.IsNullOrEmpty(payload.Phone))
            {
                howazitResponse = await callhowazitIllegalPayLoad(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "",
                    logID,
                    "phone is empty");
                
                DataProvider.UpdateinforULogResponse(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? ""
                            , logID
                            , ""
                            , -1
                            , howazitResponse.Accepted
                            , howazitResponse.ErrorDescription
                            );
            }



            else
                
                howazitResponse = await callhowazitEndPoint(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "",
                    logID,
                    payload,
                    "howazit-DeliveryCarUmi"
                    );


            return Ok(howazitResponse);






        }
        #endregion
        #region Satisfaction
        [HttpGet("Satisfaction/{codeAction}/{Lakuah}/{MisparErua}")]
        public async Task<ActionResult> Satisfaction(int codeAction, string Lakuah, string MisparErua)
        {

            //from site,
            //http://localhost/journeyApp/api/howazit/Satisfaction/44/274594/71114843
            //from program 
            //http://localhost:5279/api/howazit/Satisfaction/44/274594/71114843

            //http://umi-appsites/journeyApp/api/howazit/Satisfaction/44/274594/71114843


            bool istestEnvironment = General.IsTestEnvironment(Request.GetDisplayUrl());
            //istestEnvironment = true;

            int logID = -1;
            
            logID = DataProvider.InsertinforULogUrlRequet(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "",
                    Request.GetDisplayUrl(),
                    codeAction,
                    "howazit-Satisfaction"
                    );

            
            #region fill howazitHeader class from DB
            howazitHeader payload = DataProvider.Satisfaction(_config.GetValue<string>("ConnectionStrings:odbcAvis") ?? ""
                   , codeAction
                   , Lakuah
                   , MisparErua
                   , "Pniyot"
                   , istestEnvironment
                   , _config
                   , _paramssetting
                   );


            #endregion

            #region complete data for howazitHeader class
            payload.ExternalSource = "UMI";
            payload.ExternalBusinessId = "UMI";
            //payload.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
            payload.EndDate = DateTime.Now.ToString("u");
            payload.EntryCode = "AVIS-Pniyot";
            payload.IsTest = istestEnvironment;
            #endregion


            howazitResponse howazitResponse;
            if (String.IsNullOrEmpty(payload.Phone))
            {
                
                howazitResponse = await callhowazitIllegalPayLoad(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "",
                    logID,
                    "phone is empty");

                DataProvider.UpdateinforULogResponse(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? ""
                            , logID
                            , ""
                            , -1
                            , howazitResponse.Accepted
                            , howazitResponse.ErrorDescription
                            );
            }



            else
                howazitResponse = await callhowazitEndPoint(_config.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "",
                    logID,
                    payload,
                    "howazit-Satisfaction");
            

            return Ok(howazitResponse);
        }

        #endregion
        #region howazitResponse
        public async Task<howazitResponse> callhowazitEndPoint(string ConnectionString, int logID
            , howazitHeader payload,string serviceName)
        {
            //convert object to string
            string jsonData = JsonConvert.SerializeObject(payload);
         
            string jsonResponseContent = string.Empty;




            HttpResponseMessage response;
            howazitResponse? result = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    content.Headers.Add("x-howazit-token", _config.GetValue<string>("ApiEndPoint:accessToken") ?? "");
                    

                    DataProvider.UpdateinforULoghowazitRequest(ConnectionString, logID, payload, jsonData, serviceName);

                    response = await client.PostAsync(_config.GetValue<string>("ApiEndPoint:howazitEndPoint") ?? "", content);
                    

                    if (response.IsSuccessStatusCode)
                    {

                        jsonResponseContent = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<howazitResponse>(jsonResponseContent);
                    }
                    else
                        result = new howazitResponse()
                        {
                            Accepted = false,
                            ErrorDescription = "IsSuccessStatusCode is empty"
                        };

                    DataProvider.UpdateinforULogResponse(ConnectionString, logID
                            , jsonResponseContent
                            , (int)response.StatusCode
                            , result.Accepted
                            , result.ErrorDescription
                            );
                }
            }
            catch (Exception ex)
            {
                result = new howazitResponse()
                {
                    Accepted = false,
                    ErrorDescription = ex.Message
                };

            }


            return result;
        }
        public async Task<howazitResponse> callhowazitIllegalPayLoad(string ConnectionString, int logID,
            string ErrorMsg)
        {




            DataProvider.UpdateinforULogError(ConnectionString,
                    logID,
                    ErrorMsg);

            return new howazitResponse()
            {
                Accepted = false,
                ErrorDescription = ErrorMsg
            };

        }
        #endregion

      
    }
}
