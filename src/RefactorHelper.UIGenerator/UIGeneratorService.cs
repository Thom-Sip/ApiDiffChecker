using RefactorHelper.Models;
using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.Config;
using RefactorHelper.Models.External;
using RefactorHelper.Models.RequestHandler;
using RefactorHelper.Models.Uigenerator;
using System.Text;

namespace RefactorHelper.UIGenerator
{
    public class UIGeneratorService
    {
        protected string _outputFolder { get; set; }
        protected string _runfolder { get; set; }
        protected string _template { get; set; }
        protected string _contentTemplate { get; set; }
        protected string _diffBoxTemplate { get; set; }
        protected string _sideBarGroupTemplate { get; set; }
        protected string _sideBarGroupItemTemplate { get; set; }
        protected string _sideBarDownloadTemplate { get; set; }
        protected string _settingsFragmentTemplate { get; set; }

        protected string _requestsSidebarHtml { get; set; } = string.Empty;
        protected string _settingsSidebarHtml { get; set; } = string.Empty;

        protected Formbuilder Formbuilder { get; set; }

        private RefactorHelperState State { get; set; }

        private RefactorHelperSettings Settings { get; set; }

        public UIGeneratorService(RefactorHelperSettings settings, RefactorHelperState state)
        {
            Settings = settings;
            State = state;

            _template = File.ReadAllText($"{settings.ContentFolder}/Template.html");
            _contentTemplate = File.ReadAllText($"{settings.ContentFolder}/ContentTemplate.html");
            _diffBoxTemplate = File.ReadAllText($"{settings.ContentFolder}/DiffBoxTemplate.html");
            _sideBarGroupTemplate = File.ReadAllText($"{settings.ContentFolder}/SideBarGroup.html");
            _sideBarGroupItemTemplate = File.ReadAllText($"{settings.ContentFolder}/Components/SidebarContainerItem.html");
            _sideBarDownloadTemplate = File.ReadAllText($"{settings.ContentFolder}/Components/SidebarDownloadItem.html");
            _settingsFragmentTemplate = File.ReadAllText($"{settings.ContentFolder}/Settings/SettingsFragment.html");
            _outputFolder = settings.OutputFolder;
            _runfolder = settings.OutputFolder;

            Formbuilder = new Formbuilder(settings.ContentFolder, settings);

            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);
        }

        public void GenerateBaseUI(RefactorHelperState state)
        {
            GenerateRequestSideBarHtml(state.Data);
            GenerateSettingsSideBarFragment();

            state.BaseHtmlTemplate = new HtmlTemplate
            {
                Html = _template
                    .Replace("[RETRY_REQUEST_URL]", Url.Fragment.RetryCurrentRequest)
                    .Replace("[RETRY_ALL_URL]", Url.Fragment.RunAll)
                    .Replace("[RESET_URL]", Url.Page.Reset)
                    .Replace("[REQUEST_LIST_URL]", Url.Fragment.SideBarRequests)
                    .Replace("[SETTINGS_LIST_URL]", Url.Fragment.SideBarSettings)
                    .Replace("[SETTINGS_URL]", Url.Page.Settings)
                    .Replace("[SETTINGS_FRAGMENT_URL]", Url.Fragment.Settings)
                    .Replace("[REQUEST_FRAGMENT_URL]", $"{Url.Fragment.TestResult}/0")
                    .Replace("[ROOT_URL]", Url.Page.Root)
            };
        }

        public string GetHtmlPage(CompareResultPair resultPair)
        {
            return _template.Replace("[CONTENT_BLOCK]", GetContent(resultPair, resultPair.Diffs, null));
        }

        public string GetEmptyRequestPage() => State.BaseHtmlTemplate
            .SetContent("")
            .SetSideBar(GetRequestListFragment())
            .Html;

        public string GetTestResultPage(int? requestId = null) => State.BaseHtmlTemplate
            .SetContent(GetTestResultFragment(requestId))
            .SetSideBar(GetRequestListFragment())
            .Html;

        public string GetTestResultFragment(int? requestId = null)
        {
            State.CurrentRequest = requestId ?? State.CurrentRequest;
            var content = GetContent(State.GetCurrentRequest());
            GenerateRequestSideBarHtml(State.Data);
            return content;
        }

        public string GetSettingsPage() => State.BaseHtmlTemplate
            .SetContent(GetSettingsFragment())
            .SetSideBar(GetSettingsSideBarFragment())
            .Html;

        public string GetSettingsFragment(int runId)
        {
            var result = _settingsFragmentTemplate
                .Replace("[URL_PARAMETERS]", GetFormFragment(FormType.UrlParameters, false, runId))
                .Replace("[QUERY_PARAMETERS]", GetFormFragment(FormType.QueryParameters, false, runId));

            return result;
        }

        public string GetSettingsFragment()
        {
            var result = _settingsFragmentTemplate
                .Replace("[URL_PARAMETERS]", GetFormFragment(FormType.UrlParameters, false))
                .Replace("[QUERY_PARAMETERS]", GetFormFragment(FormType.QueryParameters, false));

            return result;
        }

        public string GetFormFragment(FormType formType, bool allowEdit, int? runId = null)
        {
            return Formbuilder.GetForm(
                    GetFormData(formType, runId),
                    GetDefaultValues(formType),
                    $"{Url.Fragment.FormPut}/{formType}",
                    $"{Url.Fragment.FormGet}/{formType}?allowEdit={!allowEdit}", allowEdit);
        }

        private List<Parameter> GetFormData(FormType formType, int? runId)
        {
            if(runId != null)
            {
                return formType switch
                {
                    FormType.QueryParameters => Settings.Runs[runId.Value].QueryParameters,
                    FormType.UrlParameters => Settings.Runs[runId.Value].UrlParameters,
                    _ => throw new NotImplementedException()
                };
            }

            return formType switch
            {
                FormType.QueryParameters => Settings.DefaultRunSettings.QueryParameters,
                FormType.UrlParameters => Settings.DefaultRunSettings.UrlParameters,
                _ => throw new NotImplementedException()
            };
        }

        private List<Parameter> GetDefaultValues(FormType formType)
        {
            return formType switch
            {
                FormType.QueryParameters => State.SwaggerOutput.QueryParameters,
                FormType.UrlParameters => State.SwaggerOutput.UrlParameters,
                _ => throw new NotImplementedException()
            }; ;
        }

        public string GetSettingsSideBarFragment() => _settingsSidebarHtml;

        public string GenerateSettingsSideBarFragment()
        {
            var sb = new StringBuilder();

            sb.Append(_sideBarGroupTemplate
              .Replace("[TITLE]", $"Settings")
              .Replace("[CONTENT]", GetSidebarSettingsFragment()));

            sb.Append(_sideBarGroupTemplate
              .Replace("[TITLE]", $"Parameters")
              .Replace("[CONTENT]", GetSidebarParametersFragment()));

            _settingsSidebarHtml = sb.ToString();
            return _settingsSidebarHtml;
        }

        private string GetSidebarSettingsFragment()
        {
            var sb = new StringBuilder();
            sb.Append("<ul>");

            sb.Append(_sideBarDownloadTemplate);

            sb.Append("</ul>");
            return sb.ToString();
        }

        private string GetSidebarParametersFragment()
        {
            var sb = new StringBuilder();
            sb.Append("<ul>");

            sb.Append(_sideBarGroupItemTemplate
                .Replace("[GET_URL]", Url.Fragment.Settings)
                .Replace("[SET_URL]", Url.Page.Settings)
                .Replace("[HX_TARGET]", "#main-content")
                .Replace("[TEXT]", "Default Values"));

            for(int i = 0; i < Settings.Runs.Count; i++)
            {
                sb.Append(_sideBarGroupItemTemplate
                    .Replace("[GET_URL]", $"{Url.Fragment.RunSettings}/{i}")
                    .Replace("[SET_URL]", $"{Url.Page.Settings}/{i}")
                    .Replace("[HX_TARGET]", "#main-content")
                    .Replace("[TEXT]", $"Run {i}"));
            }

            sb.Append(_sideBarGroupItemTemplate
                    .Replace("[GET_URL]", Url.Fragment.SideBarSettingsAddRun)
                    .Replace("[SET_URL]", "")
                    .Replace("[HX_TARGET]", "#side-bar-content")
                    .Replace("[TEXT]", $"+ Add Run"));

            sb.Append("</ul>");
            return sb.ToString();
        }

        public string GetRequestListFragment() => _requestsSidebarHtml;

        private string GetContent(RequestWrapper wrapper) =>
            GetContent(wrapper.CompareResultPair, wrapper.CompareResultPair?.Diffs ?? [], wrapper);

        private string GetContent(CompareResultPair? compareResultPair, List<Diff> diffs, RequestWrapper? wrapper)
        {
            var original = diff_prettyHtml_custom(compareResultPair?.Result1, diffs, wrapper, [Operation.EQUAL, Operation.DELETE]);
            var changed = diff_prettyHtml_custom(compareResultPair?.Result1, diffs, wrapper, [Operation.EQUAL, Operation.INSERT]);

            return _contentTemplate
                .Replace("[CONTENT_ORIGINAL]", original)
                .Replace("[CONTENT_CHANGED]", changed);
        }

        private void GenerateRequestSideBarHtml(List<RequestWrapper> wrappers)
        {
            var pendingRequests = wrappers.Where(x => !x.Executed).ToList();
            var failedRequests = wrappers.Where(x => x.Changed && x.Executed).ToList();
            var successfulRequest = wrappers.Where(x => !x.Changed && x.Executed).ToList();

            _requestsSidebarHtml = string.Empty;

            if (pendingRequests.Count > 0)
                _requestsSidebarHtml = $"{_requestsSidebarHtml}{GenerateRequestList(pendingRequests, "Pending Requests")}";

            if (failedRequests.Count > 0)
                _requestsSidebarHtml = $"{_requestsSidebarHtml}{GenerateRequestList(failedRequests, "Failed Requests")}";

            if (successfulRequest.Count > 0)
                _requestsSidebarHtml = $"{_requestsSidebarHtml}{GenerateRequestList(successfulRequest, "Success Requests")}";
        }

        private string GenerateRequestList(List<RequestWrapper> wrappers, string title)
        {
            return _sideBarGroupTemplate
              .Replace("[TITLE]", $"{title} ({wrappers.Count})")
              .Replace("[CONTENT]", GetSidebarContent(wrappers));
        }

        private string GetSidebarContent(List<RequestWrapper> resultPairs)
        {
            var sb = new StringBuilder();
            sb.Append("<ul>");

            foreach(var item in resultPairs)
            {
                sb.Append(_sideBarGroupItemTemplate
                    .Replace("[GET_URL]", $"{Url.Fragment.TestResult}/{item.Id}")
                    .Replace("[SET_URL]", $"{Url.Fragment.TestResult}/{item.Id}")
                    .Replace("[HX_TARGET]", "#main-content")
                    .Replace("[TEXT]", $"{GetResultCode(item.TestResult?.Result1)} {item.Request.Path}"));
            }

            sb.Append("</ul>");
            return sb.ToString();
        }

        private string diff_prettyHtml_custom(CompareResult? result, List<Diff> diffs, RequestWrapper? wrapper, List<Operation> operations)
        {
            StringBuilder sb = new();

            foreach (Diff aDiff in diffs ?? [])
            {
                string text = aDiff.text
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");

                switch (aDiff.operation)
                {
                    case Operation.INSERT:
                        if(operations.Contains(Operation.INSERT))
                            sb.Append("<span class=\"addition\">").Append(text).Append("</span>");
                        break;
                    case Operation.DELETE:
                        if (operations.Contains(Operation.DELETE))
                            sb.Append("<span class=\"removal\">").Append(text).Append("</span>");
                        break;
                    case Operation.EQUAL:
                        if (operations.Contains(Operation.EQUAL))
                            sb.Append("<span>").Append(text).Append("</span>");
                        break;
                }
            }

            var html = _diffBoxTemplate
                  .Replace("[TITLE]", wrapper?.Request.Path)
                  .Replace("[URL]", result?.Response?.RequestMessage?.RequestUri?.ToString() ?? "Pending")
                  .Replace("[RESULTCODE]", GetResultCodeHeaderText(wrapper))
                  .Replace("[CONTENT]", sb.ToString());

            return html;
        }

        private string GetResultCodeHeaderText(RequestWrapper? wrapper)
        {
            if (wrapper?.TestResult?.Result1 != null)
                return $"{GetResultCode(wrapper?.TestResult?.Result1)} {GetResultCodeString(wrapper?.TestResult?.Result1)}";

            return "Pending";
        }

        private string GetResultCode(RefactorTestResult? result)
        {
            var statusCode = result?.ResponseObject?.StatusCode;
            return statusCode != null
                ? ((int)statusCode).ToString()
                : "_";
        }

        private string GetResultCodeString(RefactorTestResult? result)
        {
            var statusCode = result?.ResponseObject?.StatusCode;
            return statusCode != null
                ? $"{statusCode}"
                : "N/A";
        }
    }
}
