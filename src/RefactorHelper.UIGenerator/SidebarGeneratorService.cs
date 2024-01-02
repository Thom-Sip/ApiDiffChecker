using RefactorHelper.Models.Config;
using RefactorHelper.Models;
using System.Text;
using System;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RefactorHelper.UIGenerator
{
    public class SidebarGeneratorService(
        RefactorHelperSettings settings,
        RefactorHelperState state) : BaseContentGenerator(settings, state)
    {
        protected string _sideBarGroupTemplate { get; } = File.ReadAllText($"{settings.ContentFolder}/SideBarGroup.html");
        protected string _sideBarGroupItemTemplate { get; } = File.ReadAllText($"{settings.ContentFolder}/Components/SidebarContainerItem.html");
        protected string _sideBarGroupItemTemplateWithDelete { get; } = File.ReadAllText($"{settings.ContentFolder}/Components/SidebarContainerItemWithDelete.html");
        protected string _sideBarDownloadTemplate { get; } = File.ReadAllText($"{settings.ContentFolder}/Components/SidebarDownloadItem.html");

        public string GetSettingsSideBarFragment() =>
            GenerateSettingsSideBarFragment(State.CurrentRun);

        public string GenerateSettingsSideBarFragment(int? runId)
        {
            var sb = new StringBuilder();

            sb.Append(_sideBarGroupTemplate
              .Replace("[TITLE]", "Settings")
              .Replace("[CONTENT]", GetSidebarSettingsFragment()));

            sb.Append(_sideBarGroupTemplate
              .Replace("[TITLE]", "Parameters")
              .Replace("[CONTENT]", GetSidebarParametersFragment(runId)));

            return sb.ToString();
        }

        private string GetSidebarSettingsFragment()
        {
            var sb = new StringBuilder();
            sb.Append("<ul>");

            sb.Append(_sideBarDownloadTemplate
                .Replace("[TEXT]", "View"));

            sb.Append(_sideBarGroupItemTemplate
              .Replace("[CSS_CLASS]", "request-item")
              .Replace("[GET_URL]", Url.Fragment.SaveSettingsToDisk)
              .Replace("[SET_URL]", Url.Page.Root)
              .Replace("[HX_TARGET]", Section.MainContent)
              .Replace("[TEXT]", "Save to disk")
              .Replace("[STATUS_CODE]", ""));

            sb.Append(_sideBarGroupItemTemplate
              .Replace("[CSS_CLASS]", "request-item")
              .Replace("[GET_URL]", Url.Fragment.ApplySettings)
              .Replace("[SET_URL]", Url.Page.Root)
              .Replace("[HX_TARGET]", Section.MainContent)
              .Replace("[TEXT]", "Apply Settings")
              .Replace("[STATUS_CODE]", ""));

            sb.Append("</ul>");
            return sb.ToString();
        }

        private string GetSidebarParametersFragment(int? runId)
        {
            var sb = new StringBuilder();
            sb.Append("<ul>");

            sb.Append(_sideBarGroupItemTemplate
                .Replace("[CSS_CLASS]", runId == null ? "request-item-active" : "request-item")
                .Replace("[GET_URL]", Url.Fragment.Settings)
                .Replace("[SET_URL]", Url.Page.Settings)
                .Replace("[HX_TARGET]", Section.MainContent)
                .Replace("[TEXT]", "Default Values")
                .Replace("[STATUS_CODE]", ""));

            for (int i = 0; i < Settings.Runs.Count; i++)
            {
                sb.Append(_sideBarGroupItemTemplateWithDelete
                    .Replace("[CSS_CLASS]", runId == i ? "request-item-active" : "request-item")
                    .Replace("[GET_URL]", $"{Url.Fragment.RunSettings}/{i}")
                    .Replace("[SET_URL]", $"{Url.Page.RunSettings}/{i}")
                    .Replace("[DELETE_URL]", $"{Url.Fragment.SideBarSettingsRemoveRun}/{i}")
                    .Replace("[HX_TARGET]", Section.MainContent)
                    .Replace("[HX_DELETE_TARGET]", Section.SideBar)
                    .Replace("[LI-ID]", $"run-button-{i}")
                    .Replace("[TEXT]", $"Run {i}")
                    .Replace("[STATUS_CODE]", ""));
            }

            sb.Append(_sideBarGroupItemTemplate
                    .Replace("[CSS_CLASS]", "request-item")
                    .Replace("[GET_URL]", Url.Fragment.AddNewRun)
                    .Replace("[SET_URL]", $"{Url.Page.RunSettings}/{Settings.Runs.Count}")
                    .Replace("[HX_TARGET]", Section.MainContent)
                    .Replace("[TEXT]", $"<b>+</b> Add Run")
                    .Replace("[STATUS_CODE]", ""));

            sb.Append("</ul>");
            return sb.ToString();
        }

        public string GetRequestListFragment() => GenerateRequestSideBarHtml(State.Data);

        public string GenerateRequestSideBarHtml(List<RequestWrapper> wrappers)
        {
            var pendingRequests = wrappers.Where(x => !x.Executed).ToList();
            var failedRequests = wrappers.Where(x => x.Changed && x.Executed).ToList();
            var successfulRequest = wrappers.Where(x => !x.Changed && x.Executed).ToList();

            var result = string.Empty;

            if (pendingRequests.Count > 0)
                result = $"{result}{GenerateRequestList(pendingRequests, "Pending Requests")}";

            if (failedRequests.Count > 0)
                result = $"{result}{GenerateRequestList(failedRequests, "Failed Requests")}";

            if (successfulRequest.Count > 0)
                result = $"{result}{GenerateRequestList(successfulRequest, "Success Requests")}";

            return result;
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

            foreach (var item in resultPairs)
            {
                var response1 = item.CompareResultPair?.Result1?.Response;
                var resultCode = "...";
                if (response1 != null)
                    resultCode = ((int)response1.StatusCode).ToString();
                var successClass = response1?.IsSuccessStatusCode == true ? "success" : "failed";

                sb.Append(_sideBarGroupItemTemplate
                    .Replace("[CSS_CLASS]", item.Id == State.CurrentRequest ? "request-item-active" : "request-item")
                    .Replace("[GET_URL]", $"{Url.Fragment.TestResult}/{item.Id}")
                    .Replace("[SET_URL]", $"{Url.Page.TestResult}/{item.Id}")
                    .Replace("[HX_TARGET]", Section.MainContent)
                    .Replace("[TEXT]", item.Request.Path)
                    .Replace("[STATUS_CODE]", $"<div class=\"status-code {successClass}\">{resultCode}</div>"));
            }

            sb.Append("</ul>");
            return sb.ToString();
        }
    }
}
