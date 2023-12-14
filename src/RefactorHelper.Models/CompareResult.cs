using DiffMatchPatch;

namespace RefactorHelper.Models
{
    public class CompareResult
    {
        public bool Changed { get; init; }

        public string Result1 { get; set; }

        public string Result2 { get; set; }

        public List<Diff> Diffs1 { get; init; }

        public List<Diff> Diffs2 { get; init; }

        public string Path { get; init; }
    }
}
