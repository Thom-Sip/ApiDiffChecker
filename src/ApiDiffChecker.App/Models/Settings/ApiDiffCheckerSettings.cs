using Newtonsoft.Json;

namespace ApiDiffChecker.Models.Settings
{
    public class ApiDiffCheckerSettings
    {
        public bool RunOnStart { get; set; }

        public string SwaggerUrl { get; set; } = string.Empty;

        public string SettingsJsonPath { get; set; } = string.Empty;

        public Run DefaultRunSettings { get; set; } = new();

        public List<Run> Runs { get; set; } = [];

        [JsonIgnore] public HttpClient HttpClient1 { get; set; }

        [JsonIgnore] public HttpClient HttpClient2 { get; set; }

        [JsonIgnore] public string ContentFolder { get; } = GetContentPath();

        [JsonIgnore] public string MainContentFolder { get => $"{ContentFolder}/Components/Main"; }

        [JsonIgnore] public string SidebarContentFolder { get => $"{ContentFolder}/Components/Sidebar"; }

        [JsonIgnore] public string FormsContentFolder { get => $"{ContentFolder}/Components/Forms"; }

        public static ApiDiffCheckerSettings GetSettingsFromJson(string baseUrl1, string baseUrl2) =>
            GetSettingsFromJson($"{Environment.CurrentDirectory}/apiDiffChecker.json", baseUrl1, baseUrl2);

        public static ApiDiffCheckerSettings GetSettingsFromJson(string jsonPath, string baseUrl1, string baseUrl2)
        {
            ApiDiffCheckerSettings result;

            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);
                result = JsonConvert.DeserializeObject<ApiDiffCheckerSettings>(json) ?? new ApiDiffCheckerSettings();
            }
            else
            {
                result = new()
                {
                    SettingsJsonPath = jsonPath
                };

                result.SaveSettingsToDisk();
            }

            return result.SetClientsFromUrls(baseUrl1, baseUrl2);
        }

        public ApiDiffCheckerSettings(HttpClient client1, HttpClient client2)
        {
            HttpClient1 = client1;
            HttpClient2 = client2;
        }

        public ApiDiffCheckerSettings(string baseUrl1, string baseUrl2)
        {
            this.SetClientsFromUrls(baseUrl1, baseUrl2);
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

        private ApiDiffCheckerSettings() { }

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
