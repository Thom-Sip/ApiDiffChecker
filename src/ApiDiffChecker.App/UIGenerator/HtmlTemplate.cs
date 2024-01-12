namespace ApiDiffChecker.UIGenerator
{
    public class HtmlTemplate
    {
        public string Html { get; set; } = string.Empty;

        public HtmlTemplate SetContent(string content)
        {
            return new HtmlTemplate
            {
                Html = Html.Replace("[CONTENT_BLOCK]", content)
            };
        }

        public HtmlTemplate SetSideBar(string content)
        {
            return new HtmlTemplate
            {
                Html = Html.Replace("[SIDEBAR_CONTENT]", content)
            };
        }
    }
}
