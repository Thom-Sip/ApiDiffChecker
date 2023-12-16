namespace RefactorHelper.Models.RequestHandler
{
    public class RefactorTestResultPair
    {
        public required string Path { get; init; }

        public required RefactorTestResult Result1 { get; init; }

        public required RefactorTestResult Result2 { get; init; }
    }
}
