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
        ContentGeneratorService uiGeneratorService,
        SidebarGeneratorService sidebarGeneratorService,
        Formbuilder formbuilder)
    {
        public RefactorHelperSettings Settings { get; set; } = settings;
        public RefactorHelperState State { get; set; } = state;
        public SwaggerProcessorService SwaggerProcessorService { get; set; } = swaggerProcessorService;
        public RequestHandlerService RequestHandlerService { get; set; } = requestHandlerService;
        public CompareService CompareService { get; set; } = compareService;
        public ContentGeneratorService UIGeneratorService { get; set; } = uiGeneratorService;
        public SidebarGeneratorService SidebarGeneratorService { get; set; } = sidebarGeneratorService;
        public Formbuilder Formbuilder { get; set; } = formbuilder;

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

        public void SaveForm(FormType formType, IFormCollection form, int? runId)
        {
            var run = GetRun(runId);

            switch (formType)
            {
                case FormType.UrlParameters:
                    run.UrlParameters.Clear();
                    foreach (var formfield in form.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
                    {
                        var param = run.UrlParameters.FirstOrDefault(x => x.Key == formfield.Key);

                        if (param != null)
                        {
                            param.Value = formfield.Value.ToString();
                            continue;
                        }

                        run.UrlParameters.Add(new Parameter(formfield.Key, formfield.Value.ToString()));
                    }
                    break;

                case FormType.QueryParameters:
                    run.QueryParameters.Clear();
                    foreach (var formfield in form.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
                    {
                        var param = run.QueryParameters.FirstOrDefault(x => x.Key == formfield.Key);

                        if (param != null)
                        {
                            param.Value = formfield.Value.ToString();
                            continue;
                        }

                        run.QueryParameters.Add(new Parameter(formfield.Key, formfield.Value.ToString()));
                    }
                    break;
            }
        }

        private Run GetRun(int? runId)
        {
            if(runId == null)
                return Settings.DefaultRunSettings;

            return Settings.Runs[runId.Value];
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
