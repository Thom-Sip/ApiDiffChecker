using Newtonsoft.Json;

namespace ApiDiffChecker.Models.Settings
{
    public class ApiDiffCheckerSettings
    {
        public bool RunOnStart { get; set; }

        public string SwaggerUrl { get; set; } = string.Empty;

        public string SettingsJsonPath { get; set; } = string.Empty;

        /// <summary>
        /// We don't want to serialize ApiUrl1 since this refers to the local api instance, just use the current one.
        /// </summary>
        [JsonIgnore] public string ApiUrl1 { get; set; }

        public string ApiUrl2 { get; set; }

        public Run DefaultRunSettings { get; set; } = new();

        public List<Run> Runs { get; set; } = [];

        [JsonIgnore] public HttpClient HttpClient1 { get; set; }

        [JsonIgnore] public HttpClient HttpClient2 { get; set; }

        [JsonIgnore] public string ContentFolder { get; } = GetContentPath();

        [JsonIgnore] public string MainContentFolder { get => $"{ContentFolder}/Components/Main"; }

        [JsonIgnore] public string SidebarContentFolder { get => $"{ContentFolder}/Components/Sidebar"; }

        [JsonIgnore] public string FormsContentFolder { get => $"{ContentFolder}/Components/Forms"; }

        public ApiDiffCheckerSettings() { }

        public ApiDiffCheckerSettings(HttpClient client1, HttpClient client2)
        {
            HttpClient1 = client1;
            HttpClient2 = client2;
        }

        public ApiDiffCheckerSettings(string baseUrl1, string baseUrl2)
        {
            ApiUrl1 = baseUrl1;
            ApiUrl2 = baseUrl2;
        }

        public void LoadSettingsFromDisk(string jsonPath)
        {
            SettingsJsonPath = jsonPath;
            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);
                var result = JsonConvert.DeserializeObject<ApiDiffCheckerSettings>(json) ?? new ApiDiffCheckerSettings();

                SwaggerUrl = result.SwaggerUrl;
                SettingsJsonPath = result.SettingsJsonPath;
                ApiUrl1 = result.ApiUrl1;
                ApiUrl2 = result.ApiUrl2;
                DefaultRunSettings = result.DefaultRunSettings;
                Runs = result.Runs;
            }
        }

        public void SetUrlDefaults(string localUrl)
        {
            if (string.IsNullOrWhiteSpace(ApiUrl1))
                ApiUrl1 = localUrl;

            if (string.IsNullOrWhiteSpace(ApiUrl2))
                ApiUrl2 = localUrl;
        }

        public void GenerateClients()
        {
            HttpClient1 ??= new HttpClient
            {
                BaseAddress = new Uri(ApiUrl1)
            };

            HttpClient2 ??= new HttpClient
            {
                BaseAddress = new Uri(ApiUrl2)
            };
        }

        public void SaveSettingsToDisk()
        {
            var serialized = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingsJsonPath, serialized);
        }

        public string GetSwaggerUrl()
        {
            return string.IsNullOrWhiteSpace(SwaggerUrl)
                ? $"{HttpClient1?.BaseAddress}swagger/v1/swagger.json"
                : SwaggerUrl;
        }

        private static string GetBinPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");

        private static string GetContentPath()
        {
            // We have to place the content files in 'contentFiles/any/any/' to prevent the content from being added twice to the nuget package.
            // As a result the content path is different when testing within this solution compared to when the package was used.
            // Simple fix to make both scenarios work.
            var path = Path.Combine(GetBinPath(), "contentFiles/any/any/ApiDiffChecker");

            return Directory.Exists(path)
                ? path
                : Path.Combine(GetBinPath(), "ApiDiffChecker");
        }
    }

    public static class RefactorHelperSettingsExtensions
    {
        public static void SetClientsFromUrls(this ApiDiffCheckerSettings settings, string baseUrl1, string baseUrl2)
        {
            settings.HttpClient1 = new HttpClient
            {
                BaseAddress = new Uri(baseUrl1)
            };

            settings.HttpClient2 = new HttpClient
            {
                BaseAddress = new Uri(baseUrl2)
            };
        }
    }
}
