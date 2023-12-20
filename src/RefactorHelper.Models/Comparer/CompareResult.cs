using RefactorHelper.Models.External;

namespace RefactorHelper.Models.Comparer
{
    public class CompareResult
    {
        public required string Result { get; set; }

        public required List<Diff> Diffs { get; init; }

        public required HttpResponseMessage Response { get; init; }
    }
}
