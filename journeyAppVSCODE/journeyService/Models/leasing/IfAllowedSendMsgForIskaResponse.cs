namespace journeyService.Models.leasing
{
    public class IfAllowedSendMsgForIskaResponse
    {
        public int Allowed { get; set; }
        public string NotAllowedReason { get; set; } = string.Empty;
        public int Success { get; set; }
        public string Error { get; set; } = string.Empty;



    }
}
