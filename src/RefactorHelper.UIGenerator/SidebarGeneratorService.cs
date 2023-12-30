using RefactorHelper.Models.Config;
using RefactorHelper.Models;
using System.Text;

namespace RefactorHelper.UIGenerator
{
    public class SidebarGeneratorService(
        RefactorHelperSettings settings,
        RefactorHelperState state) : BaseContentGenerator(settings, state)
    {
        protected string _sideBarGroupTemplate { get; set; } = File.ReadAllText($"{settings.ContentFolder}/SideBarGroup.html");
        protected string _sideBarGroupItemTemplate { get; set; } = File.ReadAllText($"{settings.ContentFolder}/Components/SidebarContainerItem.html");
        protected string _sideBarGroupItemTemplateWithDelete { get; set; } = File.ReadAllText($"{settings.ContentFolder}/Components/SidebarContainerItemWithDelete.html");
        protected string _sideBarDownloadTemplate { get; set; } = File.ReadAllText($"{settings.ContentFolder}/Components/SidebarDownloadItem.html");

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
            sb.Append(_sideBarDownloadTemplate);
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
                .Replace("[TEXT]", "Default Values"));

            for (int i = 0; i < Settings.Runs.Count; i++)
            {
                var template = i == 0
                    ? _sideBarGroupItemTemplate
                    : _sideBarGroupItemTemplateWithDelete;

                sb.Append(template
                    .Replace("[CSS_CLASS]", runId == i ? "request-item-active" : "request-item")
                    .Replace("[GET_URL]", $"{Url.Fragment.RunSettings}/{i}")
                    .Replace("[SET_URL]", $"{Url.Page.RunSettings}/{i}")
                    .Replace("[DELETE_URL]", $"{Url.Fragment.SideBarSettingsRemoveRun}/{i}")
                    .Replace("[HX_TARGET]", Section.MainContent)
                    .Replace("[HX_DELETE_TARGET]", Section.SideBar)
                    .Replace("[LI-ID]", $"run-button-{i}")
                    .Replace("[TEXT]", $"Run {i}"));
            }

            sb.Append(_sideBarGroupItemTemplate
                    .Replace("[CSS_CLASS]", "request-item")
                    .Replace("[GET_URL]", Url.Fragment.SideBarSettingsAddRun)
                    .Replace("[SET_URL]", "")
                    .Replace("[HX_TARGET]", Section.SideBar)
                    .Replace("[TEXT]", $"<b>+</b> Add Run"));

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
                sb.Append(_sideBarGroupItemTemplate
                    .Replace("[CSS_CLASS]", "request-item")
                    .Replace("[GET_URL]", $"{Url.Fragment.TestResult}/{item.Id}")
                    .Replace("[SET_URL]", $"{Url.Fragment.TestResult}/{item.Id}")
                    .Replace("[HX_TARGET]", Section.MainContent)
                    .Replace("[TEXT]", $"{GetResultCode(item.TestResult?.Result1)} {item.Request.Path}"));
            }

            sb.Append("</ul>");
            return sb.ToString();
        }
    }
}
