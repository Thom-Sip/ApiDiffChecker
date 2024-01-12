using ApiDiffChecker.Models.Settings;
using Newtonsoft.Json;
using ApiDiffChecker.Features.Comparer;
using ApiDiffChecker.Features.RequestHandler;
using ApiDiffChecker.Features.SwaggerProcessor;
using ApiDiffChecker.Features.UIGenerator;
using ApiDiffChecker.Models.State;

namespace ApiDiffChecker
{
    public class RefactorHelperApp(
        ApiDiffCheckerSettings settings,
        ApiDiffCheckerState state,
        SwaggerProcessorService swaggerProcessorService,
        RequestHandlerService requestHandlerService,
        CompareService compareService,
        ContentGeneratorService uiGeneratorService,
        SidebarGeneratorService sidebarGeneratorService,
        Formbuilder formbuilder)
    {
        public ApiDiffCheckerSettings Settings { get; set; } = settings;
        public ApiDiffCheckerState State { get; set; } = state;
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
            // State
            State.CurrentRequest = 0;
            State.CurrentRun = null;

            // Process Swagger
            var result = await Settings.HttpClient1.GetAsync(Settings.GetSwaggerUrl());
            State.SwaggerJson = await result.Content.ReadAsStringAsync();

            // Get requests from Swagger
            State.SwaggerOutput = SwaggerProcessorService.GetQueryParamsFromSwagger(State.SwaggerJson);
            State.Data = SwaggerProcessorService.ProcessSwagger(State.SwaggerJson);

            // Generate html output
            UIGeneratorService.GenerateBaseUI();
        }

        public void ProcessSettings()
        {
            // Combine Settings with State to generate the Final Requests
            State.Data = SwaggerProcessorService.ProcessSwagger(State.SwaggerJson);

            // Generate html output
            UIGeneratorService.GenerateBaseUI();
        }

        public async Task<string> RunAll()
        {
            // Clear previous run
            State.Data.ForEach(x => x.Clear());

            // Perform api Requests
            await RequestHandlerService.QueryApis();

            // Get diffs on responses
            CompareService.CompareResponses();

            // Generate output
            UIGeneratorService.GenerateBaseUI();

            return UIGeneratorService.GetTestResultFragment();
        }

        public async Task<string> RetryCurrentRequestFragment()
        {
            // Clear previous result
            State.Data[State.CurrentRequest].Clear();

            // Perform single api request and update result
            await RequestHandlerService.QueryEndpoint(State.GetCurrentRequest());

            // Update Compare Result
            CompareService.CompareResponse(State.GetCurrentRequest());

            // Get Content Block to display in page
            return UIGeneratorService.GetTestResultFragment();
        }

        public void ClearEmptyReplaceValues(int? runId)
        {
            if (runId == null)
            {
                Settings.DefaultRunSettings.PropertiesToReplace = 
                    Settings.DefaultRunSettings.PropertiesToReplace
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key)).ToList();

                return;
            } 

            Settings.Runs[runId.Value].PropertiesToReplace = 
                Settings.Runs[runId.Value].PropertiesToReplace
                .Where(x => !string.IsNullOrWhiteSpace(x.Key)).ToList();
        }

        public void AddRun() => Settings.Runs.Add(new());

        public void DuplicateRun(int? runId)
        {
            if (runId == null)
            {
                Settings.Runs.Add(CopyItem(Settings.DefaultRunSettings));
                return;
            }

            Settings.Runs.Add(CopyItem(Settings.Runs[runId.Value]));
        }

        public T CopyItem<T>(T item)
        {
            var json = JsonConvert.SerializeObject(item);
            return JsonConvert.DeserializeObject<T>(json);
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
