using DocumentFormat.OpenXml.Office.CustomUI;

namespace journeyService.Models.hubspot.Api
{
    public class HbUpsertAssociated
    {
        public List<typesAssocaited> types { get; set; }
        public fromObject from { get; set; }
  
        public toObject to { get; set; }

        public HbUpsertAssociated(List<typesAssocaited> _types, fromObject _from, toObject _to)
        {
                types = _types;
                from = _from;
                to = _to;
        }

    }
    public class typesAssocaited
    {
        public int associationTypeId { get; set; }
        public string associationCategory { get; set; } = string.Empty;


    }
    public class fromObject
    {
        public string id { get; set; } = string.Empty;


    }
    public class toObject
    {
        public string id { get; set; } = string.Empty;

    }
    public enum associationTypeId
    {
        contact = 73, //איש קשר
        contact_norelevant = 92, //איש קשר לא רלוונטי
        contact_danlis = 102, //איש קשר דן ליס
        contact_certified = 100, //איש קשר סרטיפייד 

        car_owner_danlis = 108, //בעל רכב דן ליס
        car_buyer_danlis = 106, //רוכש רכב דן ליס
        
        car_owner_certified = 110, //בעל רכב סרטיפייד
        car_buyer_certified = 104, //רוכש רכב סרטיפייד

        car_owner = 48, //בעל רכב
        car_buyer = 50, //בכל רכב
        
        person_tafnitumi_norelevant = 94, //אדם תפנית יו אמ איי לא רלוונטי
        person_tafnitavis_norelevant = 112 //אדם תפנית אוויס לא רלוונטי





    }
    public static class HubspotObject
    {
        public const string Car = "2-124289527";
        public const string Person = "2-130920180";
    }
}
