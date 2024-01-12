using Swashbuckle.Swagger;

namespace ApiDiffChecker.Models.SwaggerProcessor
{
    public class RequestDetails
    {
        public int Id { get; set; }

        public required string Template { get; init; }

        public required string Path { get; init; }

        public required Operation Operation { get; init; }
    }
}
