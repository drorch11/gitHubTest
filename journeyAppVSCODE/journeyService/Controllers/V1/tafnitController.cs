using journeyService.Contract;
using journeyService.Models.inforu;
using journeyService.Models.leasing;
using journeyService.Models.tafnit;
using journeyService.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace journeyService.Controllers.V1
{
    public class tafnitController: ControllerBase
    {
        private readonly IConfiguration _configuration;
        public tafnitController(IConfiguration configuration)
        {
            //by default IConfiguration is injected, not realy need to Inject
            _configuration = configuration;
        }
        [HttpGet(ApiRoutes.tafnitServ.RemoveDivurByPhoneOrEmail)]
        public async Task<ActionResult> RemoveDivurByPhoneOrEmail([FromRoute] int codeAction, 
            [FromRoute] string propertyType, [FromRoute] string propertyValue,
            [FromRoute] int logID_HubSpotImportLog, [FromRoute] string tafnitUnit)
        {
            //http://localhost:5279/api/V1/tafnitServ/RemoveDivurByPhoneOrEmail/3/Email/drorch11@gmail.com/100/tafnitAvis


            int logID = -1;

            tafnitRemoveDivurResponse? _tafnitRemoveDivurResponse = null;
            try
            {
              

                string ReqeustUrl = ((tafnitUnit == "tafnitAvis") ? (_configuration.GetValue<string>("ApiTafnit:TafnitEndPoint") ?? "") : (_configuration.GetValue<string>("ApiTafnit:TafnitUmiEndPoint") ?? ""))
                        + "DivurMgr/RemoveDivurByPhoneOrEmail"
                        + ((propertyType == "Email") ? "//" + propertyValue + "/0/DivurMgr" : "/" + propertyValue + "//0/DivurMgr");

                logID = DataProvider.InsertTafnitUnsubscribeRequet(
                     _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                     codeAction,
                     logID_HubSpotImportLog,
                     ReqeustUrl,
                     "Unsubscribe_" + propertyType,
                     propertyValue,
                     tafnitUnit
                );

                _tafnitRemoveDivurResponse = await tafnitRemoveDivurEndPoint(
                        _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                        , ReqeustUrl
                        , logID
                        );
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

            }

            return Ok(_tafnitRemoveDivurResponse);
        }



        public async Task<tafnitRemoveDivurResponse> tafnitRemoveDivurEndPoint(
            string ConnectionString,
            string ReqeustUrl, int logID)
        {

            string jsonResponseContent = string.Empty;


            HttpResponseMessage response;
            tafnitRemoveDivurResponse? result = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {

                    response = await client.GetAsync(ReqeustUrl);

                    jsonResponseContent = await response.Content.ReadAsStringAsync();
                    //convert string to object
                    result = JsonConvert.DeserializeObject<tafnitRemoveDivurResponse>(jsonResponseContent);

                    DataProvider.UpdateTafnitUnsubscribeResponse(
                       ConnectionString
                      , logID
                      , result
                      );






                }

            }
            catch (Exception ex)
            {
                result = new tafnitRemoveDivurResponse()
                {
                    StatusCode = -1,
                    StatusDesc = ex.Message
                    
                };
                DataProvider.UpdateTafnitUnsubscribeResponse(
                  _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                 , logID
                 , result
                 );



            }


            return result;
        }
    }
}
