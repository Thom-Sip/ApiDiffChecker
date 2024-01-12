using Newtonsoft.Json;
using ApiDiffChecker.Models.Config;
using Newtonsoft.Json.Linq;
using ApiDiffChecker.Models.State;
using ApiDiffChecker.Models.Enums;

namespace ApiDiffChecker.Features.RequestHandler
{
    public class RequestHandlerService(ApiDiffCheckerSettings settings)
    {
        private ApiDiffCheckerSettings Settings { get; } = settings;

        public async Task QueryApis(ApiDiffCheckerState state)
        {
            var tasks = state.Data.Select(SetResponses).ToList();
            await Task.WhenAll(tasks);
        }

        public async Task QueryEndpoint(RequestWrapper requestWrapper) => await SetResponses(requestWrapper);

        public async Task SetResponses(RequestWrapper requestWrapper)
        {
            requestWrapper.State = RequestState.Running;

            var request1 = Settings.HttpClient1.GetAsync(requestWrapper.Request.Path);
            var request2 = Settings.HttpClient2.GetAsync(requestWrapper.Request.Path);

            await Task.WhenAll(request1, request2);

            var response1 = await request1.Result.Content.ReadAsStringAsync();
            var response2 = await request2.Result.Content.ReadAsStringAsync();

            response1 = TryFormatResponse(response1);
            response2 = TryFormatResponse(response2);

            requestWrapper.Changed = response1 != response2;
            requestWrapper.TestResult = new RequestHandlerResultPair
            {
                Result1 = GetRefactorTestResult(response1, request1.Result),
                Result2 = GetRefactorTestResult(response2, request2.Result),
            };

            requestWrapper.State = RequestState.Finished;
        }

        private static RequestHandlerResult GetRefactorTestResult(string result, HttpResponseMessage response)
        {
            return new RequestHandlerResult
            {
                Response = result,
                ResponseObject = response
            };
        }

        public string TryFormatResponse(string response)
        {
            try
            {
                var responseObj1 = JsonConvert.DeserializeObject<object>(response);

                if (Settings.DefaultRunSettings.PropertiesToReplace.Count > 0)
                {
                    if (responseObj1 is JArray arr)
                    {
                        foreach (var item in arr)
                        {
                            foreach (JProperty attributeProperty in item.Cast<JProperty>())
                            {
                                var replaceProp = Settings.DefaultRunSettings.PropertiesToReplace.FirstOrDefault(x =>
                                    x.Key.Equals(attributeProperty.Name, StringComparison.OrdinalIgnoreCase));

                                if (replaceProp != null)
                                {
                                    var attribute = item[attributeProperty.Name];
                                    attributeProperty.Value = replaceProp.Value;
                                }
                            }
                        }
                    }

                    if (responseObj1 is JObject obj)
                    {
                        foreach (JProperty attributeProperty in obj.Properties())
                        {
                            var replaceProp = Settings.DefaultRunSettings.PropertiesToReplace.FirstOrDefault(x =>
                                x.Key.Equals(attributeProperty.Name, StringComparison.OrdinalIgnoreCase));

                            if (replaceProp != null)
                            {
                                var attribute = obj[attributeProperty.Name];
                                attributeProperty.Value = replaceProp.Value;
                            }
                        }
                    }
                }

                response = JsonConvert.SerializeObject(responseObj1, Formatting.Indented);
            }
            catch (Exception ex)
            {

            }
            return response;
        }
    }
}
