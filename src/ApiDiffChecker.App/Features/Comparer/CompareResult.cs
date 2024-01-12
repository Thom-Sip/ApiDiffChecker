namespace ApiDiffChecker.Features.Comparer
{
    public class CompareResult
    {
        public required string Result { get; set; }

        public required HttpResponseMessage Response { get; init; }
    }
}
