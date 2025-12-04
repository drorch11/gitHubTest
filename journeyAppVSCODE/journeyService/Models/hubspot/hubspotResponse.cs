namespace journeyService.Models.hubspot
{
    public class UnsubscribePayload
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Type { get; set; } = string.Empty;
        public long LastFetchedId { get; set; }

    }

    public class hubspotResponse
    {
        public hubspotResponseImport? _hubspotResponseImport { get; set; }

        public string errorDescription = string.Empty;

        public bool accepted;

        public hubspotResponse()
        {
                this._hubspotResponseImport = new hubspotResponseImport();
                this.accepted = false;
        }
    }
}
