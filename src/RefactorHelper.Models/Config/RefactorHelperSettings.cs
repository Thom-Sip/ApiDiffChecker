﻿using Newtonsoft.Json;

namespace RefactorHelper.Models.Config
{
    public class RefactorHelperSettings
    {
        public bool RunOnStart { get; set; }

        public string OutputFolder { get; set; } = Path.Combine(GetBinPath(), "Output");

        public string ContentFolder { get; set; } = Path.Combine(GetBinPath(), "Content");

        public string SwaggerUrl { get; set; } = string.Empty;

        public List<Parameter> DefaultParameters { get; set; } = [];

        public List<List<Parameter>> Runs { get; set; } = [];

        public List<Parameter> PropertiesToReplace { get; set; } = [];

        [JsonIgnore]
        public HttpClient? HttpClient1 { get; set; }

        [JsonIgnore]
        public HttpClient? HttpClient2 { get; set; }

        public RefactorHelperSettings(HttpClient client1, HttpClient client2)
        {
            HttpClient1 = client1;
            HttpClient2 = client2;

            Runs.Add([]);
        }

        public RefactorHelperSettings(string baseUrl1, string baseUrl2)
        {
            this.SetClientsFromUrls(baseUrl1, baseUrl2);

            Runs.Add([]);
        }

        public static RefactorHelperSettings GetSettingsFromJson(string jsonPath, string baseUrl1, string baseUrl2)
        {
            RefactorHelperSettings result = new();

            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);
                result = JsonConvert.DeserializeObject<RefactorHelperSettings>(json) ?? new RefactorHelperSettings();
            }

            if (result.Runs.Count == 0)
                result.Runs.Add([]);

            return result.SetClientsFromUrls(baseUrl1, baseUrl2);
        }

        /// <summary>
        /// used only for GetSettingsFromJson
        /// </summary>
        private RefactorHelperSettings() { }

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
        public static RefactorHelperSettings SetClientsFromUrls(this RefactorHelperSettings settings, string baseUrl1, string baseUrl2)
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
