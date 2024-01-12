using Newtonsoft.Json;

namespace ApiDiffChecker.Models.Settings
{
    public class ApiDiffCheckerSettings
    {
        public bool RunOnStart { get; set; }

        public string ContentFolder { get; set; } = Path.Combine(GetBinPath(), "ApiDiffCheckerContent");

        public string SwaggerUrl { get; set; } = string.Empty;

        public Run DefaultRunSettings { get; set; } = new();

        public List<Run> Runs { get; set; } = [];

        [JsonIgnore] public HttpClient HttpClient1 { get; set; }

        [JsonIgnore] public HttpClient HttpClient2 { get; set; }

        [JsonIgnore] public string MainContentFolder { get => $"{ContentFolder}/Components/Main"; }

        [JsonIgnore] public string SidebarContentFolder { get => $"{ContentFolder}/Components/Sidebar"; }

        [JsonIgnore] public string FormsContentFolder { get => $"{ContentFolder}/Components/Forms"; }

        public static ApiDiffCheckerSettings GetSettingsFromJson(string jsonPath, string baseUrl1, string baseUrl2)
        {
            ApiDiffCheckerSettings result = new();

            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);
                result = JsonConvert.DeserializeObject<ApiDiffCheckerSettings>(json) ?? new ApiDiffCheckerSettings();
            }

            return result.SetClientsFromUrls(baseUrl1, baseUrl2);
        }

        private ApiDiffCheckerSettings() { }

        public ApiDiffCheckerSettings(HttpClient client1, HttpClient client2)
        {
            HttpClient1 = client1;
            HttpClient2 = client2;
        }

        public ApiDiffCheckerSettings(string baseUrl1, string baseUrl2)
        {
            this.SetClientsFromUrls(baseUrl1, baseUrl2);
        }

        public string GetSwaggerUrl()
        {
            return string.IsNullOrWhiteSpace(SwaggerUrl)
                ? $"{HttpClient1?.BaseAddress}swagger/v1/swagger.json"
                : SwaggerUrl;
        }

        private static string GetBinPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
    }

    public static class RefactorHelperSettingsExtensions
    {
        public static ApiDiffCheckerSettings SetClientsFromUrls(this ApiDiffCheckerSettings settings, string baseUrl1, string baseUrl2)
        {
            settings.HttpClient1 = new HttpClient
            {
                BaseAddress = new Uri(baseUrl1)
            };

            settings.HttpClient2 = new HttpClient
            {
                BaseAddress = new Uri(baseUrl2)
            };

            return settings;
        }
    }
}
