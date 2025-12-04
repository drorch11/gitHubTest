using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Spreadsheet;
using journeyService.Contract;
using journeyService.Models.howazit;
using journeyService.Models.hubspot;
using journeyService.Models.hubspot.Api;
using journeyService.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace journeyService.Controllers.V1
{
    public class hubspotController: ControllerBase
    {
        private readonly IConfiguration _configuration;
        public hubspotController(IConfiguration configuration)
        {
            //by default IConfiguration is injected, not realy need to Inject
            _configuration = configuration;
        }

        

        

        
        public async Task<HbSearchCustomObjectResponse> HbSearchCustomEndPoint(string baseUrl, string token, 
                string url,string payload)
        {
            HbSearchCustomObjectResponse result = new HbSearchCustomObjectResponse();
            int logID;
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    // הגדרת Authorization על הלקוח כולו
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    //"objects/contacts/search"
                    // בקשת חיפוש
                    var request = new HttpRequestMessage(HttpMethod.Post, baseUrl + url);
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                   

                    var response = await client.SendAsync(request);

                    int statusCodeInt = (int)response.StatusCode;

                    if (!response.IsSuccessStatusCode)
                    {
                        result.accepted = false;
                        result.statuscode = statusCodeInt;
                        result.total = 0;
                        result.errormsg = $"Search failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
                        return result;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<HbSearchCustomObjectResponse>(json) ?? new HbSearchCustomObjectResponse();
                    result.accepted = true;
                    result.statuscode = statusCodeInt;
                    result.total = result.total;



                    // Delete all contact
                    //foreach (var contact in result.results)
                    //{
                    //    string contactId = contact.id;
                    //    string deleteUrl = baseUrl + $"objects/contacts/{contactId}";

                    //    DataProvider.UpdateHubSpotRequestSecondTime(
                    //          _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                    //        , logID
                    //        , deleteUrl
                    //        ,"delete contact"
                    //        );

                    //    HttpResponseMessage deleteResponse = await client.DeleteAsync(deleteUrl);
                    //    int statusCodeInt = (int)deleteResponse.StatusCode;

                    //    if (!deleteResponse.IsSuccessStatusCode)
                    //    {

                    //        string error = await deleteResponse.Content.ReadAsStringAsync();

                    //        DataProvider.UpdateHubSpottResponseSecondTime(
                    //         _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                    //        , logID
                    //        , statusCodeInt
                    //        , $"Failed to delete contact {contactId}: {deleteResponse.StatusCode} - {error}"
                    //        );

                    //    }
                    //    else
                    //    {
                    //        DataProvider.UpdateHubSpottResponseSecondTime(
                    //         _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                    //        ,logID
                    //        ,statusCodeInt
                    //        ,$"Deleted contact {contactId}"
                    //        );

                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                result.accepted = false;
                result.statuscode = -1;
                result.total = 0;
                result.errormsg = $"Exception: {ex.Message}";
            }

            return result;
        }



        #region  function
        public List<HbSearchCustomObject> getBodyJsonRequest(List<HbSearchfilters> _HbSearchfilters)
        {
            List<HbSearchCustomObject> _HbSearchCustomObjects = new List<HbSearchCustomObject>();

            foreach (HbSearchfilters filter in _HbSearchfilters)
            {
                _HbSearchCustomObjects.Add(
                     new HbSearchCustomObject(
                         new List<Hbfilters>()
                         {
                            new Hbfilters
                            {
                                propertyName = filter.propertyName,
                                @operator = filter.@operator,
                                value = filter.value
                            }
                         }
                     )
                 );


            }

        

            return _HbSearchCustomObjects;
        }

        public List<HbSearchfilters> fillSearchfilters(int codeAction,string _propertyName,string _operator,string _paramvalue)
        {
            List<HbSearchfilters> _HbSearchfilters = new List<HbSearchfilters>();

            _HbSearchfilters.Add(new HbSearchfilters()
            {
                propertyName = _propertyName,
                @operator = _operator,
                value = _paramvalue
            });

            return _HbSearchfilters;
        }

        public List<HbUpsertAssociated> fillUpsertAssociated(int codeAction, string _associationCategory, int _associationTypeId, string _paramvalueFromObj, string _paramvalueToObj)
        {
            List<typesAssocaited> _typesAssocaited = new List<typesAssocaited>();
            _typesAssocaited.Add(new typesAssocaited()
            {
                associationTypeId = _associationTypeId,
                associationCategory = _associationCategory
            });

            fromObject _fromObject = new fromObject();
            _fromObject.id = _paramvalueFromObj;

            toObject _toObject = new toObject();
            _toObject.id = _paramvalueToObj;


            List<HbUpsertAssociated> _HbUpsertAssociated = new List<HbUpsertAssociated>()
            {
                new HbUpsertAssociated(_typesAssocaited, _fromObject, _toObject) 
            };

            return _HbUpsertAssociated;

        }
        #endregion



    }
}
