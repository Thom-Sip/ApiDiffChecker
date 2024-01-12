namespace ApiDiffChecker.Models
{
    public static class Url
    {
        public static class Page
        {
            public static string Root { get; } = "/api-diff-checker";

            public static string TestResult { get; } = "/api-diff-checker";

            public static string Settings { get; } = "/api-diff-checker/settings";

            public static string Reset { get; } = "/api-diff-checker/reset";
        }

        public static class Fragment
        {
            public static string RunAll { get; } = "/api-diff-checker/fragment/run-all";

            public static string TestResult { get; } = "/api-diff-checker/fragment";

            public static string RetryCurrentRequest { get; } = "/api-diff-checker/fragment/retry";

            public static string Settings { get; } = "/api-diff-checker/fragment/settings";

            public static string RunSettings { get; } = "/api-diff-checker/fragment/settings/runs";

            public static string ApplySettings { get; } = "/api-diff-checker/fragment/settings/apply";

            public static string SaveSettingsToDisk { get; } = "/api-diff-checker/fragment/settings/save-to-disk";

            public static string AddNewRun { get; } = "/api-diff-checker/fragment/settings/add";

            public static string CopyRun { get; } = "/api-diff-checker/fragment/settings/copy";

            public static string FormGet { get; } = "/api-diff-checker/fragment/settings/forms";

            public static string FormPut { get; } = "/api-diff-checker/fragment/forms/save";

            public static string FormDeleteRow { get; } = "/api-diff-checker/fragment/forms/remove-row";

            public static string SideBarRequests { get; } = "/api-diff-checker/fragment/request-list";

            public static string SideBarSettings { get; } = "/api-diff-checker/fragment/sidebar/settings";

            public static string SideBarSettingsRemoveRun { get; } = "/api-diff-checker/fragment/sidebar/settings/remove";

            public static string Sidebar { get; } = "/api-diff-checker/fragment/sidebar";
        }

        public static class Download
        {
            public static string Settings { get; } = "/api-diff-checker/download/refactorHelperSettings.json";
        }
    }
}
