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
            { "customerid", "1" }
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
            Match match = Regex.Match(template, @"\{(.+?)\}");

            if(match.Success)
            {
                if (Variables.ContainsKey(GetParam(match.Value)))
                    template = template.Replace(match.Value, Variables[GetParam(match.Value)]);
            }

            return template;
        }

        private string GetParam(string urlPart)
        {
            return urlPart.Replace("{", "").Replace("}", "");
        }
    }
}
