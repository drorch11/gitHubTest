using journeyService.Models;
using journeyService.Models.flycard;
using journeyService.Models.howazit;
using journeyService.Models.hubspot;
using journeyService.Models.hubspot.Api;
using journeyService.Models.inforu;
using journeyService.Models.leasing;
using journeyService.Models.tafnit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Reflection.Metadata;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace journeyService.Utils
{
    public static class DataProvider
    {
        #region ApplicationParamaters
        public static string getParamValueApplicationParamaters(string ConnectionString, string ParamCode,string ParamGroupCode)
        {
            string sql = string.Empty;
            string ParamValue = string.Empty;

            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {
                sql = $"exec [HavayatLakoah].[getParamValue_ApplicationParamaters] '{ParamCode}','{ParamGroupCode}'";


                cn.Open();


                SqlCommand cmd = new SqlCommand(sql, cn);

                SqlDataAdapter sqlDA = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();


                sqlDA.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow drow in dt.Rows)
                    {
                        ParamValue = drow["ParamValue"].ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cn.Close();
            }
            return ParamValue;

        }
        public static void UpdateApplicationParamaters(string ConnectionString, string ParamCode, string ParamGroupCode, string ParamValue)
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {

                sql = $"exec [HavayatLakoah].[update_ApplicationParamaters] '{ParamCode}','{ParamGroupCode}','{ParamValue}'";
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }
        #endregion
        #region howazit
        public static howazitHeader DeliveryCarAvis(howazitorderAvis _howazitorderAvis,
            bool istestEnvironment, 
            IConfiguration _config, 
            IOptions<ParamsSetting> paramssetting)
        {
            

            howazitHeader howazitHeader = new howazitHeader();
            try
            {

                howazitHeader.FirstName = _howazitorderAvis.shemlakoah;
                howazitHeader.LastName = "";

                howazitHeader.Allowdivur = _howazitorderAvis.acceptdivurlak;

                howazitHeader.Phone = istestEnvironment
                                    ? paramssetting.Value.phoneTest ?? ""
                                    : _howazitorderAvis.telephone1;

                howazitHeader.Email = istestEnvironment
                            ? paramssetting.Value.mailTest ?? ""
                            : _howazitorderAvis.email ?? "";

                howazitHeader.ExternalBranchId = _howazitorderAvis.agreement_signing_location_kod;
    
                howazitHeader.Attributes.Add(new Attributes("יצרן", _howazitorderAvis.teur_yazran_al));
                howazitHeader.Attributes.Add(new Attributes("s_Degem_al", _howazitorderAvis.teur_degem_al));
                howazitHeader.Attributes.Add(new Attributes("s_Rishuy", _howazitorderAvis.car_no));
                howazitHeader.Attributes.Add(new Attributes("s_hand", _howazitorderAvis.sale_type == 3 ? "חדש 0 קמ" : "משומש"));
                howazitHeader.Attributes.Add(new Attributes("s_eroa_num", _howazitorderAvis.misparerua));
                howazitHeader.Attributes.Add(new Attributes("s_Iska_type", _howazitorderAvis.deal_type_desc));
                
                howazitHeader.Attributes.Add(new Attributes("s_Mesira_date",
                    (_howazitorderAvis.car_delivery_date ?? null) == null ? "" :
                        General.formatStringToDateFormat((_howazitorderAvis.car_delivery_date ?? ""), "dd/MM/yyyy")
                    ));
               
                howazitHeader.Attributes.Add(new Attributes("s_eroa_num", _howazitorderAvis.misparerua));
                howazitHeader.Attributes.Add(new Attributes("שם ישות מוערכת", _howazitorderAvis.agreementsigning_empnname));
                howazitHeader.Attributes.Add(new Attributes("s_seller_code", _howazitorderAvis.agreement_signing_location_kod));
                





            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                
            }


            return howazitHeader;
        }

        public static howazitHeader DeliveryCarUmi(string ConnectionString, int codeAction, string OrderNumber,
            bool istestEnvironment, IConfiguration _config,IOptions<ParamsSetting> paramssetting)
        {
            string sql = string.Empty;

            howazitHeader howazitHeader = new howazitHeader();


            OdbcConnection cn = new OdbcConnection(ConnectionString);

            try
            {




                sql = "SELECT DISTINCT";
                sql = sql + " SER.SL_LAKOAH.LAKOAH, SER.SL_LAKOAH.SHEM_LAKOAH, SER.SL_LAKOAH.TELEFON, SER.SL_LAKOAH.TELEFON_NOSAF, SER.SL_LAKOAH.LAKOAH_MAIL, SER.SL_LAKOAH.MIKUD, SER.SL_LAKOAH.REHOV,";
                sql = sql + "SER.SL_LAKOAH.IR, SER.SL_LAKOAH.MISPAR_BAIT, SER.SL_LAKOAH.ALLOW_DIVUR, SERVICE.SER_MAIN.P_NUM, SERVICE.SER_MAIN.TR_HAKAMA, SERVICE.SER_MAIN.BAKASHT_LAKOH, SERVICE.SER_MAIN.SIBAT_PNIA,";
                sql = sql + "Sales.CORDERCRM.OrderNumber, SQLUser.CH_AZMANOT_LAKOAH.KOD_DEGEM, SQLUser.CH_AZMANOT_LAKOAH.SHEM_DEGEM, SQLUser.CH_AZMANOT_LAKOAH.SHANA_IZUR, SQLUser.CH_AZMANOT_LAKOAH.YAZRAN,";
                sql = sql + "SQLUser.CH_AZMANOT_LAKOAH.COLOR_MLM, SQLUser.CH_AZMANOT_LAKOAH.MEFITZ_NAME, SQLUser.CH_AZMANOT_LAKOAH.STSTUS_AZMANA, Sales.CORDERCRM.CodeMinistryOfTransport,";
                sql = sql + "Sales.CORDERCRM.HazaaNumber, Sales.CORDERCRM.CreatedDate, SQLUser.CH_AZMANOT_LAKOAH.TAARIH_BITUL_AZMANA, SQLUser.CH_AZMANOT_LAKOAH.HORAT_TASHLOM_DATE, SER.SL_LAKOAH.MISPAR_ZEHUT,";
                sql = sql + "SQLUser.CH_AZMANOT_LAKOAH.MOCER_NAME, SQLUser.CH_AZMANOT_LAKOAH.MISPAR_MOHER, SQLUser.CH_AZMANOT_LAKOAH.TRMSIR_MEUAD, SQLUser.CH_AZMANOT_LAKOAH.KOD_LAKOAH,";
                sql = sql + "SQLUser.CH_AZMANOT_LAKOAH.FinalPriceCar, SQLUser.CH_AZMANOT_LAKOAH.PriceWithoutVT, Sales.CORDERCRM.CreatedBy, SQLUser.CH_AZMANOT_LAKOAH.SIL, SERVICE.SER_MAIN.MUSACH_garage_Name,";
                sql = sql + "SQLUser.CH_AZMANOT_LAKOAH.Selling_Mail, SQLUser.CH_AZMANOT_LAKOAH.MISPAR_SOHEN, CAR.CORDERPM.HEV, CAR.CORDERPM.HODESH_MESIRA, CAR.CORDERPM.YOM_MESIRA,";
                sql = sql + "SQLUser.CH_AZMANOT_LAKOAH.RISH,SQLUser.CH_AZMANOT_LAKOAH.TEUR_DEGEM_mish,SQLUser.CH_AZMANOT_LAKOAH.YAZRAN_MISHNA, SQLUser.CH_AZMANOT_LAKOAH.YAZRAN_MISHNA_DESC";
                sql = sql + " FROM            Sales.CORDERCRM, SQLUser.CH_AZMANOT_LAKOAH, SER.SL_LAKOAH, SERVICE.SER_MAIN, CAR.CORDERPM";
                sql = sql + " WHERE        Sales.CORDERCRM.HEV = SQLUser.CH_AZMANOT_LAKOAH.HEV AND Sales.CORDERCRM.OrderNumber = SQLUser.CH_AZMANOT_LAKOAH.MISPAR_AZMANA AND Sales.CORDERCRM.SlakCalc = SER.SL_LAKOAH.LAKOAH AND";
                sql = sql + " Sales.CORDERCRM.HEV = SER.SL_LAKOAH.HEV AND Sales.CORDERCRM.HEV = SERVICE.SER_MAIN.HEV AND Sales.CORDERCRM.EventCRM = SERVICE.SER_MAIN.P_NUM AND";
                sql = sql + " SQLUser.CH_AZMANOT_LAKOAH.HEV = CAR.CORDERPM.HEV AND SQLUser.CH_AZMANOT_LAKOAH.MISPAR_AZMANA = CAR.CORDERPM.MISPAR_AZMANA AND(Sales.CORDERCRM.OrderNumber = " + OrderNumber + ") AND";
                sql = sql + " (CAR.CORDERPM.HEV = 1) AND(CAR.CORDERPM.HODESH_MESIRA > '1901') AND(CAR.CORDERPM.SHNAT_IZUR BETWEEN 18 AND 40)";



                cn.Open();


                OdbcCommand cmd = new OdbcCommand(sql, cn);

                OdbcDataAdapter sqlDA = new OdbcDataAdapter(cmd);
                DataTable dt = new DataTable();

                sqlDA.Fill(dt);

                if (dt.Rows.Count > 0)
                {


                    foreach (DataRow drow in dt.Rows)
                    {

                        howazitHeader.FirstName = drow["SHEM_LAKOAH"].ToString() ?? "";
                        howazitHeader.LastName = "";
                        howazitHeader.Allowdivur = drow["ALLOW_DIVUR"] == DBNull.Value
                            ? false
                            : General.formatStringToBoolean(drow["ALLOW_DIVUR"].ToString() ?? "");
                        
                        howazitHeader.Phone = istestEnvironment
                                    ? paramssetting.Value.phoneTest ?? ""
                                    : General.ChoosePhone(drow["TELEFON"].ToString() ?? "", drow["TELEFON_NOSAF"].ToString() ?? "");

                        howazitHeader.Email = istestEnvironment
                                    ? paramssetting.Value.mailTest ?? ""
                                    : drow["LAKOAH_MAIL"].ToString() ?? "";


                        
                        howazitHeader.ExternalBranchId = drow["MISPAR_SOHEN"].ToString() ?? "";
                        howazitHeader.Attributes.Add(new Attributes("s_order_number", drow["OrderNumber"].ToString() ?? ""));
                        //howazitHeader.Attributes.Add(new Attributes("s_seller_name", drow["MOCER_NAME"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("שם ישות מוערכת", drow["MOCER_NAME"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("s_seller_code", string.IsNullOrEmpty(drow["CreatedBy"].ToString()) ? "-1" : drow["CreatedBy"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("s_Degem_al", drow["SHEM_DEGEM"].ToString() ?? ""));
                        
                        

                        string HODESH_MESIRA = drow["HODESH_MESIRA"].ToString() ?? "";
                        string YOM_MESIRA = drow["YOM_MESIRA"].ToString() ?? "";

                        //source : 1908 format to 2019-08-
                        string monthStr = (HODESH_MESIRA.Substring(0, 1) == "9")
                                     ? "19" + HODESH_MESIRA.Substring(0, 2) + "-"
                                     : "20" + HODESH_MESIRA.Substring(0, 2) + "-"
                                     + HODESH_MESIRA.Substring(2, 2) + "-";

                        //source : 20
                        string dayStr = (YOM_MESIRA.Substring(0, 1) == "0")
                                     ? YOM_MESIRA
                                     : (int.Parse(YOM_MESIRA) < 10 && YOM_MESIRA.Substring(0, 1) != "0")
                                     ? "0" + YOM_MESIRA
                        : YOM_MESIRA;
            
                        howazitHeader.Attributes.Add(new Attributes("s_Mesira_date", General.formatStringToDateFormat(monthStr + dayStr, "dd/MM/yyyy")));
                        
                        //howazitHeader.Attributes.Add(new Attributes("s_Mesira_date", (DateTime.Parse(monthStr + dayStr)).ToString("dd/MM/yyyy")));

                        howazitHeader.Attributes.Add(new Attributes("יצרן", (drow["YAZRAN_MISHNA_DESC"].ToString() ?? "")
                                .Replace("שברולט מסחרי", "שברולט").Replace("שברולט פרטי", "שברולט")
                                ));



                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cn.Close();
            }


            return howazitHeader;
        }


        public static howazitHeader Satisfaction(string ConnectionString, int codeAction, 
            string Lakuah, string MisparErua,string ExternalBranchId,
            bool istestEnvironment, IConfiguration _config, IOptions<ParamsSetting> paramssetting)
        {
            string sql = string.Empty;

            howazitHeader howazitHeader = new howazitHeader();


            OdbcConnection cn = new OdbcConnection(ConnectionString);

            try
            {




                sql = "SELECT ser.eruamapping.misparerua,";
                sql = sql + "ser.sl_lakoah.shem_lakoah,";
                sql = sql + "ser.sl_lakoah.lakoah_mail,";
                sql = sql + "ser.sl_lakoah.telefon,";
                sql = sql + "ser.sl_lakoah.telefon_nosaf,";
                sql = sql + "ser.eruamapping.shibtec,";
                sql = sql + "dwh.employeecls.NAME,";
                sql = sql + "ser.sl_lakoah.teur_sivug AS ksivugteur,";
                sql = sql + "ser.eruamapping.shem_ofenp AS shemtzinor,";
                sql = sql + "ser.eruamapping.teur_nose_erua AS teurnoseerua,";
                sql = sql + "ser.eruamapping.teur_degem AS teurdegem,";
                sql = sql + "ser.eruamapping.shem_ish_kesher AS shemishkesher,";
                sql = sql + "ser.eruamapping.zman_sgira AS zmansgira,";
                sql = sql + "CASE";
                sql = sql + " WHEN ser.eruamapping.code_mark_customer = 1 THEN 'כן'";
                sql = sql + " ELSE 'לא'";
                sql = sql + " END AS s_VIP,";
                sql = sql + "Datediff(d, ser.eruamapping.taarihptiha, Getdate()) AS s_amida,";
                sql = sql + "ser.eruamapping.teur_sug_erua AS teursugerua,";
                sql = sql + "ser.eruamapping.mlk,";
                sql = sql + "ser.sl_lakoah.allow_divur AS AcceptDivurLak,";
                sql = sql + "ser.sl_lakoah.lakoah,";
                sql = sql + "ser.eruamapping.rish";
                sql = sql + " FROM   ser.sl_lakoah,";
                sql = sql + " { oj SER.EruaMapping LEFT OUTER JOIN DWH.EmployeeCls ON SER.EruaMapping.ShibTec = DWH.EmployeeCls.EmployeeID }";
                sql = sql + " WHERE ser.eruamapping.hev = ser.sl_lakoah.hev";
                sql = sql + " AND ser.eruamapping.mlk = ser.sl_lakoah.lakoah";
                sql = sql + " AND(ser.eruamapping.misparerua = '" + MisparErua + "')";
                sql = sql + " AND(ser.sl_lakoah.lakoah = '" + Lakuah + "')";



                cn.Open();


                OdbcCommand cmd = new OdbcCommand(sql, cn);

                OdbcDataAdapter sqlDA = new OdbcDataAdapter(cmd);
                DataTable dt = new DataTable();

                sqlDA.Fill(dt);

                if (dt.Rows.Count > 0)
                {


                    foreach (DataRow drow in dt.Rows)
                    {

                        howazitHeader.FirstName = drow["shem_lakoah"].ToString() ?? "";
                        howazitHeader.LastName = "";
                        
                        howazitHeader.Phone = istestEnvironment
                                    ? paramssetting.Value.phoneTest ?? ""
                                    : General.ChoosePhone(drow["telefon"].ToString() ?? "", drow["telefon_nosaf"].ToString() ?? "");

                        howazitHeader.Email = istestEnvironment
                                    ? paramssetting.Value.mailTest ?? ""
                                    : drow["lakoah_mail"].ToString() ?? "";

                        string teursugerua = drow["teursugerua"].ToString() ?? "";

                        howazitHeader.ExternalBranchId = (teursugerua.Contains("שירות לקוחות-אופיס דיפו") ? "OfficeDepot" : ExternalBranchId);

                        //"טלפוני"
                        howazitHeader.Attributes.Add(new Attributes("s_pnia_type", drow["shemtzinor"].ToString() ?? ""));
                        //"כספים"
                        howazitHeader.Attributes.Add(new Attributes("s_reason", drow["teurnoseerua"].ToString() ?? ""));
                        //"בקשת חשבוניות"
                        howazitHeader.Attributes.Add(new Attributes("s_subject", drow["teurdegem"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("s_Rishuy", drow["rish"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("s_client_name", drow["shem_lakoah"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("s_driver_name", drow["shemishkesher"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("s_eroa_num", drow["misparerua"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("s_VIP", "לא"));
                        
                        //"שרות לקוחות-ליסינג אקספרס"
                        howazitHeader.Attributes.Add(new Attributes("s_transaction_type", teursugerua));
                        /*שם גורם מטפל*/
                        //"יונתן וטיס"
                        howazitHeader.Attributes.Add(new Attributes("s_id_rep", drow["shibtec"].ToString() ?? ""));
                        /*קוד גורם מטפל*/
                        //"40014390"
                        howazitHeader.Attributes.Add(new Attributes("שם ישות מוערכת", drow["NAME"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("s_cust_type", drow["ksivugteur"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("s_amida", drow["s_amida"].ToString() ?? ""));
                        howazitHeader.Attributes.Add(new Attributes("s_close_date", 
                            drow["zmansgira"] == DBNull.Value ? ""
                            : General.formatStringToDateFormat(drow["zmansgira"].ToString() ?? "", "u")
                            ));
                       }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cn.Close();
            }


            return howazitHeader;
        }

        #endregion
        #region InforULog
        public static int InsertinforULogUrlRequet(string ConnectionString, string UrlRequet,
          int codeAction, string ApiEventName, string UrlParam1 = "", string UrlParam2 = ""
          , DateTime? fromdate = null, DateTime? todate = null, int parentlogID = 0)
        {
            int rowsAffected = -1;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Insert into inforULog (UrlRequet,codeAction,ServiceName,UrlParam1,UrlParam2,fromdate,todate,parentlogID)  values(@UrlRequet,@codeAction,@ApiEventName,@UrlParam1,@UrlParam2,@fromdate,@todate,@parentlogID) SELECT SCOPE_IDENTITY() ", con))
                    {

                        cmd.Parameters.AddWithValue("@UrlRequet", UrlRequet);
                        cmd.Parameters.AddWithValue("@codeAction", codeAction);
                        cmd.Parameters.AddWithValue("@ApiEventName", ApiEventName == "" ? (object)DBNull.Value : ApiEventName);
                        cmd.Parameters.AddWithValue("@UrlParam1", UrlParam1 == "" ? (object)DBNull.Value : UrlParam1);
                        cmd.Parameters.AddWithValue("@UrlParam2", UrlParam2 == "" ? (object)DBNull.Value : UrlParam2);
                        cmd.Parameters.AddWithValue("@fromdate", fromdate == null ? (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue : fromdate);
                        cmd.Parameters.AddWithValue("@todate", todate == null ? (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue : todate);
                        cmd.Parameters.AddWithValue("@parentlogID", parentlogID == 0 ? (object)DBNull.Value : parentlogID);

                        rowsAffected = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
            return rowsAffected;




        }

        public static void UpdateinforULogError(string ConnectionString, int logID, string ErrorMsg)
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {
                //sql = "Update [HavayatLakoah].[HubSpotImportLog] Set ErrorMsgDate = getdate(), " +
                sql = "Update inforULog Set ErrorMsgDate = getdate(), " +
                    "ErrorMsg = '" + General.EscapeSpecialCharacters(ErrorMsg) + "' " +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }

        public static void UpdateinforULoghowazitRequest(string ConnectionString, int logID,
            howazitHeader payload, string jsonData,string serviceName)
        {



            string sql = string.Empty;

            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {
                if(serviceName == "howazit-DeliveryCarUmi")
                    sql = "Update inforULog Set RequestStrSecondDate = getdate()," +
                        "IshurDivur = " + (payload.Allowdivur ? 1 : 0) + "," +
                        "UrlParam1 = '" + payload.Attributes[0].Value + "'," +
                        "RequestStrSecond = '" + General.EscapeSpecialCharacters(jsonData) + "'," +
                        "DescriptionDgm = '" + General.EscapeSpecialCharacters(payload.Attributes[3].Value) + "'," +
                        "customerName = '" + General.EscapeSpecialCharacters(payload.FirstName) + "'," +
                        "customerNamePhone = '" + payload.Phone + "'," +
                        "customerEmail = '" + payload.Email + "'," +
                        "EmployeeCode = '" + payload.Attributes[2].Value + "'," +
                        "EmployeeName = '" + General.EscapeSpecialCharacters(payload.Attributes[1].Value) + "'," +
                        "BranchId = '" + payload.ExternalBranchId + "'," +
                        "car_delivery_date = '" + General.formatStringToDateFormat(payload.Attributes[4].Value, "yyyy-MM-dd") + "'," +
                        "DescriptionMan = '" + payload.Attributes[5].Value + "'" +
                         " where logID = " + logID;
                else if (serviceName == "howazit-Satisfaction")
                {
                    sql = "Update inforULog Set RequestStrSecondDate = getdate()," +
                        "RequestStrSecond = '" + General.EscapeSpecialCharacters(jsonData) + "'," +
                        "EventNum = '" + payload.Attributes[6].Value + "'," +
                        "carid = '" + payload.Attributes[3].Value + "'," +
                        "customerName = '" + General.EscapeSpecialCharacters(payload.FirstName) + "'," +
                        "customerNamePhone = '" + payload.Phone + "'," +
                        "customerEmail = '" + payload.Email + "'," +
                        "EmployeeCode = '" + payload.Attributes[9].Value + "'," +
                        "EmployeeName = '" + General.EscapeSpecialCharacters(payload.Attributes[10].Value) + "'" +
                        " where logID = " + logID;
                }
                else if (serviceName == "howazit-DeliveryCarAvis")
                {
                    sql = "Update inforULog Set RequestStrSecondDate = getdate()," +
                       "IshurDivur = " + (payload.Allowdivur ? 1 : 0) + "," +
                       "RequestStrSecond = '" + General.EscapeSpecialCharacters(jsonData) + "'," +
                       "customerName = '" + General.EscapeSpecialCharacters(payload.FirstName) + "'," +
                        "customerNamePhone = '" + payload.Phone + "'," +
                        "customerEmail = '" + payload.Email + "'," +
                        "DescriptionDgm = '" + General.EscapeSpecialCharacters(payload.Attributes[1].Value) + "'," +
                        "BranchId = '" + payload.ExternalBranchId + "'," +
                        "car_delivery_date = '" + General.formatStringToDateFormat(payload.Attributes[6].Value, "yyyy-MM-dd") + "'," +
                        "DescriptionMan = '" + payload.Attributes[0].Value + "'," +
                        "EmployeeCode = '" + payload.Attributes[9].Value + "'," +
                        "EmployeeName = '" + General.EscapeSpecialCharacters(payload.Attributes[8].Value) + "'" +
                       " where logID = " + logID;
                }



                    cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }

        public static void UpdateinforULogResponse(string ConnectionString,
            int logID,
            string jsonResponseContent,
            int StatusCode,
            bool howazitAccepted = false,
            string howazitmessage = ""

            )
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {

                sql = "Update inforULog Set ResponseStrSecondDate = getdate()," +
                    "ResponseStrSecond = '" + General.EscapeSpecialCharacters(jsonResponseContent) + "'," +
                    "StatusCode = " + StatusCode + "," +
                    "howazitAccepted = " + (howazitAccepted ? 1 : 0) + "," +
                    "howazitmessage = '" + General.EscapeSpecialCharacters(howazitmessage) + "'" +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }

        public static void UpdateinforULogFlycardRequestToken(string ConnectionString, int logID,
            tokenRequest payload, string jsonData)
        {



            string sql = string.Empty;

            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {
                
                    sql = "Update inforULog Set RequestTokenDate = getdate()," +

                        "RequestToken = '" + General.EscapeSpecialCharacters(jsonData) + "'" +

                         " where logID = " + logID;
              



                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }

        public static void UpdateinforULogFlycardRequest(string ConnectionString, int logID,
            tokenRequest payload, string jsonData)
        {



            string sql = string.Empty;

            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {

                sql = "Update inforULog Set RequestStrSecondDate = getdate()," +

                    "RequestStrSecond = '" + General.EscapeSpecialCharacters(jsonData) + "'" +

                     " where logID = " + logID;




                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }

        public static void UpdateinforULogFlycardResponseToken(string ConnectionString,
          int logID,
          string jsonResponseContent,
          int StatusCode,
          string message = ""

          )
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {

                sql = "Update inforULog Set ResponseTokenDate = getdate()," +
                    "ResponseToken = '" + General.EscapeSpecialCharacters(jsonResponseContent) + "'," +
                    "ErrorTokenMsg = '" + General.EscapeSpecialCharacters(message) + "'" +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }
        public static void UpdateinforULogFlycardReminderResponse(string ConnectionString,
          int logID,
          string jsonResponseContent,
          int StatusCode,
          populationForReminderResponse _populationForReminderResponse
          
          )
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {

                sql = "Update inforULog Set ResponseStrSecondDate = getdate()," +
                    "ResponseStrSecond = '" + General.EscapeSpecialCharacters(jsonResponseContent) + "'," +
                    "StatusCode = " + StatusCode + "," +
                    "numRecordToUpload = " + _populationForReminderResponse.results.Count + "," +
                    "ErrorMsg = '" + General.EscapeSpecialCharacters(_populationForReminderResponse.message) + "'" +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }

        public static void UpdateinforULogFlycardReminderPaymentResponse(string ConnectionString,
          int logID,
          string jsonResponseContent,
          int StatusCode,
          populationForPaymentReminderResponse _populationForPaymentReminderResponse

          )
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {

                sql = "Update inforULog Set ResponseStrSecondDate = getdate()," +
                    "ResponseStrSecond = '" + General.EscapeSpecialCharacters(jsonResponseContent) + "'," +
                    "StatusCode = " + StatusCode + "," +
                    "numRecordToUpload = " + _populationForPaymentReminderResponse.results.Count + "," +
                    "ErrorMsg = '" + General.EscapeSpecialCharacters(_populationForPaymentReminderResponse.message) + "'" +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }

        
        #endregion
        #region LeasingContractRenew
        public static int InsertLeasingContractRenewUrlRequet(string ConnectionString, string ReqeustUrl,
            tafnitRenewDealRoute payload, string serviceName)
        {
            int rowsAffected = -1;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Insert into LeasingContractRenew_Row_WsTafnit (logID_inforULog,logID_LeasingContractRenew_Row,step,HpNo,DealNo,serviceName,ReqeustUrl)  values(@logID_inforULog,@logID_LeasingContractRenew_Row,@step,@HpNo,@DealNo,@serviceName,@ReqeustUrl) SELECT SCOPE_IDENTITY() ", con))
                    {

                        cmd.Parameters.AddWithValue("@logID_inforULog", payload.LogIdInforU);
                        cmd.Parameters.AddWithValue("@logID_LeasingContractRenew_Row", payload.LogIdLeasingContractRenew);
                        cmd.Parameters.AddWithValue("@step", payload.step);
                        cmd.Parameters.AddWithValue("@HpNo", payload.hpno);
                        cmd.Parameters.AddWithValue("@DealNo", payload.dealno);
                        cmd.Parameters.AddWithValue("@serviceName", serviceName);
                        cmd.Parameters.AddWithValue("@ReqeustUrl", ReqeustUrl);
                        rowsAffected = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
            return rowsAffected;
        }
        

        public static void UpdateLeasingContractRenewError(string ConnectionString, int id,
            tafnitRenewDealRoute payload, string ErrorMsg)
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {
                sql = "Update LeasingContractRenew_Row_WsTafnit Set " +
                    "errormsg = '" + General.EscapeSpecialCharacters(ErrorMsg) + "' " +
                    " where id = " + id;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }

        public static void UpdateLeasingContractRenewResponse(string ConnectionString,
            int id,tafnitRenewDealRoute payload,
            string jsonResponseContent,int StatusCode
            ,int Success,int Allowed,string Error,string NotAllowedReason)
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);

            
            

            try
            {

                sql = "Update LeasingContractRenew_Row_WsTafnit Set " +
                    "ResponseUrl = N'" + General.EscapeSpecialCharacters(jsonResponseContent) + "'," +
                    "statuscode = " + StatusCode + "," +
                    "Success = " + Success + "," +
                    "Allowed = " + Allowed + "," +
                    "Error = N'" + General.EscapeSpecialCharacters(Error) + "'," +
                    "NotAllowedReason = N'" + General.EscapeSpecialCharacters(NotAllowedReason) + "'" +
                    " where id = " + id;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateLeasingContractRenewError(ConnectionString, id,payload, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }
        
        #endregion
        #region tafnitRemoveDivurByPhoneOrEmail
        public static int InsertTafnitUnsubscribeRequet(string ConnectionString,
            int codeAction,
            int logID_HubSpotImportLog,
            string UrlRequet,
            string serviceName,
            string propertyValue1,
            string unit)
        {
            //
            int rowsAffected = -1;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Insert into [HavayatLakoah].HubSpotImportLog_Unsubscribe (codeAction,logID_HubSpotImportLog,UrlRequet,serviceName,propertyValue1,unit)  values(@codeAction,@logID_HubSpotImportLog,@UrlRequet,@serviceName,@propertyValue1,@unit) SELECT SCOPE_IDENTITY() ", con))
                    {
                        cmd.Parameters.AddWithValue("@codeAction", codeAction);
                        cmd.Parameters.AddWithValue("@logID_HubSpotImportLog", logID_HubSpotImportLog);
                        cmd.Parameters.AddWithValue("@serviceName", serviceName);
                        cmd.Parameters.AddWithValue("@UrlRequet", UrlRequet);
                        cmd.Parameters.AddWithValue("@propertyValue1", propertyValue1);
                        cmd.Parameters.AddWithValue("@unit", unit);

                        rowsAffected = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
            return rowsAffected;
        }

        public static void UpdateTafnitUnsubscribeResponse(string ConnectionString,
            int logID,
            tafnitRemoveDivurResponse _tafnitRemoveDivurResponse
            )
        {
            
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            string jsonResponseContent = JsonConvert.SerializeObject(_tafnitRemoveDivurResponse); ;
            try
            {



                sql = "Update [HavayatLakoah].[HubSpotImportLog_Unsubscribe] Set " +
                        "ResponseStr = '" + General.EscapeSpecialCharacters(jsonResponseContent) + "'," +
                        "StatusDesc = '" + General.EscapeSpecialCharacters(_tafnitRemoveDivurResponse.StatusDesc) + "'," +
                        "StatusCode = " + _tafnitRemoveDivurResponse.StatusCode + "" + 
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }
        #endregion
        #region HubSpotImport
        public static int InsertHubSpotImportRequest(string ConnectionString,
            string UrlRequet, int codeAction, string ServiceName



            )
        {

            int rowsAffected = -1;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Insert into [HavayatLakoah].[HubSpotImportLog] (UrlRequet,codeAction,ServiceName)  values(@UrlRequet,@codeAction,@ServiceName) SELECT SCOPE_IDENTITY() ", con))
                    {

                        cmd.Parameters.AddWithValue("@UrlRequet", UrlRequet);
                        cmd.Parameters.AddWithValue("@codeAction", codeAction);
                        cmd.Parameters.AddWithValue("@ServiceName", ServiceName == "" ? (object)DBNull.Value : ServiceName);


                        rowsAffected = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
            return rowsAffected;


        }
        public static int InsertHubSpotImportRequest(string ConnectionString,
            string UrlRequet, int codeAction, string ServiceName, DateTime fromdate,
            DateTime todate



            )
        {

            int rowsAffected = -1;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Insert into [HavayatLakoah].[HubSpotImportLog] (UrlRequet,codeAction,ServiceName,fromdate,todate)  values(@UrlRequet,@codeAction,@ServiceName,@fromdate,@todate) SELECT SCOPE_IDENTITY() ", con))
                    {
                        //
                        cmd.Parameters.AddWithValue("@UrlRequet", UrlRequet);
                        cmd.Parameters.AddWithValue("@codeAction", codeAction);
                        cmd.Parameters.AddWithValue("@ServiceName", ServiceName == "" ? (object)DBNull.Value : ServiceName);
                        cmd.Parameters.AddWithValue("@fromdate", fromdate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@todate", todate.ToString("yyyy-MM-dd"));


                        rowsAffected = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
            return rowsAffected;


        }

        public static void UpdateHubSpotImportError(string ConnectionString,
            int logID, string ErrorMsg)
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {
                sql = "Update inforULog Set ErrorMsgDate = getdate(), " +
                    "ErrorMsg = '" + General.EscapeSpecialCharacters(ErrorMsg) + "' " +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }


        public static void UpdateHubSpotImportResponse(string ConnectionString,
            int logID,
            string jsonResponseContent,
            hubspotResponse _hubspotResponse
            )
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {



                sql = "Update [HavayatLakoah].[HubSpotImportLog] Set " +
                    "ResponseStr = '" + General.EscapeSpecialCharacters(jsonResponseContent) + "'," +
                    "accepted = " + (_hubspotResponse.accepted ? 1 : 0) + "," +
                    "ErrorMsg = '" + General.EscapeSpecialCharacters(_hubspotResponse.errorDescription) + "'," +
                    "state = '" + _hubspotResponse._hubspotResponseImport.state + "'," +
                    "importName = '" + _hubspotResponse._hubspotResponseImport.importName + "'," +
                    "optOutImport = " + (_hubspotResponse._hubspotResponseImport.optOutImport ? 1 : 0) + "," +
                    "id = '" + _hubspotResponse._hubspotResponseImport.id + "'," +
                    "estimatedLineCount = " + _hubspotResponse._hubspotResponseImport.estimatedLineCount + "" +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }

        public static void UpdateHubSpotImportResponse(string ConnectionString,
            int logID,
            string jsonResponseContent
            )
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {



                sql = "Update [HavayatLakoah].[HubSpotImportLog] Set " +
                        "ResponseStr = '" + General.EscapeSpecialCharacters(jsonResponseContent) + "'" +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }



        public static void UpdateHubSpotImportRequest(string ConnectionString,
            int logID,
            string jsonData,
            string UrlExternal)
        {



            string sql = string.Empty;

            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {

                sql = "Update [HavayatLakoah].[HubSpotImportLog]  Set " +
                        "RequestStr = '" + General.EscapeSpecialCharacters(jsonData) + "'," +
                        "UrlExternal = '" + UrlExternal + "'" +
                         " where logID = " + logID;




                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }


        public static void UpdateHubSpotImportResponse(string ConnectionString,
           int logID,
           UnsubscribeListResponse _UnsubscribeListResponse
           )
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {

                string jsonResponseContent = JsonConvert.SerializeObject(_UnsubscribeListResponse); ;

                sql = "Update [HavayatLakoah].[HubSpotImportLog] Set " +
                    "ResponseStr = '" + General.EscapeSpecialCharacters(jsonResponseContent) + "'," +
                    "StatusDescription = '" + General.EscapeSpecialCharacters(_UnsubscribeListResponse.StatusDescription) + "'," +
                    "estimatedLineCount = " + ((_UnsubscribeListResponse.Data == null) ? 0 : _UnsubscribeListResponse.Data.Count) + "," +
                    "StatusId = " + _UnsubscribeListResponse.StatusId + "" +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }
        public static List<UmiCars> GetUmiCars(string ConnectionString, string filterdate)
        {
            string sql = string.Empty;
            List<UmiCars> _UmiCars = new List<UmiCars>();

            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {

             





                //sql = $"exec [HavayatLakoah].[GET_UMI_CARS] '{filterdate}'";
                //sql = "exec [HavayatLakoah].[GET_UMI_CARS] '2025-04-05 00:00:00.000'";
                //sql = "exec [HavayatLakoah].[GET_UMI_CARS] NULL";

                sql = $"exec [HavayatLakoah].[GET_CARS]";

                




                cn.Open();


                SqlCommand cmd = new SqlCommand(sql, cn);

                SqlDataAdapter sqlDA = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();


                sqlDA.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow drow in dt.Rows)
                    {
                        UmiCars _UmiCar = new UmiCars();

                        _UmiCar.clientid = drow["Client ID"].ToString() ?? "";
                        //alt + ctr + o print data
                        //System.Diagnostics.Debug.WriteLine(_UmiCar.clientid);
                        _UmiCar.firstname = General.EscapeSpecialCharacters(drow["First Name"].ToString() ?? "");
                        _UmiCar.lastname = drow["Last name"].ToString() ?? "";
                        _UmiCar.email = General.EscapeSpecialCharacters(drow["Email"].ToString() ?? "");
                        //מייל לא תקין מאפס כי לא נפתחת רשומה ב HUBSPOT
                        if (drow["Email"].ToString()  != "")
                        {
                            if (!General.IsLikelyValidEmail(drow["Email"].ToString() ?? ""))
                            {
                                //Console.WriteLine("no ok = " + drow["Email"].ToString());
                                _UmiCar.email = "";
                            }
                            

                        }
                            
                        

                        

                        _UmiCar.phone = drow["Phone"].ToString() ?? "";
                        _UmiCar.manufactureyear = int.Parse(drow["Manufacture Year"].ToString() ?? "0");
                        _UmiCar.newregistrationplate = drow["Registration plate"].ToString() ?? "";
                        _UmiCar.carmodel = General.EscapeSpecialCharacters(drow["car_model"].ToString() ?? "");
                        _UmiCar.manufacturer = General.EscapeSpecialCharacters(drow["manufacturer"].ToString() ?? "");
                        _UmiCar.lasttreatment_date = (drow["last_treatment_date"] == DBNull.Value) ? "" : (DateTime.Parse((drow["last_treatment_date"].ToString() ?? ""))).ToString("yyyy-MM-dd"); ;
                        _UmiCar.cardeliverydate = (drow["car_delivery_date"] == DBNull.Value) ? "" : (DateTime.Parse((drow["car_delivery_date"].ToString() ?? ""))).ToString("yyyy-MM-dd"); ;
                        _UmiCar.allowdivur = int.Parse(drow["ALLOW_DIVUR"].ToString() ?? "0");
                        _UmiCar.kodsalesbranch = int.Parse(drow["kod_sales_branch"].ToString() ?? "0");
                        _UmiCar.salesbranch = General.EscapeSpecialCharacters(drow["sales_branch"].ToString() ?? "");
                        _UmiCar.lastgaragecode = int.Parse(drow["Last_garage_code"].ToString() ?? "0");
                        _UmiCar.lastgaragename = General.EscapeSpecialCharacters(drow["Last_garage_name"].ToString() ?? "");
                        _UmiCar.associationlable = drow["association_lable"].ToString() ?? "";
                        _UmiCar.dateidkunbaalut = (drow["DateidKunBaalut"] == DBNull.Value) ? "" : (DateTime.Parse((drow["DateidKunBaalut"].ToString() ?? ""))).ToString("yyyy-MM-dd"); ;
                        //_UmiCar.registrationplate = Utils.General.EncryptString("UmiCars2024=====", "clientid=" + _UmiCar.clientid + "&carid=" + _UmiCar.newregistrationplate).Replace("/","~").Replace("+", "!");
                        _UmiCar.registrationplate = Utils.General.EncryptString("UmiCars2024=====", "carid=" + _UmiCar.newregistrationplate).Replace("/", "~").Replace("+", "!");
                        _UmiCar.contact_association_lable = drow["contact_association_lable"].ToString() ?? "";
                        _UmiCar.date_car_test = drow["date_car_test"].ToString() ?? "";
                        _UmiCar.allow_divur_person = int.Parse(drow["ALLOW_DIVUR"].ToString() ?? "0");
                        _UmiCar.ops__tafnit_unit = drow["ops__tafnit_unit"].ToString() ?? "";

                        //tafnit avis
                        _UmiCar.kodmachzormechira = int.Parse(drow["kod_machzor_mechira"].ToString() ?? "-1");
                        _UmiCar.kmmechira = int.Parse(drow["kmmechira"].ToString() ?? "-1");
                        _UmiCar.kod_customerclassification = int.Parse(drow["kod_customerclassification"].ToString() ?? "-1");
                        _UmiCar.customerclassification = drow["customerclassification"].ToString() ?? "";
                        _UmiCar.agreementdate = (drow["agreementdate"] == DBNull.Value) ? "" : (DateTime.Parse((drow["agreementdate"].ToString() ?? ""))).ToString("yyyy-MM-dd"); ;




                        _UmiCars.Add(_UmiCar);


                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cn.Close();
            }
            return _UmiCars;
        }

        public static void DataProcessing(string ConnectionString,string sql)
        {
            //string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {

                //sql = $"exec [HavayatLakoah].[HUBSPOT_Data_Processing]";
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }
        #endregion
        #region HubSpot
        public static void UpdateHubspotRestResponse(string ConnectionString,
              int logID, string ResponseStr,string ResponseValue,string errormsg ,int statuscode
            )
        {
               

            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {
                sql = "Update [HavayatLakoah].[HubSpotImportLog_Rest] Set " +
                        "ResponseStr = '" + General.EscapeSpecialCharacters(ResponseStr) + "'," +
                        "StatusDesc = '" + General.EscapeSpecialCharacters(errormsg) + "'," +
                        "StatusCode = " + statuscode + "," +
                        "ResponseValue = '" + General.EscapeSpecialCharacters(ResponseValue) + "'" +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }
        public static void UpdateHubspotRestResponse(string ConnectionString,
            int logID, HbSearchCustomObjectResponse _HbSearchCustomObjectResponse
            ,string ResponseValue
            )
        {
            string sql = string.Empty;
            SqlConnection cn = new SqlConnection(ConnectionString);
            string jsonResponseContent = JsonConvert.SerializeObject(_HbSearchCustomObjectResponse); ;
            try
            {
                sql = "Update [HavayatLakoah].[HubSpotImportLog_Rest] Set " +
                        "ResponseStr = '" + General.EscapeSpecialCharacters(jsonResponseContent) + "'," +
                        "StatusDesc = '" + General.EscapeSpecialCharacters(_HbSearchCustomObjectResponse.errormsg) + "'," +
                        "StatusCode = " + _HbSearchCustomObjectResponse.statuscode + "," +
                        "ResponseValue = '" + ResponseValue + "'" +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }
        public static int InsertHubspotRestRequet(string ConnectionString,
            int codeAction,
            int logID_HubSpotImportLog,
            string UrlRequet,
            string serviceName,
            string RequestStr,
            string unit,
            string RequestValue)
        {
            //
            int rowsAffected = -1;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Insert into [HavayatLakoah].HubSpotImportLog_Rest (codeAction,logID_HubSpotImportLog,UrlRequet,serviceName,RequestStr,unit,RequestValue)  " +
                        "values(@codeAction,@logID_HubSpotImportLog,@UrlRequet,@serviceName,@RequestStr,@unit,@RequestValue) SELECT SCOPE_IDENTITY() "
                        , con))
                    {
                        cmd.Parameters.AddWithValue("@codeAction", codeAction);
                        cmd.Parameters.AddWithValue("@logID_HubSpotImportLog", logID_HubSpotImportLog);
                        cmd.Parameters.AddWithValue("@serviceName", serviceName);
                        cmd.Parameters.AddWithValue("@UrlRequet", UrlRequet);
                        cmd.Parameters.AddWithValue("@RequestStr", General.EscapeSpecialCharacters(RequestStr));
                        cmd.Parameters.AddWithValue("@unit", unit);
                        cmd.Parameters.AddWithValue("@RequestValue", RequestValue);

                        rowsAffected = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
            return rowsAffected;
        }

        public static int InsertHubSpotRequest(string ConnectionString,
            string UrlRequet, int codeAction, string ServiceName, 
            string payload
            )
        {

            // המר את כל הרשימה למחרוזת JSON אחת
            //string fullJson = JsonConvert.SerializeObject(_HbSearchCustomObjects);
            int rowsAffected = -1;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Insert into [HavayatLakoah].[HubSpotImportLog] (UrlRequet,codeAction,ServiceName,RequestStr)  " +
                        "values(@UrlRequet,@codeAction,@ServiceName,@RequestStr) SELECT SCOPE_IDENTITY() ", con))
                    {

                        cmd.Parameters.AddWithValue("@UrlRequet", UrlRequet);
                        cmd.Parameters.AddWithValue("@codeAction", codeAction);
                        cmd.Parameters.AddWithValue("@ServiceName", ServiceName == "" ? (object)DBNull.Value : ServiceName);
                        cmd.Parameters.AddWithValue("@RequestStr", General.EscapeSpecialCharacters(payload));


                        rowsAffected = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
            return rowsAffected;


        }
        public static void UpdateHubSpotResponse(string ConnectionString,
           int logID,
           HbSearchCustomObjectResponse _HbSearchCustomObjectResponse
           )
        {
            string sql = string.Empty;
            // המר את כל הרשימה למחרוזת JSON אחת
            string fullJson = JsonConvert.SerializeObject(_HbSearchCustomObjectResponse);

            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {



                sql = "Update [HavayatLakoah].[HubSpotImportLog] Set " +
                    "ResponseStr = '" + General.EscapeSpecialCharacters(fullJson) + "'," +
                    "accepted = " + (_HbSearchCustomObjectResponse.accepted ? 1 : 0) + "," +
                    "ErrorMsg = '" + General.EscapeSpecialCharacters(_HbSearchCustomObjectResponse.errormsg) + "'," +
                    "estimatedLineCount = " + _HbSearchCustomObjectResponse.total +
                    " where logID = " + logID;
                cn.Open();
                SqlCommand cmd = new SqlCommand(sql, cn);
                int rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                UpdateinforULogError(ConnectionString, logID, ex.Message);
            }
            finally
            {
                cn.Close();
            }


        }

        //public static void UpdateHubSpotRequestSecondTime(string ConnectionString,
        //   int logID,string RequestStrSecondTime,string ServiceNameSecondTime
        //   )
        //{
        //    string sql = string.Empty;
        //    // המר את כל הרשימה למחרוזת JSON אחת

        //    SqlConnection cn = new SqlConnection(ConnectionString);
        //    try
        //    {

        //        sql ="Update [HavayatLakoah].[HubSpotImportLog] Set " +
        //            "RequestStrSecondTime = '" + RequestStrSecondTime + "'," +
        //            "ServiceNameSecondTime = '" + ServiceNameSecondTime + "'" +
        //            " where logID = " + logID;
        //        cn.Open();
        //        SqlCommand cmd = new SqlCommand(sql, cn);
        //        int rowsAffected = cmd.ExecuteNonQuery();
        //    }
        //    catch (Exception ex)
        //    {
        //        UpdateinforULogError(ConnectionString, logID, ex.Message);
        //    }
        //    finally
        //    {
        //        cn.Close();
        //    }


        //}

        //public static void UpdateHubSpottResponseSecondTime(string ConnectionString,
        //   int logID, int StatusCodeSecondTime , string ResponseStrSecondTime
        //   )
        //{
        //    string sql = string.Empty;
        //    // המר את כל הרשימה למחרוזת JSON אחת


        //    SqlConnection cn = new SqlConnection(ConnectionString);
        //    try
        //    {

        //        sql = "Update [HavayatLakoah].[HubSpotImportLog] Set " +
        //            "StatusCodeSecondTime = " + StatusCodeSecondTime + "," +
        //            "ResponseStrSecondTime = '" + General.EscapeSpecialCharacters(ResponseStrSecondTime) + "'" +
        //            " where logID = " + logID;
        //        cn.Open();
        //        SqlCommand cmd = new SqlCommand(sql, cn);
        //        int rowsAffected = cmd.ExecuteNonQuery();
        //    }
        //    catch (Exception ex)
        //    {
        //        UpdateinforULogError(ConnectionString, logID, ex.Message);
        //    }
        //    finally
        //    {
        //        cn.Close();
        //    }


        //}

        public static List<UpDelAssociatedLabel> GetAssociatedChangedUmiCars(string ConnectionString)
        {
            string sql = string.Empty;
            List<UpDelAssociatedLabel> _UpDelAssociatedLabels = new List<UpDelAssociatedLabel>();

            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {
                sql = $"exec [HavayatLakoah].[Get_AssociatedThatNeedTobeChanged] ";



                cn.Open();


                SqlCommand cmd = new SqlCommand(sql, cn);

                SqlDataAdapter sqlDA = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();


                sqlDA.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow drow in dt.Rows)
                    {
                        UpDelAssociatedLabel _UpDelAssociatedLabel = new UpDelAssociatedLabel();

                        _UpDelAssociatedLabel.fieldname = drow["field_name"].ToString() ?? "";
                        _UpDelAssociatedLabel.oldvalue = drow["old_value"].ToString() ?? "";
                        _UpDelAssociatedLabel.newvalue = drow["new_value"].ToString() ?? "";
                        _UpDelAssociatedLabel.old_Registration_plate = drow["old_Registration_plate"].ToString() ?? "";
                        _UpDelAssociatedLabel.new_Registration_plate = drow["new_Registration_plate"].ToString() ?? "";
                        _UpDelAssociatedLabel.old_ClientID = drow["old_ClientID"].ToString() ?? "";
                        _UpDelAssociatedLabel.new_ClientID = drow["new_ClientID"].ToString() ?? "";
                        _UpDelAssociatedLabel.old_Email = General.EscapeSpecialCharacters(drow["old_Email"].ToString() ?? "");
                        _UpDelAssociatedLabel.new_Email = General.EscapeSpecialCharacters(drow["new_Email"].ToString() ?? "");
                        _UpDelAssociatedLabel.changetype = drow["change_type"].ToString() ?? "";
                        _UpDelAssociatedLabel.detectedon = (drow["detected_on"] == DBNull.Value) ? "" : (DateTime.Parse((drow["detected_on"].ToString() ?? ""))).ToString("yyyy-MM-dd"); ;

                        _UpDelAssociatedLabel.datasource = drow["datasource"].ToString() ?? "";
                        _UpDelAssociatedLabel.AssociationReasonID = int.Parse(drow["AssociationReasonID"].ToString() ?? "0");


                        _UpDelAssociatedLabels.Add(_UpDelAssociatedLabel);


                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cn.Close();
            }
            return _UpDelAssociatedLabels;
        }
        //public static List<(string KeyToArchive, string ObjectNameToArchive)> GetHubSpotArchiveRecord(string ConnectionString)
        //{
        //    string sql = string.Empty;
        //    var results = new List<(string KeyToArchive, string ObjectNameToArchive)>();

        //    SqlConnection cn = new SqlConnection(ConnectionString);
        //    try
        //    {
        //        sql = $"exec [HavayatLakoah].[HubSpotArchiveRecord] ";



        //        cn.Open();


        //        SqlCommand cmd = new SqlCommand(sql, cn);

        //        SqlDataAdapter sqlDA = new SqlDataAdapter(cmd);
        //        DataTable dt = new DataTable();


        //        sqlDA.Fill(dt);
        //        if (dt.Rows.Count > 0)
        //        {
        //            foreach (DataRow drow in dt.Rows)
        //            {
        //                results.Add(
        //                            (
        //                            drow["KeyToArchive"].ToString() ?? "", 
        //                            drow["ObjectNameToArchive"].ToString() ?? ""
        //                            )
        //                    );

                            





        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        cn.Close();
        //    }
        //    return results;

        //}

        #endregion

    }

}

