using RefactorHelper.Comparer;
using RefactorHelper.RequestHandler;
using RefactorHelper.UIGenerator;
using RefactorHelper.SwaggerProcessor;
using RefactorHelper.Models.Config;
using RefactorHelper.Models;
using Microsoft.AspNetCore.Http;

namespace RefactorHelper.App
{
    public class RefactorHelperApp(
        RefactorHelperSettings settings,
        RefactorHelperState state,
        SwaggerProcessorService swaggerProcessorService,
        RequestHandlerService requestHandlerService,
        CompareService compareService,
        UIGeneratorService uiGeneratorService)
    {
        public RefactorHelperSettings Settings { get; set; } = settings;
        public RefactorHelperState State { get; set; } = state;
        public SwaggerProcessorService SwaggerProcessorService { get; set; } = swaggerProcessorService;
        public RequestHandlerService RequestHandlerService { get; set; } = requestHandlerService;
        public CompareService CompareService { get; set; } = compareService;
        public UIGeneratorService UIGeneratorService { get; set; } = uiGeneratorService;

        public async Task Initialize()
        {
            if(!State.Initialized)
            {
                await Reset();

                // Run Once
                State.Initialized = true;
            }
        }

        public async Task Reset()
        {
            // Process Swagger
            var result = await Settings.HttpClient1.GetAsync(Settings.GetSwaggerUrl());
            State.SwaggerJson = await result.Content.ReadAsStringAsync();

            // Get requests from Swagger
            State.SwaggerOutput = SwaggerProcessorService.GetQueryParamsFromSwagger(State.SwaggerJson);
            State.Data = SwaggerProcessorService.ProcessSwagger(State.SwaggerJson);

            // Generate html output
            UIGeneratorService.GenerateBaseUI(State);
        }

        public void ProcessSettings()
        {
            // Combine Settings with State to generate the Final Requests
            State.Data = SwaggerProcessorService.ProcessSwagger(State.SwaggerJson);

            // Generate html output
            UIGeneratorService.GenerateBaseUI(State);
        }

        public async Task<string> RunAll()
        {
            // Perform api Requests
            await RequestHandlerService.QueryApis(State);

            // Get diffs on responses
            CompareService.CompareResponses(State);

            // Generate output
            UIGeneratorService.GenerateBaseUI(State);

            return UIGeneratorService.GetTestResultFragment();
        }

        public async Task<string> RetryCurrentRequestFragment()
        {
            // Perform single api request and update result
            await RequestHandlerService.QueryEndpoint(State.GetCurrentRequest());

            // Update Compare Result
            CompareService.CompareResponse(State.GetCurrentRequest());

            // Get Content Block to display in page
            return UIGeneratorService.GetTestResultFragment();
        }

        public void SaveUrlParams(IFormCollection form)
        {
            foreach(var formfield in form)
            {
                var param = Settings.DefaultParameters.FirstOrDefault(x => x.Key == formfield.Key);

                if (param != null)
                {
                    param.Value = formfield.Value.ToString();
                    continue;
                }

                Settings.DefaultParameters.Add(new Parameter(formfield.Key, formfield.Value.ToString()));
            }
        }

        public string GetContentFile(string filename) => File.ReadAllText(Path.Combine(Settings.ContentFolder, filename));

        #region Test Functions
        public async Task<string> StaticCompare(string fileOne, string fileTwo)
        {
            var file1 = File.ReadAllText(Path.Combine(Settings.ContentFolder, fileOne));
            var file2 = File.ReadAllText(Path.Combine(Settings.ContentFolder, fileTwo));

            var compareResultPair = CompareService.GetCompareResultPair(file1, file2);

            var html = UIGeneratorService.GetHtmlPage(compareResultPair);

            return html;
        }
        #endregion
    }
}
