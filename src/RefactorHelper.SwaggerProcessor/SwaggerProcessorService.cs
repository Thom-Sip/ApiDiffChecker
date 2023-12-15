using Newtonsoft.Json;
using RefactorHelper.Models;
using Swashbuckle.Swagger;
using System.Text.RegularExpressions;

namespace RefactorHelper.SwaggerProcessor
{
    public class SwaggerProcessorService
    {
        private Dictionary<string, string> Variables = new Dictionary<string, string>
        {
            { "customerid", "1" },
            { "orderid", "AGSY001" },
            { "message", "Hello World" }
        };

        public List<RequestDetails> ProcessSwagger(string swaggerJson)
        {
            var doc = JsonConvert.DeserializeObject<SwaggerDocument>(swaggerJson);
            return doc.paths.Select(p => GetRequestDetails(p)).ToList();
        }

        private RequestDetails GetRequestDetails(KeyValuePair<string, PathItem> path)
        {
            if(path.Value.get != null)
            {
                return new RequestDetails 
                { 
                    Template = path.Key.ToLower(),
                    Path = GetPath(path.Key.ToLower(), path.Value.get),
                    Operation = path.Value.get,
                };
            }

            throw new NotImplementedException();
        }

        private string GetPath(string template, Operation operation)
        {
            var queryParams = new List<string>();

            foreach(var param in operation?.parameters ?? new List<Parameter>())
            {
                if(param.@in == "path")
                {
                    if(TryGetValue(param.name, out var result))
                        template = template.Replace($"{{{param.name.ToLower()}}}", result);
                }
                if(param.@in == "query")
                {
                    var value = param.name;

                    if (TryGetValue(param.name, out var result))
                        value = result;

                    queryParams.Add($"{param.name}={value}");
                }
            }

            if(queryParams.Any())
                template = $"{template}?{string.Join('&', queryParams)}";

            return template;
        }

        private bool TryGetValue(string key, out string? value)
        {
            if(Variables.ContainsKey(key.ToLower()))
            {
                value = Variables[key.ToLower()];
                return true;
            }

            value = null;
            return false;
        }
    }
}
