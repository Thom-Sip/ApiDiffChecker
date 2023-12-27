namespace RefactorHelper.Models.Config
{
    public class RefactorHelperSettings
    {
        public bool RunOnStart { get; set; }

        public string OutputFolder { get; set; } = Path.Combine(GetBinPath(), "Output");

        public string ContentFolder { get; set; } = Path.Combine(GetBinPath(), "Content");

        public string SwaggerUrl { get; set; } = string.Empty;

        public string BaseUrl1 { get; set; } = string.Empty;

        public string BaseUrl2 { get; set; } = string.Empty;

        public List<Parameter> DefaultParameters { get; set; } = [];

        public List<List<Parameter>> Runs { get; set; } = [[]];

        public List<Parameter> PropertiesToReplace { get; set; } = [];

        public HttpClient HttpClient1 { get; set; }

        public HttpClient HttpClient2 { get; set; }

        public RefactorHelperSettings(HttpClient client1, HttpClient client2)
        {
            HttpClient1 = client1;
            HttpClient2 = client2;
        }

        public RefactorHelperSettings(string baseUrl1, string baseUrl2)
        {
            BaseUrl1 = baseUrl1;
            BaseUrl2 = baseUrl2;
            HttpClient1 = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl1)
            };
            HttpClient2 = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl2)
            };
        }

        private static string GetBinPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
    }
}
