using RefactorHelper.Models;
using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.External;
using RefactorHelper.Models.RequestHandler;

namespace RefactorHelper.Comparer
{
    public class CompareService
    {
        protected diff_match_patch _dmp { get; }

        public CompareService()
        {
            _dmp = new diff_match_patch();
        }

        public ComparerOutput CompareResponses(RequestHandlerOutput responseData)
        {
            var result = new ComparerOutput();

            foreach(var response in responseData.Results)
            {
                // Get diffs
                var diffs1 = _dmp.diff_main(response.Response1, response.Response2);
                var diffs2 = _dmp.diff_main(response.Response2, response.Response1);

                // Only show relevant differences
                _dmp.diff_cleanupSemantic(diffs1);
                _dmp.diff_cleanupSemantic(diffs2);

                result.Results.Add(new CompareResultPair
                {
                    Changed = response.Response1 != response.Response2,
                    Path = response.Path,
                    FilePath = $"{MakePathSafe(response.Path)}.html",
                    Result1 = new CompareResult
                    {
                        Diffs = diffs1,
                        Result = response.Response1
                    },
                    Result2 = new CompareResult
                    {
                        Diffs = diffs2,
                        Result = response.Response2
                    }
                });
            }

            return result;
        }

        private string MakePathSafe(string url)
        {
            return url.Replace(" ", "").Replace("&", "_").Replace("?", "_").Replace("/", "_");
        }
    }
}
