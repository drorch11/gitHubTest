using HubSpotControl.Contract;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Reflection.Emit;
using System.Data;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Http;
using System;

namespace HubSpotControl.Controllers.V1
{
    public class AssocaitedServControl: ControllerBase
    {
        private readonly IConfiguration _configuration;
        const string HUBSPOT_TOKEN = "pat-eu1-88c90f6c-98e7-45f0-858b-de6e0c9ec975"; // או מקובץ env/Secret Manager


        public AssocaitedServControl(IConfiguration configuration)
        {
            
            //by default IConfiguration is injected, not realy need to Inject
             _configuration = configuration;
            
        }
        [HttpGet(ApiRoutes.AssocaitedServ.ArchiveInActiveData)]
        public async Task<ActionResult> ArchiveInActiveDataList([FromRoute] string from_object)
        {

            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetValue<string>("ApiHubSpot:HubSpotToken") ?? "");


            //http://localhost:5166/api/V1/AssocaitedServs/ArchiveInActiveDataList/contacts
            //http://localhost:5166/api/V1/AssocaitedServs/ArchiveInActiveDataList/2-130920180
            //http://localhost:5166/api/V1/AssocaitedServs/ArchiveInActiveDataList/2-124289527
            var dt = await LoadContactVsCarAsync(
                  _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                  @"exec [HavayatLakoah].[ArchiveInActiveDataList]"

              );

            var data = dt.AsEnumerable()
              .Select(r => new
              {
                  id = r["PrimaryKey"]?.ToString() ?? ""
              })
              .Where(x => !string.IsNullOrEmpty(x.id)) // סינון רשומות ריקות
              .ToList();

            const int BatchSize = 100;



            foreach (var batch in data.Chunk(BatchSize)) // .NET 6+
            {

                var inputs = batch.Select(pair => new
                {
                    id = pair.id // או pair.CarKey, תלוי מה אתה צריך
                }).ToList();


                var payload = new
                {
                    inputs = inputs
                };


                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //delete assocaited
                
                var response = await http.PostAsync($"{_configuration.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v3/objects/{from_object}/batch/archive", content);
                
                int statusCodeInt = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                }


            }

            return Ok();



        }
        [HttpGet(ApiRoutes.AssocaitedServ.AssociatedDeleteAllFromContact)]
               
        public async Task<ActionResult> AssociatedDeleteAllFromContactList([FromRoute] string from_object, [FromRoute] string to_object)
        {

            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetValue<string>("ApiHubSpot:HubSpotToken") ?? "");


            //http://localhost:5166/api/V1/AssocaitedServs/AssociatedDeleteAllFromContactList
            var dt = await LoadContactVsCarAsync(
                  _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                  @"exec [HavayatLakoah].[AssociatedDeleteAllFromContact]"

              );

            var associations = dt.AsEnumerable()
                .Select(r => new
                {
                    ContactKey = r["ContactKey"]?.ToString() ?? "",
                    CarKey = r["CarKey"]?.ToString() ?? "",
                    TypeId = r["typeId"]?.ToString() ?? ""
                })
                .ToList();

            const int BatchSize = 100;



            foreach (var batch in associations.Chunk(BatchSize)) // .NET 6+
            {
                var inputs = batch.Select(pair => new
                {
                    types = new[]
                       {
                            new
                            {
                                associationCategory = "USER_DEFINED",
                                associationTypeId = pair.TypeId // <-- כאן השינוי
                            }
                        },
                    from = new { id = pair.ContactKey },
                    to = new { id = pair.CarKey }
                }).ToList();

                var payload = new { inputs };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //delete assocaited

                var response = await http.PostAsync($"{_configuration.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v4/associations/contacts/{HubspotObject.Car}/batch/labels/archive", content);


                int statusCodeInt = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                }


            }

            return Ok();



        }
        [HttpGet(ApiRoutes.AssocaitedServ.MissMatchContact)]
        public async Task<ActionResult> MissMatchContactList([FromRoute] string from_object, [FromRoute] string to_object)
        {

            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetValue<string>("ApiHubSpot:HubSpotToken") ?? "");


            //http://localhost:5166/api/V1/AssocaitedServs/MissMatchContactList
            var dt = await LoadContactVsCarAsync(
                  _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                  @"exec [HavayatLakoah].[AssociatedMissMatchContact]"

              );

            var associations = dt.AsEnumerable()
                .Select(r => new
                {
                    ContactKey = r["ContactKey"]?.ToString() ?? "",
                    CarKey = r["CarKey"]?.ToString() ?? "",
                    DeleteTypeId = r["DeletetypeId"]?.ToString() ?? "",
                    NewTypeId = r["NewtypeId"]?.ToString() ?? ""
                })
                .ToList();
            
            const int BatchSize = 100;


            object? payload = null;
            string? json = null;
            StringContent? content = null;
            HttpResponseMessage? response = null;
            int statusCodeInt = 0;
            List<object> inputs;

            foreach (var batch in associations.Chunk(BatchSize)) // .NET 6+
            {
                inputs = batch.Select(pair => new
                {
                    types = new[]
                 {
                    new
                    {
                        associationCategory = "USER_DEFINED",
                        associationTypeId = pair.DeleteTypeId // מחיקת סוג קיים
                    }
                },
                            from = new { id = pair.ContactKey },
                            to = new { id = pair.CarKey }
                        }).ToList<object>();

                payload = new { inputs };

                json = JsonSerializer.Serialize(payload);
                content = new StringContent(json, Encoding.UTF8, "application/json");
                //delete assocaited

                response = await http.PostAsync($"{_configuration.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v4/associations/contacts/{HubspotObject.Car}/batch/labels/archive", content);


                statusCodeInt = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                }


                inputs = batch.Select(pair => new
                {
                            types = new[]
                 {
                    new
                    {
                        associationCategory = "USER_DEFINED",
                        associationTypeId = pair.NewTypeId // יצירת סוג חדש
                    }
                },
                            from = new { id = pair.ContactKey },
                            to = new { id = pair.CarKey }
                        }).ToList<object>();

                payload = new { inputs };

                json = JsonSerializer.Serialize(payload);
                content = new StringContent(json, Encoding.UTF8, "application/json");
                //delete assocaited

                response = await http.PostAsync($"{_configuration.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v4/associations/contacts/{HubspotObject.Car}/batch/create", content);

                statusCodeInt = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                }



            }

            return Ok();



        }
        [HttpGet(ApiRoutes.AssocaitedServ.DeleteDuplicateContact)]
        public async Task<ActionResult> DeleteDuplicateContactList([FromRoute] string from_object, [FromRoute] string to_object)
        {

            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetValue<string>("ApiHubSpot:HubSpotToken") ?? "");


            //http://localhost:5166/api/V1/AssocaitedServs/DeleteDuplicateContactList
            var dt = await LoadContactVsCarAsync(
                  _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                  @"exec [HavayatLakoah].[AssociatedDuplicateContact]"

              );

            var associations = dt.AsEnumerable()
                .Select(r => new
                {
                    ContactKey = r["ContactKey"]?.ToString() ?? "",
                    CarKey = r["CarKey"]?.ToString() ?? "",
                    TypeId = r["typeId"]?.ToString() ?? ""
                })
                .ToList();

            const int BatchSize = 100;

         

            foreach (var batch in associations.Chunk(BatchSize)) // .NET 6+
            {
                var inputs = batch.Select(pair => new
                {
                    types = new[]
                       {
                            new
                            {
                                associationCategory = "USER_DEFINED",
                                associationTypeId = pair.TypeId // <-- כאן השינוי
                            }
                        },
                    from = new { id = pair.ContactKey },
                    to = new { id = pair.CarKey }
                 }).ToList();

                var payload = new { inputs };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //delete assocaited

                var response = await http.PostAsync($"{_configuration.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v4/associations/contacts/{HubspotObject.Car}/batch/labels/archive", content);
                

                int statusCodeInt = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                }


            }

            return Ok();



        }
        [HttpGet(ApiRoutes.AssocaitedServ.ReadData)]
        public async Task<ActionResult> ReadDataList([FromRoute] string from_object, [FromRoute] string to_object)
        {

            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HUBSPOT_TOKEN);


            //http://localhost:5166/api/V1/AssocaitedServs/ReadDataList
            var dt = await LoadContactVsCarAsync(
                  _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                  @"SELECT  [ContactKey], [CarKey] FROM [HavayatLakoah].[hubspotContactVsCarTest];"
              );

            var associations = dt.AsEnumerable()
                .Select(r => new
                {
                    ContactKey = r["ContactKey"]?.ToString() ?? "",
                    CarKey = r["CarKey"]?.ToString() ?? ""
                })
                .ToList();

            const int BatchSize = 100;

            // נבנה את אובייקטי ה-type פעם אחת מחוץ ללולאה
            //associationTypeId = 73 איש קשר
            var typeObjs = new[]
            {
                 new { associationCategory = "USER_DEFINED", associationTypeId = 94 }
            };

            //var typeObjs = new[]
            //{
            //     new { associationCategory = "USER_DEFINED", associationTypeId = 92 }
            //};

            foreach (var batch in associations.Chunk(BatchSize)) // .NET 6+
            {
                var inputs = batch.Select(pair => new
                {
                    types = typeObjs,
                    from = new { id = pair.ContactKey }, // <-- התיקון פה
                    to = new { id = pair.CarKey }
                }).ToList();

                var payload = new { inputs };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //delete assocaited
                //var response = await http.PostAsync($"https://api.hubspot.com/crm/v4/associations/contacts/2-124289527/batch/labels/archive", content);
                var response = await http.PostAsync($"https://api.hubspot.com/crm/v4/associations/2-130920180/2-124289527/batch/labels/archive", content);

                //create assocaited
                //var response = await http.PostAsync($"https://api.hubspot.com/crm/v4/associations/2-130920180/2-124289527/batch/create", content);


                int statusCodeInt = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                }


                }

            return Ok();



        }
        /// <summary> 
        /// Read All Data from HuubSPot 
        /// </summary>
        /// <param name="from_object"></param>
        /// <param name="to_object"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.AssocaitedServ.GetAllAssociationsBetweenObject)]
        public async Task<ActionResult> GetAssociationsList([FromRoute] string from_object, [FromRoute] string to_object)
        {
            //http://localhost:5166/api/V1/AssocaitedServs/GetAssociationsList/contacts/2-124289527 --contact
            //http://localhost:5166/api/V1/AssocaitedServs/GetAssociationsList/2-130920180/2-124289527 -- person
            //http://localhost:5166/api/V1/AssocaitedServs/GetAssociationsList/2-124289527/2-124289527 -- car
            long? typeId = null;
            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HUBSPOT_TOKEN);

            List<(string Id, string? Value, string? Value1)> ObjectDataList = new();
            
            List<(string Id, string? newregistrationplate, string? ops__tafnit_unit, string? kodmachzormechira)> carDataList = new();
            // 1) get all IDS from contacts or Person , limit 100
            if (from_object == "contacts")
            {
                ObjectDataList = await GetAllIdsAsync(http, from_object, 100, "hs_object_id,email,contact_type");
                await InsertContactIdsAsync(
                        ObjectDataList,
                        _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                    );
            }
                
            else if (from_object == "2-130920180")
            {
                ObjectDataList = await GetAllIdsAsync(http, from_object, 100, "hs_object_id,client_id");
                await InsertPersonIdsAsync(
                        ObjectDataList,
                        _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                    );
            }
            else if (from_object == "2-124289527")
            {
                carDataList = await GetAllCarsAsync(http, from_object, 100, "hs_object_id,newregistrationplate,ops__tafnit_unit,kodmachzormechira");
                await InsertCarsIdsAsync(
                        carDataList,
                        _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                    );

                return Ok();

            }








                //get only ids
            var Ids = ObjectDataList.Select(x => x.Id).ToList();
            
            var matched = new List<(string FromObjectId, string ToObjectId, long TypeId, string Label)>();

            foreach (var group in Chunk(Ids, 1000))
            {
                var res = await ReadAssociationsBatchAsync(http, from_object, to_object, group);

                if (res?.Results == null) continue;

                foreach (var row in res.Results)
                {
                    var toItems = row.To ?? new List<AssocToItem>();

                    foreach (var t in toItems)
                    {
                        var assocTypes = t.AssociationTypes ?? new List<AssocType>();

                        foreach (var assoc in assocTypes)
                        {
                            // אם נשלח typeId מסוים, סנן רק אותו
                            if (typeId is null || assoc.TypeId == typeId)
                            {
                                matched.Add((
                                    FromObjectId: row.From.Id,
                                    ToObjectId: t.ToObjectId.ToString(),
                                    TypeId: assoc.TypeId,
                                    Label: assoc.Label ?? string.Empty
                                ));
                            }
                        }
                    }
                }
            }
            
            
            //save all contact                
            await InsertObjectVsCarAssocaited(
                matched,
                _configuration.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                from_object
            );

            return Ok();
            


        }
        
        static async Task<List<(string Id, string? newregistrationplate, string? ops__tafnit_unit, string? kodmachzormechira)>> GetAllCarsAsync(HttpClient http, string from_object, int limit, string flields)
        {
            var data = new List<(string Id, string? newregistrationplate, string? ops__tafnit_unit, string? kodmachzormechira)>();
            string? after = null;

            do
            {
                var url = new StringBuilder(
                    $"https://api.hubapi.com/crm/v3/objects/{from_object}?limit={limit}&properties={flields}&archived=false"
                );
                if (!string.IsNullOrEmpty(after))
                    url.Append("&after=").Append(Uri.EscapeDataString(after));

                using var resp = await http.GetAsync(url.ToString());
                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync();
                var page = JsonSerializer.Deserialize<ContactsPage>(json);

                if (page?.Results != null)
                {
                    foreach (var r in page.Results)
                    {
                            data.Add((r.Id, r.Properties?.newregistrationplate, r.Properties?.ops__tafnit_unit, r.Properties?.kodmachzormechira));

                    }


                }
                
                after = page?.Paging?.Next?.After;
            }
            while (!string.IsNullOrEmpty(after));

            return data;

        }
        static async Task<List<(string Id, string? Email, string? ContactType)>> GetAllIdsAsync(HttpClient http, string from_object,int limit ,string flields)
        {
            var data = new List<(string Id, string? Value, string? Value1)>();
            string? after = null;

            do
            {
                var url = new StringBuilder(
                    $"https://api.hubapi.com/crm/v3/objects/{from_object}?limit={limit}&properties={flields}&archived=false"
                );
                if (!string.IsNullOrEmpty(after))
                    url.Append("&after=").Append(Uri.EscapeDataString(after));

                using var resp = await http.GetAsync(url.ToString());
                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync();
                var page = JsonSerializer.Deserialize<ContactsPage>(json);

                if (page?.Results != null)
                {
                    foreach (var r in page.Results)
                    {
                        if(from_object == "contacts")
                            data.Add((r.Id, r.Properties?.Email, r.Properties?.contact_type));
                        else if(from_object == "2-130920180")
                            data.Add((r.Id, r.Properties?.Clientid,""));

                    }
                        
                    
                }
                //return data;
                after = page?.Paging?.Next?.After;
            }
            while (!string.IsNullOrEmpty(after));

            return data;

        }

        static async Task<long?> GetAssociationTypeIdByLabelAsync(HttpClient http, string from_object, string to_object,string label)
        {
            using var resp = await http.GetAsync($"https://api.hubapi.com/crm/v4/associations/{from_object}/{to_object}/labels");
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            var doc = JsonSerializer.Deserialize<LabelsResponse>(json);
            var hit = doc?.Results?.FirstOrDefault(x => string.Equals(x.Label, label, StringComparison.Ordinal));
            return hit?.TypeId;
        }

        static async Task<BatchAssocResponse?> ReadAssociationsBatchAsync(HttpClient http, string from_object, string to_object,IEnumerable<string> contactIds)
        {
            var payload = new
            {
                inputs = contactIds.Select(id => new { id })
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var resp = await http.PostAsync($"https://api.hubapi.com/crm/v4/associations/{from_object}/{to_object}/batch/read", content);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BatchAssocResponse>(json);
        }
        static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> list, int size)
        {
            for (int i = 0; i < list.Count; i += size)
                yield return list.Skip(i).Take(Math.Min(size, list.Count - i)).ToList();
        }

        // ===== Models for Contacts (paging) =====
        #region Models for Contacts
        public sealed record ContactsPage(
          [property: JsonPropertyName("results")] List<ContactItem>? Results,
          [property: JsonPropertyName("paging")] Paging? Paging
      );

        public sealed record ContactItem(
            [property: JsonPropertyName("id")] string Id,
            [property: JsonPropertyName("properties")] ContactProperties Properties
         );

        public sealed record ContactProperties(
                    [property: JsonPropertyName("email")] string? Email,
                    [property: JsonPropertyName("client_id")] string? Clientid,
                    [property: JsonPropertyName("newregistrationplate")] string? newregistrationplate,
                    [property: JsonPropertyName("ops__tafnit_unit")] string? ops__tafnit_unit,
                    [property: JsonPropertyName("contact_type")] string? contact_type,
                    [property: JsonPropertyName("kodmachzormechira")] string? kodmachzormechira

                 );

        public sealed record Paging(
            [property: JsonPropertyName("next")] Next? Next
        );

        public sealed record Next(
            [property: JsonPropertyName("after")] string? After
        );

        // ===== Models for Labels =====
        public sealed record LabelsResponse(
            [property: JsonPropertyName("results")] List<LabelItem>? Results
        );

        public sealed record LabelItem(
            [property: JsonPropertyName("typeId")] long TypeId,
            [property: JsonPropertyName("label")] string Label
        );

        // ===== Models for Associations (Batch Read) =====
        public sealed record BatchAssocResponse(
            [property: JsonPropertyName("results")] List<BatchAssocRow>? Results,
            [property: JsonPropertyName("errors")] List<BatchAssocError>? Errors,
            [property: JsonPropertyName("numErrors")] int? NumErrors
        );

        public sealed record BatchAssocRow(
            [property: JsonPropertyName("from")] AssocFrom From,
            [property: JsonPropertyName("to")] List<AssocToItem>? To
        );

        public sealed record AssocFrom(
            [property: JsonPropertyName("id")] string Id
        );

        public sealed record AssocToItem(
            [property: JsonPropertyName("toObjectId")] long ToObjectId,
            [property: JsonPropertyName("associationTypes")] List<AssocType>? AssociationTypes
        );

        public sealed record AssocType(
            [property: JsonPropertyName("category")] string? Category,
            [property: JsonPropertyName("typeId")] long TypeId,
            [property: JsonPropertyName("label")] string? Label
        );

        // ===== Optional: Errors from API =====
        public sealed record BatchAssocError(
            [property: JsonPropertyName("status")] string? Status,
            [property: JsonPropertyName("category")] string? Category,
            [property: JsonPropertyName("subCategory")] string? SubCategory,
            [property: JsonPropertyName("message")] string? Message,
            [property: JsonPropertyName("context")] Dictionary<string, List<string>>? Context
        ); 
        #endregion







        public Dictionary<int, string> associationLabels = new Dictionary<int, string>
            {
                { 73, "איש קשר" },
                { 46, "בעל רכב" },
                { 48, "מחזיק רכב" },
                { 99, "מוסך" },
                { 120, "פוליסה" }
            };
        public static class HubspotObject
        {
            public const string Car = "2-124289527";
            public const string Person = "2-130920180";
        }
        public async Task InsertContactIdsAsync(List<(string Id, string? Email, string? contact_type)> contactList, string connectionString)
        {

            var table = new DataTable();
            table.Columns.Add("ContactKey", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("contact_type", typeof(string));
            table.Columns.Add("InsertedAt", typeof(DateTime));
            
            var now = DateTime.UtcNow;
            foreach (var id in contactList)
                table.Rows.Add(id.Id, id.Email, id.contact_type, now);


            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var bulk = new SqlBulkCopy(conn)
            {
                DestinationTableName = "[HavayatLakoah].[hubspotContact]",
                BatchSize = 5000
            };
            bulk.ColumnMappings.Add("ContactKey", "ContactKey");
            bulk.ColumnMappings.Add("Email", "Email");
            bulk.ColumnMappings.Add("contact_type", "contact_type");
            bulk.ColumnMappings.Add("InsertedAt", "InsertedAt");

            await bulk.WriteToServerAsync(table);
        }
        public async Task InsertPersonIdsAsync(List<(string Id, string? ClientId, string? EmptyValue)> personList, string connectionString)
        {

            var table = new DataTable();
            table.Columns.Add("PersonKey", typeof(string));
            table.Columns.Add("ClientID", typeof(string));
            table.Columns.Add("InsertedAt", typeof(DateTime));

            var now = DateTime.UtcNow;
            foreach (var id in personList)
                table.Rows.Add(id.Id, id.ClientId, now);


            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var bulk = new SqlBulkCopy(conn)
            {
                DestinationTableName = "[HavayatLakoah].[hubspotPerson]",
                BatchSize = 5000
            };
            bulk.ColumnMappings.Add("PersonKey", "PersonKey");
            bulk.ColumnMappings.Add("ClientID", "ClientID");
            bulk.ColumnMappings.Add("InsertedAt", "InsertedAt");

            await bulk.WriteToServerAsync(table);
        }
        public async Task InsertCarsIdsAsync(List<(string Id, string? newregistrationplate, string? ops__tafnit_unit, string? kodmachzormechira)> carList, string connectionString)
        {

            var table = new DataTable();
            table.Columns.Add("CarKey", typeof(string));
            table.Columns.Add("newregistrationplate", typeof(string));
            table.Columns.Add("ops__tafnit_unit", typeof(string));
            table.Columns.Add("InsertedAt", typeof(DateTime));
            table.Columns.Add("kodmachzormechira", typeof(string));

            var now = DateTime.UtcNow;
            foreach (var id in carList)
                table.Rows.Add(id.Id, id.newregistrationplate, id.ops__tafnit_unit, now, id.kodmachzormechira);


            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var bulk = new SqlBulkCopy(conn)
            {
                DestinationTableName = "[HavayatLakoah].[hubspotCars]",
                BatchSize = 5000
            };
            bulk.ColumnMappings.Add("CarKey", "CarKey");
            bulk.ColumnMappings.Add("newregistrationplate", "newregistrationplate");
            bulk.ColumnMappings.Add("ops__tafnit_unit", "Unit");
            bulk.ColumnMappings.Add("InsertedAt", "InsertedAt");
            bulk.ColumnMappings.Add("kodmachzormechira", "kodmachzormechira");

            await bulk.WriteToServerAsync(table);
        }
        public async Task InsertObjectVsCarAssocaited(List<(string FromObjectId, string ToObjectId, long TypeId, string Label)> contactVsCarList, string connectionString,string from_object)
        {
            string _DestinationTableName = string.Empty;
            var table = new DataTable();

            if (from_object == "contacts")
            {
                _DestinationTableName = "[HavayatLakoah].[hubspotContactVsCar]";
                table.Columns.Add("ContactKey", typeof(string));
             }
                
            else if (from_object == "2-130920180")
            {
                _DestinationTableName = "[HavayatLakoah].[hubspotPersonVsCar]";
                table.Columns.Add("PersonKey", typeof(string));
            }
                

            
            
            table.Columns.Add("CarKey", typeof(string));
            table.Columns.Add("typeId", typeof(long));
            table.Columns.Add("label", typeof(string));
            table.Columns.Add("InsertedAt", typeof(DateTime));

            var now = DateTime.UtcNow;
            foreach (var id in contactVsCarList)
                table.Rows.Add(id.FromObjectId, id.ToObjectId, id.TypeId, id.Label,now);


            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var bulk = new SqlBulkCopy(conn)
            {
                      
                
                DestinationTableName = _DestinationTableName,
                BatchSize = 5000
            };
            if (from_object == "contacts")
                bulk.ColumnMappings.Add("ContactKey", "ContactKey");
            else if (from_object == "2-130920180")
                bulk.ColumnMappings.Add("PersonKey", "PersonKey");

            bulk.ColumnMappings.Add("CarKey", "CarKey");
            bulk.ColumnMappings.Add("typeId", "typeId");
            bulk.ColumnMappings.Add("label", "label");
            bulk.ColumnMappings.Add("InsertedAt", "InsertedAt");

            await bulk.WriteToServerAsync(table);
        }
        public async Task<DataTable> LoadContactVsCarAsync(string connectionString,string query)
        {
            var table = new DataTable();

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();

            table.Load(reader);

            return table;
        }


    }
}
