using RefactorHelper.Models.External;

namespace RefactorHelper.Models.Comparer
{
    public class CompareResult
    {
        public string Result { get; set; }

        public List<Diff> Diffs { get; init; }
    }
}
