using RefactorHelper.Comparer;
using RefactorHelper.RequestHandler;
using RefactorHelper.UIGenerator;
using RefactorHelper.SwaggerProcessor;
using RefactorHelper.Models.Config;
using RefactorHelper.Models;
using Microsoft.AspNetCore.Http;
using RefactorHelper.Models.Uigenerator;

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
        private RefactorHelperSettings Settings { get; set; } = settings;
        private SwaggerProcessorService SwaggerProcessorService { get; set; } = swaggerProcessorService;
        private RequestHandlerService RequestHandlerService { get; set; } = requestHandlerService;
        private CompareService CompareService { get; set; } = compareService;
        private UIGeneratorService UIGeneratorService { get; set; } = uiGeneratorService;
        private RefactorHelperState State { get; set; } = state;

        public async Task Initialize(HttpContext httpContext)
        {
            if(!State.Initialized)
            {
                await Reset(httpContext);

                // Run Once
                State.Initialized = true;
            }
        }

        public async Task Reset(HttpContext httpContext)
        {
            // Process Swagger
            var client = new HttpClient();
            var result = await client.GetAsync(GetSwaggerUrl(httpContext));
            State.SwaggerJson = await result.Content.ReadAsStringAsync();

            // Get requests from Swagger
            State.SwaggerOutput = SwaggerProcessorService.GetQueryParamsFromSwagger(State.SwaggerJson);
            State.Data = SwaggerProcessorService.ProcessSwagger(State.SwaggerJson);

            // Generate html output
            UIGeneratorService.GenerateBaseUI(State, httpContext);
        }

        public void ProcessSettings(HttpContext httpContext)
        {
            // Combine Settings with State to generate the Final Requests
            State.Data = SwaggerProcessorService.ProcessSwagger(State.SwaggerJson);

            // Generate html output
            UIGeneratorService.GenerateBaseUI(State, httpContext);
        }

        public string GetDashboard()
        {
            return State.BaseHtmlTemplate.SetContent("");
        }

        public string GetResultPage(int requestId, HttpContext httpContext)
        {
            State.CurrentRequest = requestId;
            var content = UIGeneratorService.GetTestResultFragment(State.GetCurrentRequest(), State, httpContext);
            return State.BaseHtmlTemplate.SetContent(content);
        }

        public string GetSettingsPage(HttpContext httpContext)
        {
            var content = UIGeneratorService.GetSettingsFragment(State.SwaggerOutput);
            return State.BaseHtmlTemplate.SetContent(content);
        }

        public string GetSettingsFragment(HttpContext httpContext) =>
            UIGeneratorService.GetSettingsFragment(State.SwaggerOutput);

        public string GetFormFragment(FormType formType, bool allowEdit) =>
            UIGeneratorService.GetFormFragment(State.SwaggerOutput, allowEdit, formType);

        public async Task<string> StaticCompare(string fileOne, string fileTwo)
        {
            var file1 = File.ReadAllText(Path.Combine(Settings.ContentFolder, fileOne));
            var file2 = File.ReadAllText(Path.Combine(Settings.ContentFolder, fileTwo));

            var compareResultPair = CompareService.GetCompareResultPair(file1, file2);

            var html = UIGeneratorService.GetHtmlPage(compareResultPair);

            return html;
        }

        public async Task<string> RunAll(HttpContext httpContext)
        {
            // Perform api Requests
            await RequestHandlerService.QueryApis(State);

            // Get diffs on responses
            CompareService.CompareResponses(State);

            // Generate output
            UIGeneratorService.GenerateBaseUI(State, httpContext);

            return UIGeneratorService.GetTestResultFragment(State.GetCurrentRequest(), State, httpContext);
        }

        public string GetResultFragment(HttpContext httpContext, int requestId)
        {
            State.CurrentRequest = requestId;
            return UIGeneratorService.GetTestResultFragment(State.GetCurrentRequest(), State, httpContext);
        }

        public async Task<string> RetryCurrentRequestFragment(HttpContext httpContext)
        {
            // Perform single api request and update result
            await RequestHandlerService.QueryEndpoint(State.GetCurrentRequest());

            // Update Compare Result
            CompareService.CompareResponse(State.GetCurrentRequest());

            // Get Content Block to display in page
            return UIGeneratorService.GetTestResultFragment(State.GetCurrentRequest(), State, httpContext);
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

        public string GetRequestListFragment() => UIGeneratorService.GetRequestListFragment();

        public string GetContentFile(string filename) => File.ReadAllText(Path.Combine(Settings.ContentFolder, filename));

        private string GetSwaggerUrl(HttpContext httpContext)
        {
            if (!string.IsNullOrWhiteSpace(Settings.SwaggerUrl))
                return Settings.SwaggerUrl;

            return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}/swagger/v1/swagger.json";
        }
    }
}
