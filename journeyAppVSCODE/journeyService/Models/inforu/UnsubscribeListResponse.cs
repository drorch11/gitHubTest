namespace journeyService.Models.inforu
{
    public class UnsubscribeDataList
    {
        public string Type { get; set; }= string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime UnsubscribeDatetime { get; set; }
    }
    public class UnsubscribeData
    {
        public int Count { get; set; }
        public long LastFetchedId { get; set; }

        public List<UnsubscribeDataList> List { get; set; }

        public UnsubscribeData()
        {
                this.List = new List<UnsubscribeDataList>();
        }
    }


    public class UnsubscribeListResponse
    {
        public int StatusId { get; set; }
        public string StatusDescription { get; set; } = string.Empty;

        public UnsubscribeData Data { get; set; }

    }
}
