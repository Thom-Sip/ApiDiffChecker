using Swashbuckle.Swagger;

namespace RefactorHelper.Models.SwaggerProcessor
{
    public class RequestDetails
    {
        public int Id { get; set; }

        public required string Template { get; init; }

        public required string Path { get; init; }

        public required Operation Operation { get; init; }
    }
}
