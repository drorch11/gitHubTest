using journeyService.Models.hubspot.Api;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using DocumentFormat.OpenXml.Math;

namespace journeyService.Models.hubspot
{
    public class HubSpotValueToIdMapper
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly IConfiguration _configuration;

        public HubSpotValueToIdMapper(string accessToken, IConfiguration configuration)
        {
            _configuration = configuration;
            _accessToken = accessToken;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        }
        public async Task<Dictionary<string, string>> MapValueToIdsAsync(string url, List<UpDelAssociatedLabel> records,int logID
            ,string searchname, string fieldNameDB, string filterName, string Outputfield)
        {
            //StringComparer.OrdinalIgnoreCase : תשווה בין מחרוזות כאילו אותיות גדולות וקטנות הן זהות
            var valueToId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            HbSearchCustomObjectResponse result = new HbSearchCustomObjectResponse();

            //var values = records
            //    .Select(r => r.old_Email?.Trim().ToLower())
            //    .Where(e => !string.IsNullOrEmpty(e))
            //    .Distinct()
            //    .ToList();

            //fieldName = old_Email
            var values = records
                .Select(r => r.GetType().GetProperty(fieldNameDB)?.GetValue(r)?.ToString()?.Trim().ToLower())
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct()
                .ToList();

            //loop over email , Chunk : מחלק את קבוצת המיילים ל 100
            //יש הגבלה עד 100 פרטים ל hunspot במכה
            foreach (var batch in Chunk(values, 100))
            {
                var payload = new
                {
                    filterGroups = new[] {
                    new {
                        filters = new[] {
                            new {
                                propertyName = filterName, //"email"
                                @operator = "IN",
                                values = batch
                            }
                        }
                    }
                },
                    properties = new[] { Outputfield }, //"email"
                    limit = "100"
                };

                string serviceName = searchname switch
                {
                    "searchViaEmail" => "GetContactIDViaEmail",
                    "searchViaClientID" => "GetPersontIDViaClientID",
                    "searchViaCarNum" => "GetCarIDViaCarNum",
                    _ => ""
                };
                //

                int logIDRest = Utils.DataProvider.InsertHubspotRestRequet(
                 _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                 5,
                 logID,
                 url,
                 serviceName,
                 JsonSerializer.Serialize(payload),
                 "Umi",
                 ""
                );
                //searchViaClientID
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
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

                    return valueToId;
                }

             


                var json = await response.Content.ReadAsStringAsync();
                //convert json string to JsonDocument object
                //שימושי במיוחד כשמבנה ה-JSON גמיש או כשלא כל הערכים מעניינים אותך.
                var doc = JsonDocument.Parse(json);

                

                //EnumerateArray() – מאפשר לבצע foreach על כל איבר במערך
                foreach (var item in doc.RootElement.GetProperty("results").EnumerateArray())
                {
                    var email = item.GetProperty("properties").GetProperty(Outputfield).GetString(); //"email"
                    var id = item.GetProperty("id").GetString();
                    //ContainsKey - check not exists in Dictionary
                    if (!string.IsNullOrEmpty(email) && !valueToId.ContainsKey(email))
                    {
                        valueToId[email] = id;
                    }
                }

                result.accepted = true;
                result.statuscode = statusCodeInt;
                result.errormsg = "";
                result.total = batch.Count;

                Utils.DataProvider.UpdateHubspotRestResponse(
                   _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                  , logIDRest
                  , JsonSerializer.Serialize(doc)
                  , JsonSerializer.Serialize(valueToId)
                  , result.errormsg
                  , result.statuscode
                 );



            }
            //emailToId Dictionary <email,id>
            return valueToId;
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
