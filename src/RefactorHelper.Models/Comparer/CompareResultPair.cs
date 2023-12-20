namespace RefactorHelper.Models.Comparer
{
    public class CompareResultPair
    {
        public required string Path { get; init; }

        public required string FilePath { get; init; }

        public required CompareResult Result1 { get; init; }

        public required CompareResult Result2 { get; init; }
    }
}
