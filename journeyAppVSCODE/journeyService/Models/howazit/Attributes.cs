namespace journeyService.Models.howazit
{
    public class Attributes
    {
        private string _key = string.Empty;
        private string _value = string.Empty;

        public string Key { get => _key; set => _key = value; }
        public string Value { get => _value; set => _value = value; }

        public Attributes(string key, string value)
        {
            this._key = key;
            this._value = value;
        }
    }
}
