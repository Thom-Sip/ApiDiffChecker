using Swashbuckle.Swagger;

namespace RefactorHelper.Models
{
    public class RequestDetails
    {
        public required string Template { get; init; }

        public required string Path { get; init; }

        public required Operation Operation { get; init; }
    }
}
