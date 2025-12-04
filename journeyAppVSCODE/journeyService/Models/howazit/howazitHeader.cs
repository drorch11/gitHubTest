namespace journeyService.Models.howazit
{
    public class howazitHeader
    {
        private string externalSource = string.Empty;
        private string externalBusinessId = string.Empty;
        private string externalBranchId = string.Empty; // (קוד מפיץ)
        private string endDate = string.Empty; //Date that the message will be send.
        private string entryCode = string.Empty;
        private string firstName = string.Empty;
        private string phone = string.Empty;
        private string email = string.Empty;
        private bool allowdivur = false;
        private bool isTest = true;
        private List<Attributes> attributes;

        public string ExternalSource { get => externalSource; set => externalSource = value; }
        public string ExternalBusinessId { get => externalBusinessId; set => externalBusinessId = value; }
        public string ExternalBranchId { get => externalBranchId; set => externalBranchId = value; }
        public string EndDate { get => endDate; set => endDate = value; }
        public string EntryCode { get => entryCode; set => entryCode = value; }
        public string FirstName { get => firstName; set => firstName = value; }
        public string LastName { get; set; } = string.Empty;
        public string Phone { get => phone; set => phone = value; }
        public string Email { get => email; set => email = value; }
        public bool IsTest { get => isTest; set => isTest = value; }
        public List<Attributes> Attributes { get => attributes; set => attributes = value; }
        public bool Allowdivur { get => allowdivur; set => allowdivur = value; }

        public howazitHeader()
        {
            this.attributes = new List<Attributes>();
        }
    }
}
