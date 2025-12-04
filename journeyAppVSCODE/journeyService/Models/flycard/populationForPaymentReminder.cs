namespace journeyService.Models.flycard
{
    public class populationForPaymentReminder
    {
        public int reservation_flycar_number { get; set; }
        public string contact_phone { get; set; } = string.Empty;
        public string report_url { get; set; } = string.Empty;
        public string pickup_datetime { get; set; } = string.Empty;
        public string contact_name { get; set; } = string.Empty;
        public string pickup_station_country { get; set; } = string.Empty;
        public string pickup_station_name { get; set; } = string.Empty;
        public string contact_email { get; set; } = string.Empty;

        /// <summary>
        /// fileld do not exists in populationForReminder class
        /// </summary>
        public string payment_url { get; set; } = string.Empty;
        public string first_name { get; set; } = string.Empty;
        public string last_name { get; set; } = string.Empty;
        public string customer_name { get; set; } = string.Empty;
        public bool is_private_customer { get; set; }

        public populationForPaymentReminder()
        {
            this.reservation_flycar_number = -1;
            this.is_private_customer = false;
        }
    }
}
