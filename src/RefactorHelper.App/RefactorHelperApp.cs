using RefactorHelper.Comparer;
using RefactorHelper.Models;
using RefactorHelper.RequestHandler;
using RefactorHelper.UIGenerator;
using RefactorHelper.SwaggerProcessor;

namespace RefactorHelper.App
{
    public class RefactorHelperApp
    {
        private RefactorHelperSettings Settings { get; set; }

        private UIGeneratorService UIGeneratorService { get; set; }

        private RequestHandlerService RequestHandler { get; set; }

        private SwaggerProcessorService SwaggerProcessor { get; set; }

        private CompareService CompareService { get; set; }

        private string SwaggerJson { get; set; } = string.Empty;

        public RefactorHelperApp(RefactorHelperSettings settings)
        {
            Settings = settings;

            if (string.IsNullOrWhiteSpace(Settings.OutputFolder))
                Settings.OutputFolder = $"{GetBinPath()}/Files/Output/";

            if (string.IsNullOrWhiteSpace(Settings.ContentFolder))
                Settings.ContentFolder = $"{GetBinPath()}/Files/Content/";

            // Setup Swagger Processor
            SwaggerProcessor = new SwaggerProcessorService(Settings);

            // Setup Request Handler
            RequestHandler = new RequestHandlerService(
                new HttpClient
                {
                    BaseAddress = new Uri(Settings.BaseUrl1)
                },
                new HttpClient
                {
                    BaseAddress = new Uri(Settings.BaseUrl2)
                }
            );

            // Compare Service
            CompareService = new CompareService();

            // UI Generator
            UIGeneratorService = new UIGeneratorService(Settings.ContentFolder, Settings.OutputFolder);
        }

        private static string GetBinPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");

        public async Task<List<string>> Run()
        {
            if (!string.IsNullOrWhiteSpace(SwaggerJson))
                return [];

            if(string.IsNullOrWhiteSpace(SwaggerJson))
            {
                var client = new HttpClient();
                var result = await client.GetAsync(Settings.SwaggerUrl);
                SwaggerJson = await result.Content.ReadAsStringAsync();
            }     

            // Get requests from swagger
            var requestDetails = SwaggerProcessor.ProcessSwagger(SwaggerJson);

            // Perform api Requests
            var responses = await RequestHandler.QueryApis(requestDetails);

            // Get diffs on responses
            var results = CompareService.CompareResponses(responses);

            // Generate output
            var outputFileNames = UIGeneratorService.GenerateUI(results);

            return outputFileNames;
        }
    }
}
