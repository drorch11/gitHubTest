using journeyService.Models.hubspot.Api;
using journeyService.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace journeyService.Models.hubspot
{
    public class HubSpotAssociationDelete
    {

        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly IConfiguration _configuration;

        public HubSpotAssociationDelete(string accessToken,IConfiguration configuration)
        {
            
            _httpClient = new HttpClient();
            _accessToken = accessToken;
            _configuration = configuration;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task DelAssociationsAsyncVer2(List<(string fromID, string toID)> associations,
                string searchBy, int logID, string url)
        {

            const int BatchSize = 100;
            var types = searchBy switch
            {
                "DeleteContactAssociated" => new[] { associationTypeId.contact, associationTypeId.contact_certified, associationTypeId.contact_danlis },
                "DeletePersonAssociated" => new[] { associationTypeId.car_owner_certified, associationTypeId.car_owner_danlis, associationTypeId.car_buyer_certified, associationTypeId.car_buyer_danlis, associationTypeId.car_owner, associationTypeId.car_buyer },
                _ => new[] { associationTypeId.contact }
            };
            foreach (var batch in associations.Chunk(BatchSize))
            {
                await DelAssociationsBatchAsyncVer2(batch.ToList(), types, searchBy, logID, url);
            }
        }
        public async Task DelAssociationsBatchAsyncVer2(List<(string fromID, string toID)> batch,
            IEnumerable<associationTypeId> associationTypes,
            string searchBy, int logID, string url)
        {
            HbSearchCustomObjectResponse result = new HbSearchCustomObjectResponse();

            var inputs = batch.Select(pair => new
            {
                types = associationTypes.Select(type => new
                {
                    associationCategory = "USER_DEFINED",
                    associationTypeId = (int)type
                }).ToArray(),

                from = new { id = pair.fromID },
                to = new { id = pair.toID }
            });

            var payload = new { inputs };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            int logIDRest = Utils.DataProvider.InsertHubspotRestRequet(
                _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                5,
                logID,
                url,
                searchBy, //this is serviceName in DB
                JsonSerializer.Serialize(payload),
                "Umi",
                ""
            );

            var response = await _httpClient.PostAsync(url, content);
            int statusCodeInt = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                result.accepted = false;
                result.statuscode = statusCodeInt;
                result.total = 0;
                result.errormsg = $"Search failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";

                DataProvider.UpdateHubspotRestResponse(
                       _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                      , logIDRest
                      , result
                      , ""
                     );
                return;
            }

            result.accepted = true;
            result.statuscode = statusCodeInt;
            result.errormsg = "";
            result.total = batch.Count;

            //Hebrew encoding
            var batchjson = JsonSerializer.Serialize(batch, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // יאפשר עברית בלי קידוד
                WriteIndented = true // אופציונלי — בשביל יופי
            });


            DataProvider.UpdateHubspotRestResponse(
                     _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                    , logIDRest
                    , result
                    , batchjson
                   );


        }
        //public async Task ArchiveRecord(List<(string KeyToArchive, string ObjectNameToArchive)> ArchiveList)
        //{


        //    const int BatchSize = 100;



        //    foreach (var row in ArchiveList.Chunk(BatchSize)) // .NET 6+
        //    {
        //        var inputs = row.Select(pair => new
        //        {
        //            id = pair.KeyToArchive 
        //        }).ToList();

        //    }

            

        //    //https://api.hubapi.com/crm/v3/objects/2-130920180/batch/archive
        //}


    }
}
