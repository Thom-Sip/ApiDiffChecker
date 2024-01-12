namespace ApiDiffChecker.Models.Settings
{
    public class Run
    {
        public List<Parameter> UrlParameters { get; set; } = [];

        public List<Parameter> QueryParameters { get; set; } = [];

        public List<Parameter> PropertiesToReplace { get; set; } = [];
    }
}
