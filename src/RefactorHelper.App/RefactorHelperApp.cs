using RefactorHelper.Comparer;
using RefactorHelper.RequestHandler;
using RefactorHelper.UIGenerator;
using RefactorHelper.SwaggerProcessor;
using RefactorHelper.Models.Config;
using RefactorHelper.Models.SwaggerProcessor;
using RefactorHelper.Models;
using RefactorHelper.Models.Comparer;

namespace RefactorHelper.App
{
    public class RefactorHelperApp
    {
        private RefactorHelperSettings Settings { get; set; }

        private SwaggerProcessorService SwaggerProcessorService { get; set; }

        private RequestHandlerService RequestHandlerService { get; set; }

        private CompareService CompareService { get; set; }

        private UIGeneratorService UIGeneratorService { get; set; }

        private string SwaggerJson { get; set; } = string.Empty;

        private SwaggerProcessorOutput SwaggerProcessorOutput { get; set; }

        private RequestHandlerOutput RequestHandlerOutput { get; set; }

        private ComparerOutput ComparerOutput { get; set; }

        private List<string> OutputFileNames { get; set; }

        public RefactorHelperApp(RefactorHelperSettings settings)
        {
            Settings = SetDefaults(settings);

            // Setup Swagger Processor
            SwaggerProcessorService = new SwaggerProcessorService(Settings);

            // Setup Request Handler
            RequestHandlerService = new RequestHandlerService(
                new HttpClient
                {
                    BaseAddress = new Uri(Settings.BaseUrl1)
                },
                new HttpClient
                {
                    BaseAddress = new Uri(Settings.BaseUrl2)
                }, 
                Settings
            );

            // Compare Service
            CompareService = new CompareService();

            // UI Generator
            UIGeneratorService = new UIGeneratorService(Settings.ContentFolder, Settings.OutputFolder);
        }

        public async Task<List<string>> Run()
        {
            if(string.IsNullOrWhiteSpace(SwaggerJson))
            {
                var client = new HttpClient();
                var result = await client.GetAsync(Settings.SwaggerUrl);
                SwaggerJson = await result.Content.ReadAsStringAsync();
            }

            // Get requests from swagger
            SwaggerProcessorOutput = SwaggerProcessorService.ProcessSwagger(SwaggerJson);

            // Perform api Requests
            RequestHandlerOutput = await RequestHandlerService.QueryApis(SwaggerProcessorOutput);

            // Get diffs on responses
            ComparerOutput = CompareService.CompareResponses(RequestHandlerOutput);

            // Generate output
            OutputFileNames = UIGeneratorService.GenerateUI(ComparerOutput);

            return OutputFileNames;
        }

        public async Task<string> PerformSingleCall(int runId)
        {
            // Perform single api request and update result
            RequestHandlerOutput.Results[runId] = await RequestHandlerService.QueryEndpoint(SwaggerProcessorOutput.Requests[runId]);

            // Update Compare Result
            ComparerOutput.Results[runId] = CompareService.CompareResponse(RequestHandlerOutput.Results[runId]);

            // Get Content Block to display in page
            var result = UIGeneratorService.GetSinglePageContent(ComparerOutput.Results[runId]);

            return result;
        }

        private static RefactorHelperSettings SetDefaults(RefactorHelperSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.OutputFolder))
                settings.OutputFolder = $"{GetBinPath()}/Files/Output/";

            if (string.IsNullOrWhiteSpace(settings.ContentFolder))
                settings.ContentFolder = $"{GetBinPath()}/Files/Content/";

            return settings;
        }

        private static string GetBinPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
    }
}
