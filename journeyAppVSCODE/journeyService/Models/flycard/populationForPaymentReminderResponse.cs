namespace journeyService.Models.flycard
{
    public class populationForPaymentReminderResponse
    {
        public bool success { get; set; }

        public List<populationForPaymentReminder> results { get; set; }

        public string message { get; set; } = string.Empty;

        public populationForPaymentReminderResponse()
        {
            this.results = new List<populationForPaymentReminder>();
        }
    }
}
