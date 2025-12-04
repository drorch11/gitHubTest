namespace journeyService.Models.howazit
{
    public class howazitorderAvis
    {
        public string misparlakoah = string.Empty;
        public string shemlakoah { get; set; } =string.Empty;
        public string email { get; set; } = string.Empty;
        public string telephone1 { get; set; } = string.Empty;
        //קוד עובד הסכם
        public string empname { get; set; } = string.Empty;
        public string teur_yazran_al { get; set; } = string.Empty;
        public string teur_degem_al { get; set; } = string.Empty;
        public string car_no { get; set; } = string.Empty;
        //סוג קניה
        public int sale_type { get; set; } 
        public string misparerua { get; set; } = string.Empty;
        public string deal_type_desc { get; set; } = string.Empty;
        public string? car_delivery_date { get; set; } = string.Empty;

        public bool acceptdivurlak { get; set; }

        //קוד סניף חתימה
        public string agreement_signing_location_kod { get; set; } = string.Empty;
        //שם עובד
        public string agreementsigning_empnname { get; set; } = string.Empty;

        public string entrycode { get; set; } = string.Empty;
    }
}
