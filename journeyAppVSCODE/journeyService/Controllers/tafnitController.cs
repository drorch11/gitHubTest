using journeyService.Models.howazit;
using journeyService.Models.leasing;
using journeyService.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace journeyService.Controllers
{
    [Route("api/tafnit")]
    [ApiController]
    public class tafnitController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public tafnitController(IConfiguration configuration)
        {

            //by default IConfiguration is injected, not realy need to Inject
            _configuration = configuration;
        }

        #region Api function
        
        [HttpGet("IfAllowedSendMsgForIska/{LogIdInforU}/{HpNo}/{DealNo}/{Step}/{LogIdLeasingContractRenew}")]
        public async Task<ActionResult> IfAllowedSendMsgForIska([FromRoute] tafnitRenewDealRoute _RenewDealRoute)

        {

            //from site,
            //http://localhost/journeyApp/api/tafnit/IfAllowedSendMsgForIska/970/34289595/3510056915/1/111

            //from program 
            //http://localhost:5279/api/tafnit/IfAllowedSendMsgForIska/970/34289595/3510056915/1/111

            //http://umi-appsites/journeyApp/api/tafnit/IfAllowedSendMsgForIska/970/34289595/3510056915/1/111


            IfAllowedSendMsgForIskaResponse _IfAllowedSendMsgForIskaResponse;

            //concat Api Url
            string ReqeustUrl = (_configuration.GetValue<string>("ApiTafnit:TafnitEndPoint") ?? "")
                         + "DivurMgr/IfAllowedSendMsgForIska/"
                         + _RenewDealRoute.hpno + "/" + _RenewDealRoute.dealno + "/" + _RenewDealRoute.step;

            //Insert record to Campaign..LeasingContractRenew_Row_WsTafnit for log
            int index = DataProvider.InsertLeasingContractRenewUrlRequet(_configuration.GetValue<string>("ConnectionStrings:sqlInterHost") ?? ""
                        , ReqeustUrl
                        , _RenewDealRoute
                        , "IfAllowedSendMsgForIska");

            //call Tafnit Api 
            _IfAllowedSendMsgForIskaResponse = await IfAllowedSendMsgForIskaEndPoint(_configuration.GetValue<string>("ConnectionStrings:sqlInterHost") ?? ""
                    , ReqeustUrl
                    , index
                    , _RenewDealRoute
                    );

            return Ok(_IfAllowedSendMsgForIskaResponse);

        }
        [HttpGet("SetSignStepMasaMlkLeasing/{LogIdInforU}/{HpNo}/{DealNo}/{Step}/{LogIdLeasingContractRenew}")]
        public async Task<ActionResult> SetSignStepMasaMlkLeasing([FromRoute] tafnitRenewDealRoute _RenewDealRoute)

        {


            //from site,
            //http://localhost/journeyApp/api/tafnit/SetSignStepMasaMlkLeasing/970/34289595/3510056915/1/111
            //from program 
            //http://localhost:5279/api/tafnit/SetSignStepMasaMlkLeasing/970/34289595/3510056915/1/111


            //http://umi-appsites/journeyApp/api/tafnit/SetSignStepMasaMlkLeasing/970/34289595/3510056915/1/111


            SetSignStepMasaMlkLeasingResponse _SetSignStepMasaMlkLeasingResponse;

            //concat Api Url

            string ReqeustUrl = (_configuration.GetValue<string>("ApiTafnit:TafnitEndPoint") ?? "")
                         + "DivurMgr/SetSignStepMasaMlkLeasing/"
                         + _RenewDealRoute.hpno + "/" + _RenewDealRoute.dealno + "/" + _RenewDealRoute.step;

            //Insert record to Campaign..LeasingContractRenew_Row_WsTafnit for log
            int index = DataProvider.InsertLeasingContractRenewUrlRequet(_configuration.GetValue<string>("ConnectionStrings:sqlInterHost") ?? ""
                        , ReqeustUrl
                        , _RenewDealRoute
                        , "SetSignStepMasaMlkLeasing");

            //call Tafnit Api 
            _SetSignStepMasaMlkLeasingResponse = await SetSignStepMasaMlkLeasingEndPoint(_configuration.GetValue<string>("ConnectionStrings:sqlInterHost") ?? ""
                    , ReqeustUrl
                    , index
                    , _RenewDealRoute
                    );

            return Ok(_SetSignStepMasaMlkLeasingResponse);

        }
        


        #endregion


        #region function
        public async Task<IfAllowedSendMsgForIskaResponse> IfAllowedSendMsgForIskaEndPoint(string ConnectionString,
            string ReqeustUrl,int index,
          tafnitRenewDealRoute payload)
        {

            string jsonResponseContent = string.Empty;
            

            HttpResponseMessage response;
            IfAllowedSendMsgForIskaResponse? result = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    
                   response = await client.GetAsync(ReqeustUrl);

                    if (response.IsSuccessStatusCode)
                    {

                        jsonResponseContent = await response.Content.ReadAsStringAsync();
                        //convert string to object
                        result = JsonConvert.DeserializeObject<IfAllowedSendMsgForIskaResponse>(jsonResponseContent);
                    }
                    else
                        result = new IfAllowedSendMsgForIskaResponse()
                        {
                            Allowed = 0,
                            NotAllowedReason = "IsSuccessStatusCode is empty",
                            Success = 0,
                            Error = "IsSuccessStatusCode is empty"
                        };

                    


                    DataProvider.UpdateLeasingContractRenewResponse(ConnectionString
                             , index
                             , payload
                            , jsonResponseContent
                            , (int)response.StatusCode
                            , result.Success, result.Allowed, result.Error, result.NotAllowedReason
                            );
                }

            }
            catch (Exception ex)
            {
                result = new IfAllowedSendMsgForIskaResponse()
                {
                    Allowed = 0,
                    NotAllowedReason = ex.Message,
                    Success = 0,
                    Error = ex.Message
                };



            }


            return result;
        }
        public async Task<SetSignStepMasaMlkLeasingResponse> SetSignStepMasaMlkLeasingEndPoint(string ConnectionString,
            string ReqeustUrl, int index,
            tafnitRenewDealRoute payload)
        {

            string jsonResponseContent = string.Empty;


            HttpResponseMessage response;
            SetSignStepMasaMlkLeasingResponse? result = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {

                    response = await client.GetAsync(ReqeustUrl);

                    if (response.IsSuccessStatusCode)
                    {

                        jsonResponseContent = await response.Content.ReadAsStringAsync();
                        //convert string to object
                        result = JsonConvert.DeserializeObject<SetSignStepMasaMlkLeasingResponse>(jsonResponseContent);
                    }
                    else
                        result = new SetSignStepMasaMlkLeasingResponse()
                        {
                            Signed = 0,
                            NotSignedReason = "IsSuccessStatusCode is empty",
                            Success = 0,
                            Error = "IsSuccessStatusCode is empty"
                        };

                    DataProvider.UpdateLeasingContractRenewResponse(ConnectionString
                             , index
                             , payload
                            , jsonResponseContent
                            , (int)response.StatusCode
                            , result.Success, result.Signed, result.Error, result.NotSignedReason
                            );
                }

            }
            catch (Exception ex)
            {
                result = new SetSignStepMasaMlkLeasingResponse()
                {
                    Signed = 0,
                    NotSignedReason = ex.Message,
                    Success = 0,
                    Error = ex.Message
                };



            }


            return result;
        }
      
        #endregion




    }
}
