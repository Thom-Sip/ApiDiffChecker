namespace RefactorHelper.Models
{
    public enum SidebarType
    {
        Requests,
        RequestsPolling,
        Settings
    }

    public static class Url
    {
        public static class Page
        {
            public static string Root { get; } = "/run-refactor-helper";

            public static string TestResult { get; } = "/run-refactor-helper";

            public static string Settings { get; } = "/run-refactor-helper/settings";

            public static string RunSettings { get; } = "/run-refactor-helper/settings/run";

            public static string Reset { get; } = "/run-refactor-helper/reset";
        }

        public static class Fragment
        {
            public static string RunAll { get; } = "/run-refactor-helper/fragment/run-all";

            public static string TestResult { get; } = "/run-refactor-helper/fragment";

            public static string RetryCurrentRequest { get; } = "/run-refactor-helper/fragment/retry";

            public static string Settings { get; } = "/run-refactor-helper/fragment/settings";

            public static string RunSettings { get; } = "/run-refactor-helper/fragment/settings/runs";

            public static string ApplySettings { get; } = "/run-refactor-helper/fragment/settings/apply";

            public static string SaveSettingsToDisk { get; } = "/run-refactor-helper/fragment/settings/save-to-disk";

            public static string AddNewRun { get; } = "/run-refactor-helper/fragment/settings/add";

            public static string CopyRun { get; } = "/run-refactor-helper/fragment/settings/copy";

            public static string FormGet { get; } = "/run-refactor-helper/fragment/settings/forms";

            public static string FormPut { get; } = "/run-refactor-helper/fragment/forms/save";

            public static string FormDeleteRow { get; } = "/run-refactor-helper/fragment/forms/remove-row";

            public static string SideBarRequests { get; } = "/run-refactor-helper/fragment/request-list";

            public static string SideBarSettings { get; } = "/run-refactor-helper/fragment/sidebar/settings";

            public static string SideBarSettingsRemoveRun { get; } = "/run-refactor-helper/fragment/sidebar/settings/remove";

            public static string Sidebar { get; } = "/run-refactor-helper/fragment/sidebar";
        }

        public static class Download
        {
            public static string Settings { get; } = "/run-refactor-helper/download/refactorHelperSettings.json";
        }
    }
}
