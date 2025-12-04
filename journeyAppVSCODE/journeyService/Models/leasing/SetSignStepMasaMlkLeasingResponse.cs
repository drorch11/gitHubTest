namespace journeyService.Models.leasing
{
    public class SetSignStepMasaMlkLeasingResponse
    {
        public int Signed { get; set; }
        public string NotSignedReason { get; set; } = string.Empty;
        public int Success { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}

