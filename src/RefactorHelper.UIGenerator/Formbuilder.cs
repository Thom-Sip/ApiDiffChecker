using RefactorHelper.Models.Config;

namespace RefactorHelper.UIGenerator
{
    public class Formbuilder
    {
        protected string _formTemplate { get; set; }

        protected string _formFieldTemplate { get; set; }

        protected RefactorHelperSettings Settings { get; set; }

        public Formbuilder(string contentFolder, RefactorHelperSettings settings)
        {
            Settings = settings;
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
            var existingSettings = Settings.DefaultParameters.FirstOrDefault(x => x.Key == paramater.Key);

            return _formFieldTemplate
                .Replace("[KEY]", paramater.Key)
                .Replace("[VALUE]", existingSettings?.Value ?? paramater.Value);
        }
    }
}
