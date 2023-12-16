using RefactorHelper.Comparer;
using RefactorHelper.RequestHandler;
using RefactorHelper.UIGenerator;
using RefactorHelper.SwaggerProcessor;
using RefactorHelper.Models.Config;

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
                }
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
            var swaggerProcessorOuput = SwaggerProcessorService.ProcessSwagger(SwaggerJson);

            // Perform api Requests
            var requestHandlerOutput = await RequestHandlerService.QueryApis(swaggerProcessorOuput);

            // Get diffs on responses
            var ComparerOutput = CompareService.CompareResponses(requestHandlerOutput);

            // Generate output
            var outputFileNames = UIGeneratorService.GenerateUI(ComparerOutput);

            return outputFileNames;
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
