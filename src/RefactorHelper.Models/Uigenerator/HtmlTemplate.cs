namespace RefactorHelper.Models.Uigenerator
{
    public class HtmlTemplate
    {
        public string Html { get; set; } = string.Empty;

        public string SetContent(string content)
        {
            return Html.Replace("[CONTENT_BLOCK]", content);
        }
    }
}
