using journeyService.Contract;
using journeyService.Models.flycard;
using journeyService.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace journeyService.Controllers.V1
{
    public class flycardServController: ControllerBase
    {
        private readonly IConfiguration _configuration;
        public flycardServController(IConfiguration configuration)
        {
            //by default IConfiguration is injected, not realy need to Inject
            _configuration = configuration;
        }

        

        [HttpGet(ApiRoutes.flycardServ.GetpopulationForReminder)]
        public async Task<ActionResult> populationForReminder([FromRoute] int codeAction,
            [FromRoute] int maxDays, [FromRoute] int LogIdInforU)
        {

            //from program 
            //http://localhost:5279/api/V1/flycardServs/populationForReminder/57/3/84231
            //http://localhost/journeyApp/api/V1/flycardServs/populationForReminder/57/3/84231
            //http://umi-appsites/journeyApp/api/V1/flycardServs/populationForReminder/57/3/84231




            //Token Api Url
            string ReqeustUrl = (_configuration.GetValue<string>("ApiflyCard:flyCardEndPoint") ?? "")
                         + "auth/site-token";



            //token payload
            tokenRequest payload = new tokenRequest();
            payload.email = (_configuration.GetValue<string>("ApiflyCard:emailToken") ?? "");
            payload.password = (_configuration.GetValue<string>("ApiflyCard:passwordToken") ?? "");

            tokenResponse _tokenResponse;

            //get token value
            _tokenResponse = await getToken(
                (_configuration.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "")
                , LogIdInforU
                , ReqeustUrl
                , payload
                  );


            populationForReminderResponse _populationForReminderResponse = new populationForReminderResponse();

            //token is valid            
            if (_tokenResponse.token != "N/A")
            {



                ReqeustUrl = (_configuration.GetValue<string>("ApiflyCard:flyCardEndPoint") ?? "")
                         + "external/outgoingReservations/populationForReminder?maxDays=" + maxDays;

                _populationForReminderResponse = await callpopulationForReminderEndPoint(
                    (_configuration.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "")
                    , LogIdInforU
                    , ReqeustUrl
                    , _tokenResponse

                    );
            }

            return Ok(_populationForReminderResponse);

        }


        [HttpGet(ApiRoutes.flycardServ.GetpopulationForPaymentReminder)]
        public async Task<ActionResult> populationForPaymentReminder([FromRoute] int codeAction,
           [FromRoute] int maxDays, [FromRoute] int LogIdInforU)
        {

            //http://localhost:5279/api/V1/flycardServs/populationForPaymentReminder/58/10/84231
            //http://localhost/journeyApp/api/V1/flycardServs/populationForPaymentReminder/58/10/84231
            //http://umi-appsites/journeyApp/api/V1/flycardServs/populationForPaymentReminder/58/10/84231




            //Token Api Url
            string ReqeustUrl = (_configuration.GetValue<string>("ApiflyCard:flyCardEndPoint") ?? "")
                         + "auth/site-token";



            //token payload
            tokenRequest payload = new tokenRequest();
            payload.email = (_configuration.GetValue<string>("ApiflyCard:emailToken") ?? "");
            payload.password = (_configuration.GetValue<string>("ApiflyCard:passwordToken") ?? "");

            tokenResponse _tokenResponse;

            //get token value
            _tokenResponse = await getToken(
                (_configuration.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "")
                , LogIdInforU
                , ReqeustUrl
                , payload
                  );


            populationForPaymentReminderResponse _populationForPaymentReminderResponse = new populationForPaymentReminderResponse();

            //token is valid            
            if (_tokenResponse.token != "N/A")
            {



                ReqeustUrl = (_configuration.GetValue<string>("ApiflyCard:flyCardEndPoint") ?? "")
                         + "external/outgoingReservations/populationForPaymentReminder?maxDays=" + maxDays;

                _populationForPaymentReminderResponse = await callpopulationForPaymentReminderEndPoint(
                    (_configuration.GetValue<string>("ConnectionStrings:sqlAvisDataBIMrr") ?? "")
                    , LogIdInforU
                    , ReqeustUrl
                    , _tokenResponse

                    );
            }

            return Ok(_populationForPaymentReminderResponse);

        }
        #region end point flycard token
        public async Task<tokenResponse> getToken(string ConnectionString,
           int logID,
           string ReqeustUrl,
           tokenRequest payload
           )
        {
            //convert object to string
            string jsonData = JsonConvert.SerializeObject(payload);

            string jsonResponseContent = string.Empty;




            HttpResponseMessage response;
            tokenResponse? result = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //content.Headers.Add("x-howazit-token", _config.GetValue<string>("ApiEndPoint:accessToken") ?? "");


                    DataProvider.UpdateinforULogFlycardRequestToken(ConnectionString, logID,
                        payload, jsonData);

                    response = await client.PostAsync(ReqeustUrl, content);


                    if (response.IsSuccessStatusCode)
                    {

                        jsonResponseContent = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<tokenResponse>(jsonResponseContent);
                    }
                    else
                        result = new tokenResponse()
                        {
                            token = "N/A",
                            message = "error from token"
                        };

                    DataProvider.UpdateinforULogFlycardResponseToken(ConnectionString, logID
                            , jsonResponseContent
                            , (int)response.StatusCode
                            , response.IsSuccessStatusCode ? "" : result.message
                            );
                }
            }
            catch (Exception ex)
            {
                result = new tokenResponse()
                {
                    token = "N/A",
                    message = ex.Message
                };

            }


            return result;
        }
        #endregion

        #region end point flycard populationForReminder
        public async Task<populationForReminderResponse> callpopulationForReminderEndPoint(string ConnectionString,
           int logID,
           string ReqeustUrl,
           tokenResponse payload
           )
        {
            string jsonResponseContent = string.Empty;

            HttpResponseMessage response;
            populationForReminderResponse? result = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Add("Authorization", "SiteCustomToken" + payload.token);
                    response = await client.GetAsync(ReqeustUrl);


                    if (response.IsSuccessStatusCode)
                    {
                        jsonResponseContent = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<populationForReminderResponse>(jsonResponseContent);
                    }
                    else
                        result = new populationForReminderResponse()
                        {
                            message = "error from endpoint"
                        };
                    DataProvider.UpdateinforULogFlycardReminderResponse(ConnectionString, logID
                            , jsonResponseContent
                            , (int)response.StatusCode
                            , result



                            );
                }
            }
            catch (Exception ex)
            {
                result = new populationForReminderResponse()
                {
                    message = ex.Message
                };

            }
            return result;
        }
        #endregion

        #region end point flycard populationForPaymentReminder
        public async Task<populationForPaymentReminderResponse> callpopulationForPaymentReminderEndPoint(string ConnectionString,
           int logID,
           string ReqeustUrl,
           tokenResponse payload
           )
        {
            string jsonResponseContent = string.Empty;

            HttpResponseMessage response;
            populationForPaymentReminderResponse? result = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Add("Authorization", "SiteCustomToken" + payload.token);
                    response = await client.GetAsync(ReqeustUrl);


                    if (response.IsSuccessStatusCode)
                    {
                        jsonResponseContent = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<populationForPaymentReminderResponse>(jsonResponseContent);
                    }
                    else
                        result = new populationForPaymentReminderResponse()
                        {
                            message = "error from endpoint"
                        };
                    DataProvider.UpdateinforULogFlycardReminderPaymentResponse(ConnectionString, logID
                            , jsonResponseContent
                            , (int)response.StatusCode
                            , result



                            );
                }
            }
            catch (Exception ex)
            {
                result = new populationForPaymentReminderResponse()
                {
                    message = ex.Message
                };

            }
            return result;
        }
        #endregion


    }
}
