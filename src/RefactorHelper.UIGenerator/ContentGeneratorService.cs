﻿using RefactorHelper.Models;
using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.Config;
using RefactorHelper.Models.External;
using RefactorHelper.Models.Uigenerator;
using System.Text;

namespace RefactorHelper.UIGenerator
{
    public class ContentGeneratorService(
        RefactorHelperSettings settings,
        RefactorHelperState state,
        SidebarGeneratorService sidebarGeneratorService,
        Formbuilder formBuilder) : BaseContentGenerator(settings, state)
    {
        protected string _template { get; set; } = File.ReadAllText($"{settings.ContentFolder}/Template.html");
        protected string _contentTemplate { get; set; } = File.ReadAllText($"{settings.ContentFolder}/ContentTemplate.html");
        protected string _diffBoxTemplate { get; set; } = File.ReadAllText($"{settings.ContentFolder}/DiffBoxTemplate.html");
        protected string _settingsFragmentTemplate { get; set; } = File.ReadAllText($"{settings.ContentFolder}/Settings/SettingsFragment.html");

        protected Formbuilder Formbuilder { get; set; } = formBuilder;
        protected SidebarGeneratorService SidebarGeneratorService { get; set; } = sidebarGeneratorService;

        public void GenerateBaseUI(RefactorHelperState state)
        {
            SidebarGeneratorService.GenerateRequestSideBarHtml(state.Data);
            SidebarGeneratorService.GenerateSettingsSideBarFragment(null);

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
            .SetSideBar(SidebarGeneratorService.GetRequestListFragment())
            .Html;

        public string GetTestResultPage(int? requestId = null) => State.BaseHtmlTemplate
            .SetContent(GetTestResultFragment(requestId))
            .SetSideBar(SidebarGeneratorService.GetRequestListFragment())
            .Html;

        public string GetTestResultFragment(int? requestId = null)
        {
            State.CurrentRequest = requestId ?? State.CurrentRequest;
            var content = GetContent(State.GetCurrentRequest());
            SidebarGeneratorService.GenerateRequestSideBarHtml(State.Data);
            return content;
        }

        public string GetSettingsPage() => State.BaseHtmlTemplate
            .SetContent(GetSettingsFragment())
            .SetSideBar(SidebarGeneratorService.GetSettingsSideBarFragment())
            .Html;

        public string GetRunSettingsPage(int runId) => State.BaseHtmlTemplate
           .SetContent(GetSettingsFragment(runId))
           .SetSideBar(SidebarGeneratorService.GetSettingsSideBarFragment())
           .Html;

        public string GetSettingsFragment(int? runId = null)
        {
            State.CurrentRun = runId;

            var result = _settingsFragmentTemplate
                .Replace("[URL_PARAMETERS]", GetFormFragment(FormType.UrlParameters, false, runId))
                .Replace("[QUERY_PARAMETERS]", GetFormFragment(FormType.QueryParameters, false, runId))
                .Replace("[TITLE]", GetSettingsTitle(runId))
                .Replace("[TEXT]", GetSettingsText(runId))
                .Replace("[BUTTON-TEXT]", GetSettingsCopyButtontext(runId));

            return result;
        }

        private static string GetSettingsTitle(int? runId)
        {
            return runId == null
                ? "Default Parameters"
                : $"Run {runId}";
        }

        private static string GetSettingsText(int? runId)
        {
            return runId == null
                ? "These values will be used to replace all url parameters and query string parameters found in your swagger."
                : "For every Run RefactorHelper will generate api-requests using the run's parameters. If a parameter is not found in the run, the Default Parameters will be used instead.";
        }

        private static string GetSettingsCopyButtontext(int? runId)
        {
            return runId == null
                ? "Copy to new Run"
                : "Duplicate Run";
        }

        public string GetFormFragment(FormType formType, bool allowEdit, int? runId = null)
        {
            var getUrl = $"{Url.Fragment.FormPut}/{formType}";
            var putUrl = $"{Url.Fragment.FormGet}/{formType}?allowEdit={!allowEdit}";

            if(runId != null)
            {
                getUrl = $"{Url.Fragment.FormPut}/{formType}?runId={runId}";
                putUrl = $"{Url.Fragment.FormGet}/{formType}?runId={runId}&allowEdit={!allowEdit}";
            }

            return Formbuilder.GetForm(
                    GetFormData(formType, runId),
                    GetDefaultValues(formType),
                    getUrl, putUrl, allowEdit);
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
    }
}
