using Newtonsoft.Json;
using RefactorHelper.Models;
using RefactorHelper.Models.SwaggerProcessor;
using Swashbuckle.Swagger;
using Parameter = RefactorHelper.Models.Parameter;

namespace RefactorHelper.SwaggerProcessor
{
    public class SwaggerProcessorService
    {
        private RefactorHelperSettings Settings { get; }

        public SwaggerProcessorService(RefactorHelperSettings settings)
        {
            Settings = settings;
        }

        public SwaggerProcessorResult ProcessSwagger(string swaggerJson)
        {
            var doc = JsonConvert.DeserializeObject<SwaggerDocument>(swaggerJson);

            var result = new List<RequestDetails>();

            foreach (var run in Settings.Runs)
            {
                result.AddRange(doc.paths.Select(p => GetRequestDetails(p, run)).ToList());
            }

            return new SwaggerProcessorResult
            {
                Requests = result.DistinctBy(x => x.Path).ToList()
            };
        }

        private RequestDetails GetRequestDetails(KeyValuePair<string, PathItem> path, List<Parameter> parameters)
        {
            if(path.Value.get != null)
            {
                return new RequestDetails 
                {
                    Template = path.Key.ToLower(),
                    Path = GetPath(path.Key.ToLower(), path.Value.get, parameters),
                    Operation = path.Value.get,
                };
            }

            throw new NotImplementedException();
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
                        template = template.Replace($"{{{param.name.ToLower()}}}", result);
                    }
                    else if (TryGetValue(param.name, out var result2, Settings.DefaultParameters))
                    {
                        template = template.Replace($"{{{param.name.ToLower()}}}", result2);
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
    }
}
