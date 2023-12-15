namespace RefactorHelper.Models
{
    public class RefactorHelperSettings
    {
        public required string OutputFolder { get; set; }

        public required string ContentFolder { get; set; }

        public required string swaggerUrl { get; set; }

        public required string BaseUrl1 { get; set; }

        public required string BaseUrl2 { get; set; }

        public List<Parameter> DefaultParameters { get; set; } = [];

        public List<List<Parameter>> Runs { get; set; } = [[]];
    }

    public class Parameter
    {
        public string Key { get; private set; }

        public string Value { get; private set; }

        public Parameter(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
