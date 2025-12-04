using DnsClient;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using journeyService.Models.hubspot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace journeyService.Utils
{
    public static class General
    {


        public static string EscapeSpecialCharacters(string comments)
        {



            return comments.Replace("'", "''");
        }
        public static bool IsTestEnvironment(string myUrl)
        {
            
            return (myUrl.Contains("http://localhost") ||
                myUrl.Contains("https://localhost"));
        }
        public static string ChoosePhone(string phone1, string phone2)
        {
            string TELEFON = "";
            string TELEFON_NOSAF = "";

            TELEFON = String.IsNullOrEmpty(phone1) ? "" : "972" + phone1.TrimStart(new Char[] { '0' }).Replace("-", "");
            TELEFON_NOSAF = String.IsNullOrEmpty(phone2) ? "" : "972" + phone2.TrimStart(new Char[] { '0' }).Replace("-", "");

            return (TELEFON != "" && TELEFON.Substring(0, 4) == "9725") ? TELEFON
                : (TELEFON_NOSAF != "" && TELEFON_NOSAF.Substring(0, 4) == "9725") ? TELEFON_NOSAF
                : "";
        }
     
        public static string formatStringToDateFormat(string field,string FormatSpecifier)
        {
            return DateTime.Parse(field ?? "").ToString(FormatSpecifier);
        }
        public static bool formatStringToBoolean(string field)
        {
            return (field == "1") ? true : false;
            
        }

        public static string ReadJsonFileTemplate(string path,string filename)
        {
            string jsonStr = string.Empty;
            using (StreamReader r = new StreamReader(path + filename))
            {

                jsonStr = r.ReadToEnd();

            }
            return jsonStr;

        }

        #region EncryptDecrypt
        public static string EncryptString(string key, string plainText)
        {
            //key must be len of 16
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }
        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string getUrlAfterQuestionMark()
        {
            HttpContextAccessor accessor = new HttpContextAccessor();
            string url = accessor.HttpContext?.Request?.GetDisplayUrl() ?? "";
            return url.Substring(url.LastIndexOf("?") + 1);


            
        }

        public static string getParamfromstrDecrypt(string strDecrypt, string desiredKey)
        {

            //string RawUrl = "http://www.example.com?param1=good&param2=bad";
            //int index = RawUrl.IndexOf("?");
            //if (index > 0)
            //    RawUrl = RawUrl.Substring(index).Remove(0, 1);

            //Uri myUri = new Uri(RawUrl, UriKind.RelativeOrAbsolute);
            //string param = HttpUtility.ParseQueryString(strDecrypt).Get(desiredKey);
            //return param;

            string desiredValue = string.Empty;
            foreach (string item in strDecrypt.Split('&'))
            {
                string[] parts = item.Split('=');
                if (parts[0] == desiredKey)
                {
                    desiredValue = parts[1];
                    break;
                }
            }
            return desiredValue;
        }

        #endregion

        #region Excel
        public static void CreateExcel(string filePath, List<UmiCars> umiCars)
        {
            
                //excel source https://www.youtube.com/watch?v=6gMDGbf9XZ4
            
                
                using(SpreadsheetDocument doc  = SpreadsheetDocument.Open(filePath,true))
                {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheet sheet = workbookPart.Workbook.Descendants<Sheet>().First();
                    WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                    foreach (var item in umiCars)
                    {
                        Row newRow = new Row();
                        Cell cell;
                        #region write row Cells
                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.clientid)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.firstname)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.lastname)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.allowdivur)
                    };
                    newRow.Append(cell);


                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.email)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.phone)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.manufactureyear)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.newregistrationplate)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.carmodel)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.manufacturer)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.lasttreatment_date)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.cardeliverydate)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.kodsalesbranch)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.salesbranch)
                    };
                    newRow.Append(cell);


                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.lastgaragecode)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.lastgaragename)
                    };
                    newRow.Append(cell);


                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.associationlable)
                    };
                    newRow.Append(cell);


                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(DateTime.Now.ToString("yyyy-MM-dd"))
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.dateidkunbaalut)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.registrationplate)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.contact_association_lable)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.date_car_test)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.allow_divur_person)
                    };
                    newRow.Append(cell);

                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.ops__tafnit_unit)
                    };
                    newRow.Append(cell);

      


                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.kodmachzormechira)
                    };
                    newRow.Append(cell);
                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.kmmechira)
                    };
                    newRow.Append(cell);
                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.kod_customerclassification)
                    };
                    newRow.Append(cell);
                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.customerclassification)
                    };
                    newRow.Append(cell);
                    cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(item.agreementdate)
                    };
                    newRow.Append(cell);
                    #endregion

                    //foreach (var cellData in rowData)
                    //{
                    //    Cell cell = new Cell()
                    //    {
                    //        DataType = CellValues.String,
                    //        CellValue = new CellValue(cellData)
                    //    };
                    //    newRow.Append(cell);
                    //}
                    sheetData.Append(newRow);
                    }
                  
                       
                    
                   


                }
            

        }
        #endregion


        #region ValidEmail
        public static bool IsValidSyntax(string email)
        {
            try
            {
                var addr = new MailAddress(email.Trim());
                return addr.Address == email.Trim();
            }
            catch { return false; }
        }
        public static bool DomainHasMX(string email)
        {
            try
            {
                var domain = email.Split('@')[1];
                var lookup = new LookupClient();
                var result = lookup.Query(domain, QueryType.MX);
                return result.Answers.MxRecords().Any();
            }
            catch { return false; }
        }
        public static bool IsValidEmailRFC5322(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern =
                @"^(?:[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*|\""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*\"")@(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}|(?:\[(?:(?:25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?|[a-zA-Z-]*[a-zA-Z0-9]:.+)\]))$";

            return Regex.IsMatch(email, pattern);
        }
        private static readonly HashSet<string> ValidTlds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "com", "net", "org", "gov", "edu", "mil", "il",
        "co.il", "net.il", "biz", "info", "me", "io", "us", "uk", "de", "fr", "ru", "ca", "au"
    };

        public static bool IsLikelyValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim();

            // תווים אסורים
            if (Regex.IsMatch(email, @"[!#$%^&*(),?"";:{}|<>\\/\[\]\s]"))
                return false;

            // חובה @ אחד בלבד
            var parts = email.Split('@');
            if (parts.Length != 2)
                return false;

            var local = parts[0];
            var domain = parts[1];

            // חלקים ריקים
            if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(domain))
                return false;

            // חובה נקודה אחרי ה־@
            if (!domain.Contains('.'))
                return false;

            // בדיקת סיומת
            var tld = domain.Substring(domain.LastIndexOf('.') + 1);
            if (tld.Length < 2 || tld.Length > 3)
                return false;

            if (!ValidTlds.Contains(tld))
                return false;

            return true;
        }

        #endregion
    }
}
