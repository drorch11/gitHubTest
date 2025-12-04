namespace journeyService.Models.hubspot.Api
{
    //public class HbSearchCustomObject
    //{
    //    public string email { get; set; } = string.Empty;
    //}

    public class Hbfilters
    {
        public string propertyName { get; set; } = string.Empty;

        public string @operator { get; set; } = string.Empty;
        
        public string value { get; set; } = string.Empty;
    }
    public class HbSearchCustomObject
    {
        public List<Hbfilters> filters { get; set; }
        //public List<Hbfilters> filters { get; set; }
        public HbSearchCustomObject()
        {
            //filters = new List<Hbfilters>();
            filters = new List<Hbfilters>();
        }
        public HbSearchCustomObject(List<Hbfilters> _Hbfilters)
        {
            //filters = new List<Hbfilters>();
            filters = _Hbfilters;
        }

    }
}
