using Newtonsoft.Json;
using RefactorHelper.Models;
using RefactorHelper.Models.Config;
using RefactorHelper.Models.SwaggerProcessor;
using Swashbuckle.Swagger;
using Parameter = RefactorHelper.Models.Config.Parameter;

namespace RefactorHelper.SwaggerProcessor
{
    public class SwaggerProcessorService
    {
        private RefactorHelperSettings Settings { get; }

        public SwaggerProcessorService(RefactorHelperSettings settings)
        {
            Settings = settings;
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

        private RequestDetails GetRequestDetails(KeyValuePair<string, PathItem> path, List<Parameter> parameters)
        {
            return new RequestDetails 
            {
                Template = path.Key.ToLower(),
                Path = GetPath(path.Key.ToLower(), path.Value.get, parameters),
                Operation = path.Value.get,
            };
        }

        private string GetPath(string template, Operation operation, List<Parameter> parameters)
        {
            var queryParams = new List<string>();

            foreach(var param in operation?.parameters ?? new List<Swashbuckle.Swagger.Parameter>())
            {
                if(param.@in == "path")
                {
                    if(TryGetValue(param.name, out var result, parameters))
                    {
                        template = ReplaceUrlParam(template, param, result);
                    }
                    else if (TryGetValue(param.name, out var result2, Settings.DefaultParameters))
                    {
                        template = ReplaceUrlParam(template, param, result2);
                    }

                }
                if(param.@in == "query")
                {
                    var value = param.name;

                    if (TryGetValue(param.name, out var result, parameters))
                    {
                        value = result;
                    }
                    else if (TryGetValue(param.name, out var result2, Settings.DefaultParameters))
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
