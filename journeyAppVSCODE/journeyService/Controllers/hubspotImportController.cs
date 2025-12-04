using DocumentFormat.OpenXml.Wordprocessing;
using journeyService.Models;
using journeyService.Models.howazit;
using journeyService.Models.hubspot;
using journeyService.Models.hubspot.Api;
using journeyService.Models.leasing;
using journeyService.Utils;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace journeyService.Controllers
{
    [Route("api/hubspotImport")]
    [ApiController]
    public class hubspotImportController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IOptions<ParamsSetting> _paramssetting;

        #region ctor
        public hubspotImportController(IConfiguration configuration,
            IOptions<ParamsSetting> paramssetting)
        {
            //by default IConfiguration is injected, not realy need to Inject
            _config = configuration;
            _paramssetting = paramssetting;

        }
        #endregion


        [HttpGet("ImportTafnitCsvFile/{codeAction}")]
        public async Task<ActionResult> ImportTafnitCsvFile([FromRoute] int codeAction)
        {
            //from site,
            //http://localhost/journeyApp/api/hubspotImport/ImportTafnitCsvFile/1
            //from program 
            //http://localhost:5279/api/hubspotImport/ImportTafnitCsvFile/1

            //http://umi-appsites/journeyApp/api/hubspotImport/ImportTafnitCsvFile/1

            hubspotResponse _hubspotResponse = null;
            int logID = -1;
            try
            {
             

                logID = DataProvider.InsertHubSpotImportRequest(
                    _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                   Request.GetDisplayUrl(),
                   codeAction,
                   "ImportUmiCars"
                   );

                string jsonResponseContent = string.Empty;
                //Models\hubspot\
                string HubSpotImportPath = (@_config.GetValue<string>("ApiHubSpot:HubSpotImportPath") ?? "");
                //UmiCars
                string HubSpotImportUmiFileName = (_config.GetValue<string>("ApiHubSpot:HubSpotImportUmiFileName") ?? "");





                //format filename (for exmaple "UmiCars20240909.xlsx")
                string filename = HubSpotImportUmiFileName + DateTime.Now.ToString("yyyyMMdd")
                    + ".xlsx";

                // File.Copy(Path.Combine(sourceDir, fName), Path.Combine(backupDir, fName), true);
                // Will overwrite if the destination file already exists.
                //copy Models\hubspot\excelFile\UmiCars.xlsx
                //to folder Models\hubspot\excelFile\fileToUpload\ and rename to UmiCarsyyyymmdd.xlsx
                #region copy file
                System.IO.File.Copy(
                       Path.Combine(Environment.CurrentDirectory
                       + HubSpotImportPath
                       + "excelFile\\",
                       HubSpotImportUmiFileName + ".xlsx")
                        ,
                       Path.Combine(Environment.CurrentDirectory +
                       HubSpotImportPath
                       + "excelFile\\fileToUpload\\", filename)
                       , true);
                #endregion
                //check if current file(Models\hubspot\excelFile\fileToUpload\UmiCarsyyyymmdd.xlsx)
                //exists in directory
                if (!System.IO.File.Exists(Environment.CurrentDirectory +
                    HubSpotImportPath
                    + "excelFile\\fileToUpload\\" + filename))
                {
                    _hubspotResponse = new hubspotResponse();
                    _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
                    _hubspotResponse.accepted = false;
                    _hubspotResponse.errorDescription = $"the file: {filename} is missing";

                    DataProvider.UpdateHubSpotImportResponse(
                            _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                           , logID
                           , ""
                           , _hubspotResponse
                          );

                }

                else
                {


                    string ParamValue = DataProvider.getParamValueApplicationParamaters(
                       _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                       "IMPORT", "HUBSPOT"
                       );

                    string[] parts = ParamValue.Split('/');
                    //Get data to upload
                    //SP : HUBSPOT_Data_Processing get timeout , therefore i separated
                    //DataProvider.DataProcessing(_config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "");
                    DataProvider.DataProcessing(_config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "", $"exec [HavayatLakoah].GET_UMI_CARS_TODAY");
                    DataProvider.DataProcessing(_config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "", $"exec [HavayatLakoah].GET_AVIS_CARS_TODAY");
                    DataProvider.DataProcessing(_config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "", $"exec [HavayatLakoah].GET_CARS_TODAY_UNION");
                    DataProvider.DataProcessing(_config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "", $"exec [HavayatLakoah].AssociatedThatNeedTobeChanged");




                    List<UmiCars> _UmiCars = DataProvider.GetUmiCars(
                        _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                        $"{parts[2]}-{parts[1]}-{parts[0]} 00:00:00"
                        );

                    //write data in excel
                    if (_UmiCars.Count > 0)
                    {
                        ///////////Start Update Associated/////////////
                        ///חייב שירוץ לפני העלאת נתונים כי אחרת לא תמיד יצליח לעשות את השינויים בגלל המפתחות
                        ///לדוגמא יחליט לשים איש קשר וכבר יש איש קשר
                        //await UpdateAndDeleteAssociatedLabelInternal(5);
                        await UpdateAndDeleteAssociatedLabelInternalVer2(5);

                        ///////////End Update Associated/////////////
                        ///

                        //fill relevant excel 
                        General.CreateExcel(
                         Path.Combine(Environment.CurrentDirectory +
                         HubSpotImportPath
                         + "excelFile\\fileToUpload\\", filename)
                         , _UmiCars
                         );


                        //read template Models\hubspot\jsonFile\UmiCars.json
                        //for hubspot
                        #region Read UmiCars.json 
                        string JsonFileTemplateStr = Utils.General.ReadJsonFileTemplate(
                                  Environment.CurrentDirectory +
                                 HubSpotImportPath
                                 + "jsonFile\\"
                                 ,
                                 HubSpotImportUmiFileName +
                                 ".json"
                                 );
                        ////reaplce from JsonFileTemplateStr "templatefile.xlsx" to  UmiCarsyyyymmdd.xlsx
                        JsonFileTemplateStr = JsonFileTemplateStr.Replace("templatefile.xlsx", filename);
                        #endregion
                        //Import Data to Hubspot
                        _hubspotResponse = await callHubSpotEndPoint(
                             _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                            , _config.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint") + "crm/v3/imports" ?? ""
                            , _config.GetValue<string>("ApiHubSpot:HubSpotToken") ?? ""
                            , logID
                            , filename
                            , JsonFileTemplateStr
                            , HubSpotImportPath
                            );


                        ////delete record exist in hubspot and not exists In Tafint
                        //List<(string KeyToArchive, string ObjectNameToArchive)> ArchiveList =
                        //    DataProvider.GetHubSpotArchiveRecord(_config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "");
                        //if(ArchiveList.Any())
                        //{
                        //    string token = _config.GetValue<string>("ApiHubSpot:HubSpotToken") ?? "";
                        //    // הסרת Bearer אם כבר קיים בקובץ הקונפיג
                        //    if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        //    {
                        //        token = token.Substring("Bearer ".Length);
                        //    }

                        //    var deleter = new HubSpotAssociationDelete(token, _config);
                        //    await deleter.ArchiveRecord(ArchiveList);
                        //}

                        DataProvider.UpdateApplicationParamaters(
                            _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                            "IMPORT", "HUBSPOT", DateTime.Now.ToString("dd/MM/yyyy")
                            );
                    }
                    else
                    {
                        _hubspotResponse = new hubspotResponse();
                        _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
                        _hubspotResponse.accepted = false;
                        _hubspotResponse.errorDescription = $"the file: {filename} is emtpy";

                        DataProvider.UpdateHubSpotImportResponse(
                                _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                               , logID
                               , ""
                               , _hubspotResponse
                              );
                    }
                }

             }
            catch (IOException ex)
            {
                _hubspotResponse = new hubspotResponse();
                _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
                _hubspotResponse.accepted = false;


                // Check if the exception is due to the file being used by another process
                if (IsFileLocked(ex))
                {
                    _hubspotResponse.errorDescription = "The file is currently in use by another process.";

                }
                else
                {
                    _hubspotResponse.errorDescription = ex.Message;
                }
                DataProvider.UpdateHubSpotImportResponse(
                        _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                       , logID
                       , ""
                       , _hubspotResponse
                      );
            }
            catch (Exception ex)
            {
                _hubspotResponse = new hubspotResponse();
                _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
                _hubspotResponse.accepted = false;
                _hubspotResponse.errorDescription = ex.Message;

                DataProvider.UpdateHubSpotImportResponse(
                        _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                       , logID
                       , ""
                       , _hubspotResponse
                      );

            }


            return Ok(_hubspotResponse);
        }

        //// GET: api/<hubspotImportController>
        //[HttpGet("ImportCsvFile/{codeAction}")]
        //public async Task<ActionResult> ImportCsvFile([FromRoute] int codeAction)
        //{
        //    //from site,
        //    //http://localhost/journeyApp/api/hubspotImport/ImportCsvFile/1
        //    //from program 
        //    //http://localhost:5279/api/hubspotImport/ImportCsvFile/1

        //    //http://umi-appsites/journeyApp/api/hubspotImport/ImportCsvFile/1
        //    /*
        //     * example to Use In My PostMan file name : UpsertFIleCSv
        //     * Need to check that i have that the file exists with yyyymmdd
        //     */


            




        //    hubspotResponse _hubspotResponse = null;
        //    int logID = -1;
        //    try
        //    {
                



        //        logID = DataProvider.InsertHubSpotImportRequest(
        //          _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
        //          Request.GetDisplayUrl(),
        //          codeAction,
        //          "ImportUmiCars"
        //          );


        //        string jsonResponseContent = string.Empty;
        //        //Models\hubspot\
        //        string HubSpotImportPath = (@_config.GetValue<string>("ApiHubSpot:HubSpotImportPath") ?? "");
        //        //UmiCars
        //        string HubSpotImportUmiFileName = (_config.GetValue<string>("ApiHubSpot:HubSpotImportUmiFileName") ?? "");



                

        //        //format filename (for exmaple "UmiCars20240909.xlsx")
        //        string filename = HubSpotImportUmiFileName + DateTime.Now.ToString("yyyyMMdd")
        //            + ".xlsx";

        //        // File.Copy(Path.Combine(sourceDir, fName), Path.Combine(backupDir, fName), true);
        //        // Will overwrite if the destination file already exists.
        //        //copy Models\hubspot\excelFile\UmiCars.xlsx
        //        //to folder Models\hubspot\excelFile\fileToUpload\ and rename to UmiCarsyyyymmdd.xlsx
        //        #region copy file
        //        System.IO.File.Copy(
        //               Path.Combine(Environment.CurrentDirectory
        //               + HubSpotImportPath
        //               + "excelFile\\",
        //               HubSpotImportUmiFileName + ".xlsx")
        //                ,
        //               Path.Combine(Environment.CurrentDirectory +
        //               HubSpotImportPath
        //               + "excelFile\\fileToUpload\\", filename)
        //               , true);
        //        #endregion
        //        //check if current file(Models\hubspot\excelFile\fileToUpload\UmiCarsyyyymmdd.xlsx)
        //        //exists in directory
        //        if (!System.IO.File.Exists(Environment.CurrentDirectory +
        //            HubSpotImportPath
        //            + "excelFile\\fileToUpload\\" + filename))
        //        {
        //            _hubspotResponse = new hubspotResponse();
        //            _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
        //            _hubspotResponse.accepted = false;
        //            _hubspotResponse.errorDescription = $"the file: {filename} is missing";

        //            DataProvider.UpdateHubSpotImportResponse(
        //                    _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
        //                   , logID
        //                   , ""
        //                   , _hubspotResponse
        //                  );

        //        }

        //        else
        //        {

        //            string ParamValue = DataProvider.getParamValueApplicationParamaters(
        //                _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
        //                "IMPORT", "HUBSPOT"
        //                );
        //            string[] parts = ParamValue.Split('/');
        //            //Get data to upload
        //            List<UmiCars> _UmiCars = DataProvider.GetUmiCars(
        //                _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
        //                $"{parts[2]}-{parts[1]}-{parts[0]} 00:00:00"
        //                );


        //            //write data in excel
        //            if (_UmiCars.Count > 0)
        //            {

        //                ///////////Start Update Associated/////////////
        //                ///חייב שירוץ לפני העלאת נתונים כי אחרת לא תמיד יצליח לעשות את השינויים בגלל המפתחות
        //                ///לדוגמא יחליט לשים איש קשר וכבר יש איש קשר
        //                await UpdateAndDeleteAssociatedLabelInternal(5);

        //                ///////////End Update Associated/////////////
        //                //fill relevant excel 
        //                General.CreateExcel(
        //                 Path.Combine(Environment.CurrentDirectory +
        //                 HubSpotImportPath
        //                 + "excelFile\\fileToUpload\\", filename)
        //                 , _UmiCars
        //                 );


        //                //read template Models\hubspot\jsonFile\UmiCars.json
        //                //for hubspot
        //                #region Read UmiCars.json 
        //                string JsonFileTemplateStr = Utils.General.ReadJsonFileTemplate(
        //                          Environment.CurrentDirectory +
        //                         HubSpotImportPath
        //                         + "jsonFile\\"
        //                         ,
        //                         HubSpotImportUmiFileName +
        //                         ".json"
        //                         );
        //                ////reaplce from JsonFileTemplateStr "templatefile.xlsx" to  UmiCarsyyyymmdd.xlsx
        //                JsonFileTemplateStr = JsonFileTemplateStr.Replace("templatefile.xlsx", filename);
        //                #endregion
        //                //Import Data to Hubspot
        //                _hubspotResponse = await callHubSpotEndPoint(
        //                     _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
        //                    , _config.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint") + "crm/v3/imports" ?? ""
        //                    , _config.GetValue<string>("ApiHubSpot:HubSpotToken") ?? ""
        //                    , logID
        //                    , filename
        //                    , JsonFileTemplateStr
        //                    , HubSpotImportPath
        //                    );

        //                DataProvider.UpdateApplicationParamaters(
        //                    _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
        //                    "IMPORT", "HUBSPOT", DateTime.Now.ToString("dd/MM/yyyy")
        //                    );

        //            }
        //            else
        //            {
        //                _hubspotResponse = new hubspotResponse();
        //                _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
        //                _hubspotResponse.accepted = false;
        //                _hubspotResponse.errorDescription = $"the file: {filename} is emtpy";

        //                DataProvider.UpdateHubSpotImportResponse(
        //                        _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
        //                       , logID
        //                       , ""
        //                       , _hubspotResponse
        //                      );
        //            }






        //        }

        //    }
        //    catch (IOException ex)
        //    {
        //        _hubspotResponse = new hubspotResponse();
        //        _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
        //        _hubspotResponse.accepted = false;
                

        //        // Check if the exception is due to the file being used by another process
        //        if (IsFileLocked(ex))
        //        {
        //            _hubspotResponse.errorDescription = "The file is currently in use by another process.";
                    
        //        }
        //        else
        //        {
        //            _hubspotResponse.errorDescription = ex.Message;
        //        }
        //        DataProvider.UpdateHubSpotImportResponse(
        //                _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
        //               , logID
        //               , ""
        //               , _hubspotResponse
        //              );
        //    }
        //    catch (Exception ex)
        //    {
        //        _hubspotResponse = new hubspotResponse();
        //        _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
        //        _hubspotResponse.accepted = false;
        //        _hubspotResponse.errorDescription = ex.Message;

        //        DataProvider.UpdateHubSpotImportResponse(
        //                _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
        //               , logID
        //               , ""
        //               , _hubspotResponse
        //              );

        //    }
        

        //    return Ok(_hubspotResponse);

        //}

        #region hubspotUmiImportResponse
        [NonAction]
        public async Task UpdateAndDeleteAssociatedLabelInternalVer2(int codeAction)
        {
            int logID = -1;
            int logIDRest = -1;
            string payload = string.Empty;
            hubspotResponse _hubspotResponse = null;
            List<(string fromID, string toID)> associations;
            //save assocaited i need to treated <fromId, toObjectId,ChangeType>
            List<HubSpotAssociationChangeType> _HubSpotAssociationChangeType = new List<HubSpotAssociationChangeType>();
            try
            {
                string token = _config.GetValue<string>("ApiHubSpot:HubSpotToken") ?? "";
                // הסרת Bearer אם כבר קיים בקובץ הקונפיג
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = token.Substring("Bearer ".Length);
                }

                //save log
                logID = DataProvider.InsertHubSpotImportRequest(
                  _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? "",
                  Request.GetDisplayUrl(),
                  codeAction,
                  "updateanddeleteAssociatedLabel"
                  );
                //get record from db (contact_association_lable && association_lable)
                List<UpDelAssociatedLabel> _UpDelAssociatedLabel = DataProvider.GetAssociatedChangedUmiCars(
                     _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                     );

                if (_UpDelAssociatedLabel == null || !_UpDelAssociatedLabel.Any())
                {
                    _hubspotResponse = new hubspotResponse();
                    _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
                    _hubspotResponse.accepted = false;
                    _hubspotResponse.errorDescription = "No changes were found from the previous time.";


                    DataProvider.UpdateHubSpotImportResponse(
                            _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                           , logID
                           , ""
                           , _hubspotResponse
                          );
                    //return NotFound("לא נמצאו רשומות מה-DB.");
                }
                else
                {
                    List<string> clientids;
                    var emailMapper = new HubSpotValueToIdMapper(token, _config);
                    //  קבלת Custom Object IDs לפי Contact IDs
                    var reader = new HubSpotAssociationReader(token, _config);

                    #region car_contact_Associated
                    var filteredContactList = _UpDelAssociatedLabel
                                   .Where(x => x.fieldname == "contact_association_lable")
                                   .ToList();


                    // שלב 2: חילוץ כתובות אימייל
                    var emails = filteredContactList
                        .Select(r => r.old_Email?.Trim().ToLower())
                        .Where(e => !string.IsNullOrEmpty(e))
                        .Distinct()
                        .ToList();

                    if (emails.Any())
                    {
                        // get id properties from contact hubspot via email
                        //return Dictionary <email,id>
                        Dictionary<string, string> emailToContactIdMap = await emailMapper.MapValueToIdsAsync(
                            $"{_config.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v3/objects/contacts/search",
                            filteredContactList,
                            logID, "searchViaEmail", "old_Email", "email", "email");

                        //take all value from Dictionary without key (all id list)
                        var contactIds = emailToContactIdMap.Values.ToList();

                        //// get id properties from cars object via hubspot associations cotains :  List <fromId, toObjectId>
                        //לקוח יכול להיות איש קשר לכמה רכבים
                        //מחזיר רשומות עם אסוציאציה איש קשר או איש קשר דן ליס או איש קשר סרטיפייד
                        associations = await reader.GetFromIDToIDHasAssociated(
                            $"{_config.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v4/associations/contacts/{HubspotObject.Car}/batch/read",
                            contactIds,
                            logID
                            );
                        if (associations.Any())
                        {
                            var deleter = new HubSpotAssociationDelete(token, _config);
                            await deleter.DelAssociationsAsyncVer2(
                                    associations,
                                    "DeleteContactAssociated",
                                    logID,
                                    $"{_config.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v4/associations/contacts/{HubspotObject.Car}/batch/labels/archive"
                                    );

                            _hubspotResponse = new hubspotResponse();
                            _hubspotResponse._hubspotResponseImport.estimatedLineCount = filteredContactList.Count;
                            _hubspotResponse.accepted = true;
                            _hubspotResponse.errorDescription = "";

                            DataProvider.UpdateHubSpotImportResponse(
                                 _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                                , logID
                                , ""
                                , _hubspotResponse
                               );
                        }
                        else
                        {
                            _hubspotResponse = new hubspotResponse();
                            _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
                            _hubspotResponse.accepted = false;
                            _hubspotResponse.errorDescription = "contact assocaited not found";


                            DataProvider.UpdateHubSpotImportResponse(
                                    _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                                   , logID
                                   , ""
                                   , _hubspotResponse
                                  );
                        }



                    }
                    #endregion
                    #region car_personAssociated

                    var filteredPersonList = _UpDelAssociatedLabel
                         .Where(x => x.fieldname == "association_lable")
                         .ToList();

                        clientids = filteredPersonList
                          .Select(r => r.old_ClientID?.Trim().ToLower() ?? "")
                          .Where(e => !string.IsNullOrEmpty(e))
                          .Distinct()
                          .ToList();

                        var carnums = filteredPersonList
                          .Select(r => r.old_Registration_plate?.Trim().ToLower())
                          .Where(e => !string.IsNullOrEmpty(e))
                          .Distinct()
                          .ToList();
                        if (clientids.Any() && carnums.Any())
                        {
                            // get id properties from person object via hubspot by clientid

                            //return Dictionary <clientid,id>

                            Dictionary<string, string> clientidToPersonIdMap = await emailMapper.MapValueToIdsAsync(
                                $"{_config.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v3/objects/{HubspotObject.Person}/search",
                                filteredPersonList,
                                logID, "searchViaClientID", "old_ClientID", "client_id", "client_id");


                            //return Dictionary <carnum,id>

                            Dictionary<string, string> carnumTocarIdMap = await emailMapper.MapValueToIdsAsync(
                                $"{_config.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v3/objects/{HubspotObject.Car}/search",
                                filteredPersonList,
                                logID, "searchViaCarNum", "old_Registration_plate", "newregistrationplate", "newregistrationplate");



                            //take all value from Dictionary without key (all id list)
                            var personIds = clientidToPersonIdMap.Values.ToList();

                            // if associated in (48,50) return the objectid
                            //להבדיל מאיש קשר שזה כן או לא , כאן יכול להיות כמה צירופים
                            //אז מספיק שאחד מהם קיים אני מוחק ויוצר מחדש
                            associations = await reader.GetFromIDToIDHasAssociated(
                                    $"{_config.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v4/associations/{HubspotObject.Person}/{HubspotObject.Car}/batch/read",
                                    personIds,
                                    logID,
                                    clientidToPersonIdMap, //Dictionary < clientid, id >
                                    carnumTocarIdMap //Dictionary <carnum,id>
                                    );
                            if (associations.Any())
                            {
                                var deleter = new HubSpotAssociationDelete(token, _config);
                                await deleter.DelAssociationsAsyncVer2(
                                      associations,
                                      "DeletePersonAssociated",
                                      logID,
                                      $"{_config.GetValue<string>("ApiHubSpot:HubSpotImportEndPoint")?.TrimEnd('/')}/crm/v4/associations/{HubspotObject.Person}/{HubspotObject.Car}/batch/labels/archive"
                                      );
                        }
                            else
                            {
                                _hubspotResponse = new hubspotResponse();
                                _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
                                _hubspotResponse.accepted = false;
                                _hubspotResponse.errorDescription = "person assocaited not found";


                                DataProvider.UpdateHubSpotImportResponse(
                                        _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                                       , logID
                                       , ""
                                       , _hubspotResponse
                                      );
                            }

                    }
                        #endregion
                    }

            }
            catch (Exception ex)
            {
                _hubspotResponse = new hubspotResponse();
                _hubspotResponse._hubspotResponseImport.estimatedLineCount = -1;
                _hubspotResponse.accepted = false;
                _hubspotResponse.errorDescription = ex.Message;

                DataProvider.UpdateHubSpotImportResponse(
                        _config.GetValue<string>("ConnectionStrings:sqlBIDataGlobalBI") ?? ""
                       , logID
                       , ""
                       , _hubspotResponse
                      );

            }
        }
        
        public async Task<hubspotResponse> callHubSpotEndPoint(
              string ConnectionString, 
              string urlApi,
              string tokenApi,  
              int logID,
              string filename,
              string JsonFileTemplateStr,
              string HubSpotImportPath
            )
        {

            
            HttpResponseMessage response;
            hubspotResponse result  = new hubspotResponse();
            string jsonResponseContent = string.Empty;

            var stream = System.IO.File.OpenRead(
                        Environment.CurrentDirectory +
                        HubSpotImportPath
                         + "excelFile\\fileToUpload\\"
                         + filename);

            try
            {

                var client = new HttpClient();

                //conect endpoint 
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    urlApi);

                //add token to header
                request.Headers.Add("Authorization",
                        tokenApi);


                var content = new MultipartFormDataContent();

                
                    

                //add todays file name to body request with key name "files"                       
                content.Add(new StreamContent(stream),"files",filename);

                //content.Add(new StreamContent(
                //    System.IO.File.OpenRead(
                //        Environment.CurrentDirectory +
                //        HubSpotImportPath
                //         + "excelFile\\fileToUpload\\" 
                //         + filename)
                //    ),
                //    "files",
                //    filename);

                //content.Add(new StringContent("{\"name\":\"dror08092024\",\"dateFormat\":\"YEAR_MONTH_DAY\",\"files\":[{\"fileName\":\"UmiCars20240909.xlsx\",\"fileFormat\":\"SPREADSHEET\",\"fileImportPage\":{\"hasHeader\":true,\"columnMappings\":[{\"columnObjectTypeId\":\"
                //
                //
                //\",\"columnName\":\"client_id\",\"propertyName\":\"client_id\",\"columnType\":\"HUBSPOT_ALTERNATE_ID\"},{\"columnObjectTypeId\":\"2-130920180\",\"columnName\":\"firstname\",\"propertyName\":\"firstname\"},{\"columnObjectTypeId\":\"2-130920180\",\"columnName\":\"lastname\",\"propertyName\":\"lastname\"},{\"columnObjectTypeId\":\"0-1\",\"columnName\":\"allow_divur\",\"propertyName\":\"allow_divur\"},{\"columnObjectTypeId\":\"0-1\",\"columnName\":\"email\",\"propertyName\":\"email\",\"columnType\":\"HUBSPOT_ALTERNATE_ID\"},{\"columnObjectTypeId\":\"2-130920180\",\"columnName\":\"phone_number\",\"propertyName\":\"phone_number\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"manufacture_year\",\"propertyName\":\"manufacture_year\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"registration_plate\",\"propertyName\":\"registration_plate\",\"columnType\":\"HUBSPOT_ALTERNATE_ID\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"car_model\",\"propertyName\":\"car_model\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"manufacturer\",\"propertyName\":\"manufacturer\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"last_treatment_date\",\"propertyName\":\"last_treatment_date\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"car_delivery_date\",\"propertyName\":\"car_delivery_date\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"kod_sales_branch\",\"propertyName\":\"kod_sales_branch\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"sales_branch\",\"propertyName\":\"sales_branch\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"last_garage_code\",\"propertyName\":\"last_garage_code\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"last_garage_name\",\"propertyName\":\"last_garage_name\"},{\"columnName\":\"association_lable\",\"columnType\":\"FLEXIBLE_ASSOCIATION_LABEL\",\"columnObjectTypeId\":\"2-124289527\",\"toColumnObjectTypeId\":\"2-130920180\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"entrydate\",\"propertyName\":\"entrydate\"},{\"columnObjectTypeId\":\"2-124289527\",\"columnName\":\"dateidkunbaalu\",\"propertyName\":\"dateidkunbaalu\"}]}}]}"), "importRequest");

                //add Json temaplte to body with key name "importRequest"                       
                content.Add(new StringContent(JsonFileTemplateStr), "importRequest");


                request.Content = content;

                //update request In DB
                DataProvider.UpdateHubSpotImportRequest(ConnectionString, 
                    logID, 
                    JsonFileTemplateStr,
                    urlApi);


                response = await client.SendAsync(request);


                //response.EnsureSuccess
                //();

                if (response.IsSuccessStatusCode)
                {
                    jsonResponseContent = await response.Content.ReadAsStringAsync();


                    
                    result.accepted = true;
                    result.errorDescription = "";
                    result._hubspotResponseImport = JsonConvert.DeserializeObject<hubspotResponseImport>(jsonResponseContent);
                    result._hubspotResponseImport.estimatedLineCount = Convert.ToInt64(GetElementestimatedLineCount(jsonResponseContent));


                }
                else
                {
                    jsonResponseContent = await response.Content.ReadAsStringAsync();
                    result._hubspotResponseImport.estimatedLineCount = -1;
                    result.accepted = false;
                    result.errorDescription = "error description in RequestStr field ";
                    result._hubspotResponseImport = JsonConvert.DeserializeObject<hubspotResponseImport>(jsonResponseContent);


                }
          
            }
            catch (Exception ex)
            {
                result._hubspotResponseImport.estimatedLineCount = -1;
                result.accepted = false;
                result.errorDescription = ex.Message;

            }
            finally
            {
                DataProvider.UpdateHubSpotImportResponse(
                            ConnectionString
                           , logID
                           , jsonResponseContent
                           , result

                           
                           );
                stream.Close();

            

            }


            return result;
           
        }
        
        public ulong GetElementestimatedLineCount(string json)
        {
                // https://kevsoft.net/2021/12/19/traversing-json-with-jsondocument.html
            var yourObject = System.Text.Json.JsonDocument.Parse(json);
            var fileImportsJsonElement = yourObject.RootElement.GetProperty("importRequestJson").GetProperty("fileImports");
            foreach (var jsonElement in fileImportsJsonElement.EnumerateArray())
            {
                var estimatedLineCountElement = jsonElement.GetProperty("estimatedLineCount");
                return estimatedLineCountElement.GetUInt64();
            }
            return Convert.ToUInt64(-1);
        }

        #endregion

        static bool IsFileLocked(IOException ioException)
        {
            int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ioException) & 0xFFFF;
            return errorCode == 32 || errorCode == 33;
            // ERROR_SHARING_VIOLATION = 32
            // ERROR_LOCK_VIOLATION = 33
        }


    }
}
