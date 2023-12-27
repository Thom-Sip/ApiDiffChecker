using Microsoft.AspNetCore.Http;
using RefactorHelper.Models;
using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.Config;
using RefactorHelper.Models.External;
using RefactorHelper.Models.RequestHandler;
using RefactorHelper.Models.SwaggerProcessor;
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
        protected string _settingsFragmentTemplate { get; set; }

        protected string _requestListHtml { get; set; } = string.Empty;

        protected Formbuilder Formbuilder { get; set; }

        private RefactorHelperState State { get; set; }

        public UIGeneratorService(RefactorHelperSettings settings, RefactorHelperState state)
        {
            State = state;

            _template = File.ReadAllText($"{settings.ContentFolder}/Template.html");
            _contentTemplate = File.ReadAllText($"{settings.ContentFolder}/ContentTemplate.html");
            _diffBoxTemplate = File.ReadAllText($"{settings.ContentFolder}/DiffBoxTemplate.html");
            _sideBarGroupTemplate = File.ReadAllText($"{settings.ContentFolder}/SideBarGroup.html");
            _settingsFragmentTemplate = File.ReadAllText($"{settings.ContentFolder}/Settings/SettingsFragment.html");
            _outputFolder = settings.OutputFolder;
            _runfolder = settings.OutputFolder;

            Formbuilder = new Formbuilder(settings.ContentFolder, settings);

            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);
        }

        public void GenerateBaseUI(RefactorHelperState state)
        {
            GenerateRequestListHtml(state.Data);

            state.BaseHtmlTemplate = new HtmlTemplate
            {
                Html = _template
                    .Replace("[RETRY_REQUEST_URL]", "/run-refactor-helper/fragment/retry")
                    .Replace("[RETRY_ALL_URL]", "/run-refactor-helper/fragment/run-all")
                    .Replace("[RESET_URL]", "/run-refactor-helper/reset")
                    .Replace("[REQUEST_LIST_URL]", "/run-refactor-helper/fragment/request-list")
                    .Replace("[SETTINGS_URL]", "/run-refactor-helper/settings")
                    .Replace("[SETTINGS_FRAGMENT_URL]", "/run-refactor-helper/fragment/settings")
            };
        }

        public string GetHtmlPage(CompareResultPair resultPair)
        {
            return _template.Replace("[CONTENT_BLOCK]", GetContent(resultPair, resultPair.Diffs, null));
        }

        public string GetTestResultPage(int? requestId = null) => 
            State.BaseHtmlTemplate.SetContent(GetTestResultFragment(requestId));

        public string GetTestResultFragment(int? requestId = null)
        {
            State.CurrentRequest = requestId ?? State.CurrentRequest;
            var content = GetContent(State.GetCurrentRequest());
            GenerateRequestListHtml(State.Data);
            return content;
        }

        public string GetSettingsPage() => State.BaseHtmlTemplate.SetContent(GetSettingsFragment());

        public string GetSettingsFragment()
        {
            var result = _settingsFragmentTemplate
                .Replace("[URL_PARAMETERS]", GetFormFragment(FormType.UrlParameters, false))
                .Replace("[QUERY_PARAMETERS]", GetFormFragment(FormType.QueryParameters, false));

            return result;
        }

        public string GetFormFragment(FormType formType, bool allowEdit)
        {
            return Formbuilder.GetForm(
                    GetFormData(State.SwaggerOutput, formType),
                    $"/run-refactor-helper/fragment/save/{formType}",
                    $"/run-refactor-helper/fragment/settings/{formType}?allowEdit={!allowEdit}", allowEdit);
        }

        private static List<Parameter> GetFormData(SwaggerProcessorOutput swaggerOutput, FormType formType)
        {
            return formType switch
            {
                FormType.QueryParameters => swaggerOutput.QueryParameters,
                FormType.UrlParameters => swaggerOutput.UrlParameters,
                _ => throw new NotImplementedException()
            };
        }

        public string GetRequestListFragment() => _requestListHtml;

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

        private void GenerateRequestListHtml(List<RequestWrapper> wrappers)
        {
            var pendingRequests = wrappers.Where(x => !x.Executed).ToList();
            var failedRequests = wrappers.Where(x => x.Changed && x.Executed).ToList();
            var successfulRequest = wrappers.Where(x => !x.Changed && x.Executed).ToList();

            _requestListHtml = string.Empty;

            if (pendingRequests.Count > 0)
                _requestListHtml = $"{_requestListHtml}{GenerateRequestList(pendingRequests, "Pending Requests")}";

            if (failedRequests.Count > 0)
                _requestListHtml = $"{_requestListHtml}{GenerateRequestList(failedRequests, "Failed Requests")}";

            if (successfulRequest.Count > 0)
                _requestListHtml = $"{_requestListHtml}{GenerateRequestList(successfulRequest, "Success Requests")}";
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
                sb.Append(
                    $"<li>" +
                        $"<span class=\"request-item\" " +
                              $"hx-get=\"/run-refactor-helper/fragment/{item.Id}\" " +
                              $"hx-replace-url=\"/run-refactor-helper/{item.Id}\" " +
                              $"hx-swap=\"innerHTML\" " +
                              $"hx-target=\"#main-content\">" +
                              $"{GetResultCode(item.TestResult?.Result1)} {item.Request.Path}" +
                        $"</span>" +
                    $"</li>");
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
