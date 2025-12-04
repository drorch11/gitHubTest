namespace journeyService.Models.hubspot
{
    public class hubspotResponseImport
    {
        public string state { get; set; } = string.Empty;
        public string importName { get; set; } = string.Empty;
        public bool optOutImport { get; set; }
        public string id { get; set; } = string.Empty;

        public long estimatedLineCount { get; set; }

        public hubspotResponseImport()
        {
               
        }

    }
}
