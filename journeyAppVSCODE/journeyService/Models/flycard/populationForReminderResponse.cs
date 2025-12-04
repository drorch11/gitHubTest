namespace journeyService.Models.flycard
{
    public class populationForReminderResponse
    {
        public bool success { get; set; }

        public List<populationForReminder> results { get; set; }

        public string message { get; set; } = string.Empty;

        public populationForReminderResponse()
        {
            this.results = new List<populationForReminder>();
        }




    }
}
