using DiffMatchPatch;
using RefactorHelper.Models;

namespace RefactorHelper.Comparer
{
    public class CompareService
    {
        protected diff_match_patch _dmp { get; }

        public CompareService()
        {
            _dmp = new diff_match_patch();
        }

        public List<CompareResult> CompareResponses(List<RefactorTestResponse> responseData)
        {
            var result = new List<CompareResult>();

            foreach(var response in responseData)
            {
                // Get diffs
                var diffs1 = _dmp.diff_main(response.Response1, response.Response2);
                var diffs2 = _dmp.diff_main(response.Response2, response.Response1);

                // Only show relevant differences
                _dmp.diff_cleanupSemantic(diffs1);
                _dmp.diff_cleanupSemantic(diffs2);

                result.Add(new CompareResult
                {
                    Result1 = response.Response1,
                    Result2 = response.Response2,
                    Changed = diffs1.Count == 0,
                    Diffs1 = diffs1,
                    Diffs2 = diffs2,
                    Path = response.Path
                });
            }

            return result;
        }
    }
}
