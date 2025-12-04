namespace journeyService.Models.hubspot.Api
{
    public class UpDelAssociatedLabel
    {
        public string fieldname { get; set; } = string.Empty;
        public string oldvalue { get; set; } = string.Empty;
        public string newvalue { get; set; } = string.Empty;
        public string old_Registration_plate { get; set; } = string.Empty;
        public string new_Registration_plate { get; set; } = string.Empty;
        public string old_ClientID { get; set; } = string.Empty;
        public string new_ClientID { get; set; } = string.Empty;
        public string old_Email { get; set; } = string.Empty;
        public string new_Email { get; set; } = string.Empty;
        public string changetype { get; set; } = string.Empty;
        public string detectedon { get; set; } = string.Empty;

        public int AssociationReasonID { get; set; }  //AssociationChangeReason table
        public string datasource { get; set; } = string.Empty;

        



    }
}
