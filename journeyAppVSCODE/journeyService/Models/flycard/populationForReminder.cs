namespace journeyService.Models.flycard
{
    public class populationForReminder
    {
        public string pickup_station_name { get; set; } = string.Empty;
        public string pickup_datetime { get; set; } = string.Empty;
        public int reservation_flycar_number { get; set; }
        public string contact_phone { get; set; } = string.Empty;
        
        public string contact_email { get; set; } = string.Empty;
        
        public string contact_name { get; set; } = string.Empty;
        public string pickup_station_country { get; set; } = string.Empty;
        public string report_url { get; set; } = string.Empty;

        public populationForReminder()
        {
            this.reservation_flycar_number = -1;
        }
    }
}
