namespace RefactorHelper.Models.Config
{
    public class RefactorHelperSettings
    {
        public bool RunOnStart { get; set; }

        public string OutputFolder { get; set; } = string.Empty;

        public string ContentFolder { get; set; } = string.Empty;

        public required string SwaggerUrl { get; set; }

        public required string BaseUrl1 { get; set; }

        public required string BaseUrl2 { get; set; }

        public List<Parameter> DefaultParameters { get; set; } = [];

        public List<List<Parameter>> Runs { get; set; } = [[]];

        public List<Parameter> PropertiesToReplace { get; set; } = [];
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
