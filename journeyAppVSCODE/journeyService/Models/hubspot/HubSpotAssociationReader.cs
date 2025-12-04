using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using journeyService.Models.hubspot.Api;
using Microsoft.Extensions.Configuration;
using journeyService.Utils;

namespace journeyService.Models.hubspot
{
    public class HubSpotAssociationReader
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        //private readonly string _objectType; // דוג': "2-124289527"
        private readonly IConfiguration _configuration;
        public HubSpotAssociationReader(string accessToken, IConfiguration configuration)
        {
            _configuration = configuration;
            _accessToken = accessToken;
            //_objectType = objectType;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }
        public async Task<List<(string fromID, string toID)>> GetFromIDToIDHasAssociated(string url, List<string> fromIDs,int logID)
        {
            HbSearchCustomObjectResponse result = new HbSearchCustomObjectResponse();
            var results = new List<(string fromID, string toID)>();
            


            foreach (var batch in Chunk(fromIDs, 100))
            {

                var payload = new
                {
                    inputs = batch.Select(id => new { id }).ToList()
                };

               


                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                int logIDRest = Utils.DataProvider.InsertHubspotRestRequet(
                     _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                     5,
                     logID,
                     url,
                     "GetIDsOfAssocaiteContact",
                     JsonSerializer.Serialize(payload),
                     "Umi",
                     ""
                 );

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

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
                    return results;
                }

              


                var json = await response.Content.ReadAsStringAsync();
                //convert json string to JsonDocument object
                //שימושי במיוחד כשמבנה ה-JSON גמיש או כשלא כל הערכים מעניינים אותך.
                var doc = JsonDocument.Parse(json);

              

                foreach (var item in doc.RootElement.GetProperty("results").EnumerateArray())
                {
                    var fromId = item.GetProperty("from").GetProperty("id").ToString();

                    foreach (var to in item.GetProperty("to").EnumerateArray())
                    {
                        // בדיקה אם יש toObjectId
                        if (to.TryGetProperty("toObjectId", out var toObjectIdElem))
                        {
                            var toObjectId = toObjectIdElem.ToString();

                            // סינון לפי associationTypeId == 73
                            if (to.TryGetProperty("associationTypes", out var assocTypes))
                            {

                                bool hasAssocaitedContact = assocTypes.EnumerateArray().Any(t =>
                                      t.TryGetProperty("typeId", out var typeIdElem) &&
                                      t.TryGetProperty("category", out var categoryElem) &&
                                      (
                                          typeIdElem.GetInt32() == (int)associationTypeId.contact ||
                                          typeIdElem.GetInt32() == (int)associationTypeId.contact_certified ||
                                          typeIdElem.GetInt32() == (int)associationTypeId.contact_danlis 
                                      ) &&
                                      categoryElem.GetString() == "USER_DEFINED"
                                    );


                                if (hasAssocaitedContact)
                                    {
                                        results.Add((fromId, toObjectId));
                                    }
                                
                              

                            }
                        }
                    }
                }
                
                
                
                var resultsAsObjects = results.Select(r => new { r.fromID, r.toID }).ToList();

              


                result.accepted = true;
                result.statuscode = statusCodeInt;
                result.errormsg = "";
                result.total = batch.Count;

                Utils.DataProvider.UpdateHubspotRestResponse(
                   _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                  , logIDRest
                  , JsonSerializer.Serialize(doc)
                  , JsonSerializer.Serialize(resultsAsObjects)
                  , result.errormsg
                  , result.statuscode
                 );

            }

            return results;
        }
        public async Task<List<(string fromID, string toID)>> GetFromIDToIDHasAssociated(string url, List<string> fromIDs, int logID,
            Dictionary<string, string> clientIdToFromId, Dictionary<string, string> carnumToToId)
        {
            var results = new List<(string fromID, string toID)>();
            HbSearchCustomObjectResponse result = new HbSearchCustomObjectResponse();

            foreach (var batch in Chunk(fromIDs, 100))
            {

                var payload = new
                {
                    inputs = batch.Select(id => new { id }).ToList()
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                int logIDRest = Utils.DataProvider.InsertHubspotRestRequet(
                   _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                   5,
                   logID,
                   url,
                   "GetIDsOfAssocaitePerson",
                   JsonSerializer.Serialize(payload),
                   "Umi",
                   ""
               );


                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();


                int statusCodeInt = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    result.accepted = false;
                    result.statuscode = statusCodeInt;
                    result.total = 0;
                    result.errormsg = $"Search failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";

                    Utils.DataProvider.UpdateHubspotRestResponse(
                           _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                          , logIDRest
                          , result
                          , ""
                         );

                    return results;
                }



               


                var json = await response.Content.ReadAsStringAsync();
                //convert json string to JsonDocument object
                //שימושי במיוחד כשמבנה ה-JSON גמיש או כשלא כל הערכים מעניינים אותך.
                var doc = JsonDocument.Parse(json);






                foreach (var item in doc.RootElement.GetProperty("results").EnumerateArray())
                {
                    var fromId = item.GetProperty("from").GetProperty("id").ToString(); //personid

                    foreach (var to in item.GetProperty("to").EnumerateArray())
                    {
                        if (to.TryGetProperty("toObjectId", out var toObjectIdElem))
                        {
                            var toObjectId = toObjectIdElem.ToString(); //carid

                            // חפש את id המתאים ל-from.id
                            //clientIdToFromId = Dictionary < clientid, id >
                            var clientMatch = clientIdToFromId.FirstOrDefault(x => x.Value == fromId);
                            //save clientid
                            var clientId = clientMatch.Key;

                            // חפש את ID-toObjectId
                            //carnumToToId = Dictionary <carnum,id>
                            var carMatch = carnumToToId.FirstOrDefault(x => x.Value == toObjectId);
                            //save carnum
                            var carNum = carMatch.Key;

                            
                            if (clientId != null && carNum != null)
                            {
                                //Console.WriteLine($"{clientId} - {carNum} - {fromId} - {toObjectId}");

                                // בדוק אם קיימות אסוציאציות מסוג car_owner או car_holder
                                if (to.TryGetProperty("associationTypes", out var assocTypes))
                                {
                                    bool hasAssocaitedPerson = assocTypes.EnumerateArray().Any(t =>
                                        t.TryGetProperty("typeId", out var typeIdElem) &&
                                        t.TryGetProperty("category", out var categoryElem) &&
                                        (
                                            typeIdElem.GetInt32() == (int)associationTypeId.car_buyer ||
                                            typeIdElem.GetInt32() == (int)associationTypeId.car_owner ||
                                            typeIdElem.GetInt32() == (int)associationTypeId.car_buyer_danlis ||
                                            typeIdElem.GetInt32() == (int)associationTypeId.car_owner_danlis ||
                                            typeIdElem.GetInt32() == (int)associationTypeId.car_buyer_certified ||
                                            typeIdElem.GetInt32() == (int)associationTypeId.car_owner_certified
                                        ) &&
                                        categoryElem.GetString() == "USER_DEFINED"
                                    );

                                    if (hasAssocaitedPerson)
                                    {
                                        results.Add((fromId, toObjectId));
                                        
                                    }
                                }
                            }
                        }
                    }
                }

                var resultsAsObjects = results.Select(r => new { r.fromID, r.toID }).ToList();

                result.accepted = true;
                result.statuscode = statusCodeInt;
                result.errormsg = "";
                result.total = batch.Count;

                Utils.DataProvider.UpdateHubspotRestResponse(
                   _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                  , logIDRest
                  , JsonSerializer.Serialize(doc)
                  , JsonSerializer.Serialize(resultsAsObjects)
                  , result.errormsg
                  , result.statuscode
                 );
            }

            return results;
        }

        private static IEnumerable<List<T>> Chunk<T>(List<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
            {
                yield return source.GetRange(i, Math.Min(size, source.Count - i));
            }
        }
    }
}
