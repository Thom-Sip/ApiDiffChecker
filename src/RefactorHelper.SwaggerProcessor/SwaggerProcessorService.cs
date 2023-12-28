using Newtonsoft.Json;
using RefactorHelper.Models;
using RefactorHelper.Models.Config;
using RefactorHelper.Models.SwaggerProcessor;
using Swashbuckle.Swagger;
using Parameter = RefactorHelper.Models.Config.Parameter;

namespace RefactorHelper.SwaggerProcessor
{
    public class SwaggerProcessorService(RefactorHelperSettings settings)
    {
        private RefactorHelperSettings Settings { get; } = settings;

        public static SwaggerProcessorOutput GetQueryParamsFromSwagger(string swaggerJson)
        {
            var doc = JsonConvert.DeserializeObject<SwaggerDocument>(swaggerJson);

            return new()
            {
                UrlParameters = GetParams(doc, "path"),
                QueryParameters = GetParams(doc, "query"),
                Requests = GetGetRequests(doc)
            };
        }

        private static List<Parameter> GetParams(SwaggerDocument doc, string paramType)
        {
            var getRequestsWithParams = doc.paths
                .Where(x => x.Value.get != null && x.Value.get.parameters != null)
                .Select(x => x.Value.get).ToList();

            var urlParams = getRequestsWithParams
                .SelectMany(x => x.parameters.Where(y => y.@in == paramType))
                .Distinct()
                .ToList();

            return urlParams.Select(x => new Parameter(x.name, $"{{{x.name}}}")).ToList();
        }

        private static List<RequestDetails> GetGetRequests(SwaggerDocument doc)
        {
            var getRequests = doc.paths
               .Where(x => x.Value.get != null).ToList();

            return getRequests.Select(x => new RequestDetails
            {
                Operation = x.Value.get,
                Template = x.Key,
                Path = x.Key
            }).ToList();
        }

        public List<RequestWrapper> ProcessSwagger(string swaggerJson)
        {
            var doc = JsonConvert.DeserializeObject<SwaggerDocument>(swaggerJson);

            var request = new List<RequestDetails>();

            foreach (var run in Settings.Runs)
                request.AddRange(doc.paths.Where(x => x.Value.get != null).Select(p => GetRequestDetails(p, run)).ToList());

            request = request.OrderBy(x => x.Path).DistinctBy(x => x.Path).ToList();

            var result = new List<RequestWrapper>();
            for (int i = 0; i < request.Count; i++)
            {
                request[i].Id = i;
                result.Add(new RequestWrapper
                {
                    Id = i,
                    Request = request[i],
                });
            }

            return result;
        }

        private RequestDetails GetRequestDetails(KeyValuePair<string, PathItem> path, Run run)
        {
            return new RequestDetails 
            {
                Template = path.Key.ToLower(),
                Operation = path.Value.get,
                Path = GetPath(path.Key.ToLower(), path.Value.get, run)
            };
        }

        private string GetPath(string template, Operation operation, Run run)
        {
            var queryParams = new List<string>();

            foreach(var param in operation?.parameters ?? new List<Swashbuckle.Swagger.Parameter>())
            {
                if(param.@in == "path")
                {
                    if(TryGetValue(param.name, out var result, run.UrlParameters))
                    {
                        template = ReplaceUrlParam(template, param, result);
                    }
                    else if (TryGetValue(param.name, out var result2, Settings.DefaultRunSettings.UrlParameters))
                    {
                        template = ReplaceUrlParam(template, param, result2);
                    }

                }
                if(param.@in == "query")
                {
                    var value = param.name;

                    if (TryGetValue(param.name, out var result, run.QueryParameters))
                    {
                        value = result;
                    }
                    else if (TryGetValue(param.name, out var result2, Settings.DefaultRunSettings.QueryParameters))
                    {
                        value = result2;
                    }

                    queryParams.Add($"{param.name}={value}");
                }
            }

            if(queryParams.Any())
                template = $"{template}?{string.Join('&', queryParams)}";

            return template;
        }

        private static bool TryGetValue(string key, out string? value, List<Parameter> Params)
        {
            value = Params.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase))?.Value;
            return value != null;
        }

        private static string ReplaceUrlParam(string template, Swashbuckle.Swagger.Parameter param, string? result)
        {
            return template.Replace($"{{{param.name.ToLower()}}}", result);
        }
    }
}
