namespace RefactorHelper.Models
{
    public class RequestDetails
    {
        public required string Path { get; init; }

        public List<string> UrlVariables { get; init; } = new();
    }
}
