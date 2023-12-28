namespace RefactorHelper.Models
{
    public static class Url
    {
        public static class Page
        {
            public static string Root { get; } = "/run-refactor-helper";

            public static string TestResult { get; } = "/run-refactor-helper";

            public static string Settings { get; } = "/run-refactor-helper/settings";

            public static string Reset { get; } = "/run-refactor-helper/reset";
        }

        public static class Fragment
        {
            public static string RunAll { get; } = "/run-refactor-helper/fragment/run-all";

            public static string TestResult { get; } = "/run-refactor-helper/fragment";

            public static string RetryCurrentRequest { get; } = "/run-refactor-helper/fragment/retry";

            public static string Settings { get; } = "/run-refactor-helper/fragment/settings";

            public static string RunSettings { get; } = "/run-refactor-helper/fragment/settings/runs";

            public static string FormGet { get; } = "/run-refactor-helper/fragment/settings/forms";

            public static string FormPut { get; } = "/run-refactor-helper/fragment/forms/save";

            public static string SideBarRequests { get; } = "/run-refactor-helper/fragment/request-list";

            public static string SideBarSettings { get; } = "/run-refactor-helper/fragment/sidebar/settings";

            public static string SideBarSettingsAddRun { get; } = "/run-refactor-helper/fragment/sidebar/settings/add";
        }

        public static class Download
        {
            public static string Settings { get; } = "/run-refactor-helper/download/settings";
        }
    }
}
