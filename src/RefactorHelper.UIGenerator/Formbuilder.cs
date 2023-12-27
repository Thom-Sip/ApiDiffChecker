using RefactorHelper.Models.Config;

namespace RefactorHelper.UIGenerator
{
    public class Formbuilder
    {
        protected string _formTemplate { get; set; }

        protected string _formFieldTemplate { get; set; }

        public Formbuilder(string contentFolder)
        {
            _formTemplate = File.ReadAllText($"{contentFolder}/Forms/FormTemplate.html");
            _formFieldTemplate = File.ReadAllText($"{contentFolder}/Forms/FormFieldTemplate.html");
        }

        public string GetForm(List<Parameter> parameters, string putUrl)
        {
            var text = _formTemplate
                .Replace("[PUT_URL]", putUrl)
                .Replace("[GET_URL]", $"")
                .Replace("[FORM_FIELDS]", string.Join(Environment.NewLine, parameters.Select(GetFormField)));

            return text;
        }

        private string GetFormField(Parameter paramater)
        {
            return _formFieldTemplate
                .Replace("[KEY]", paramater.Key)
                .Replace("[VALUE]", paramater.Value);
        }
    }
}
