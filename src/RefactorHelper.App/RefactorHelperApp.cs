using RefactorHelper.Comparer;
using RefactorHelper.RequestHandler;
using RefactorHelper.UIGenerator;
using RefactorHelper.SwaggerProcessor;
using RefactorHelper.Models.Config;
using RefactorHelper.Models;
using Microsoft.AspNetCore.Http;

namespace RefactorHelper.App
{
    public class RefactorHelperApp
    {
        private RefactorHelperSettings Settings { get; set; }

        private RefactorHelperState State { get; set; } = new();

        private SwaggerProcessorService SwaggerProcessorService { get; set; }

        private RequestHandlerService RequestHandlerService { get; set; }

        private CompareService CompareService { get; set; }

        private UIGeneratorService UIGeneratorService { get; set; }

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

        public async Task<List<string>> Run(HttpContext httpContext)
        {
            if(string.IsNullOrWhiteSpace(State.SwaggerJson))
            {
                var client = new HttpClient();
                var result = await client.GetAsync(Settings.SwaggerUrl);
                State.SwaggerJson = await result.Content.ReadAsStringAsync();
            }

            // Get requests from swagger
            State.SwaggerProcessorOutput = SwaggerProcessorService.ProcessSwagger(State.SwaggerJson);

            // Perform api Requests
            State.RequestHandlerOutput = await RequestHandlerService.QueryApis(State.SwaggerProcessorOutput);

            // Get diffs on responses
            State.ComparerOutput = CompareService.CompareResponses(State.RequestHandlerOutput);

            // Generate output
            State.OutputFileNames = UIGeneratorService.GenerateUI(State.ComparerOutput, httpContext);

            return State.OutputFileNames;
        }

        public async Task<string> PerformSingleCall(HttpContext context, int requestId)
        {
            // Perform single api request and update result
            State.RequestHandlerOutput.Results[requestId] = await RequestHandlerService.QueryEndpoint(State.SwaggerProcessorOutput.Requests[requestId]);

            // Update Compare Result
            State.ComparerOutput.Results[requestId] = CompareService.CompareResponse(State.RequestHandlerOutput.Results[requestId]);

            // Get Content Block to display in page
            var result = UIGeneratorService.GetSinglePageContent(State.ComparerOutput.Results[requestId], State.ComparerOutput, context);

            return result;
        }

        public string GetRequestListHtml() => UIGeneratorService.GetRequestListHtml();

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
