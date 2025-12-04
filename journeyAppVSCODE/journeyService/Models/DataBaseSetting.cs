namespace journeyService.Models
{
    public class DataBaseSetting
    {
        public const string SectionName = "Database";
        public string ConnectionString { get; set; } = string.Empty;
    }
}
