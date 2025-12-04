namespace journeyService.Models.hubspot.Api
{


    public class Properties
    {

        public string client_first_name { get; set; } = string.Empty;
        public string umi_uid { get; set; } = string.Empty; //encoded code
        public string phone_number { get; set; } = string.Empty;
        public string newregistrationplate { get; set; } = string.Empty; //real car id

        public string client_email { get; set; } = string.Empty; //


    }
    public class Result
    {
        public string id { get; set; }
        public Properties properties { get; set; } = new Properties();
    }
    public class HbSearchCustomObjectResponse
    {
        public bool accepted { get; set; }
        public string errormsg { get; set; } = string.Empty;
        public int total { get; set; }
        public int statuscode { get; set; }
        public List<Result> results { get; set; } = new List<Result>();


    }

    

}
