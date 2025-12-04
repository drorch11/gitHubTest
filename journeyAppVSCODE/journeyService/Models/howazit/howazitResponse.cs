namespace journeyService.Models.howazit
{
    public class howazitResponse
    {
        private bool accepted;
        private string errorDescription = string.Empty;

        public string ErrorDescription { get => errorDescription; set => errorDescription = value; }

        public bool Accepted { get => accepted; set => accepted = value; }
    }
}
