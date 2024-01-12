namespace ApiDiffChecker.RequestHandler
{
    public class RequestHandlerResult
    {
        public required HttpResponseMessage ResponseObject { get; init; }

        public required string Response { get; init; }
    }
}
