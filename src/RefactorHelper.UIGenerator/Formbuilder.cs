using RefactorHelper.Models.Config;

namespace RefactorHelper.UIGenerator
{
    public class Formbuilder
    {
        protected string _formTemplate { get; set; }

        protected string _formTemplateEdit { get; set; }

        protected string _formFieldTemplate { get; set; }

        protected string _formFieldTemplateEdit { get; set; }

        protected RefactorHelperSettings Settings { get; set; }

        public Formbuilder(string contentFolder, RefactorHelperSettings settings)
        {
            Settings = settings;
            _formTemplate = File.ReadAllText($"{contentFolder}/Forms/FormTemplate.html");
            _formTemplateEdit = File.ReadAllText($"{contentFolder}/Forms/FormTemplateEdit.html");
            _formFieldTemplate = File.ReadAllText($"{contentFolder}/Forms/FormFieldTemplate.html");
            _formFieldTemplateEdit = File.ReadAllText($"{contentFolder}/Forms/FormFieldTemplateEdit.html");
        }

        public string GetForm(List<Parameter> parameters, string putUrl, string getUrl, bool allowEdit)
        {
            var text = (allowEdit ? _formTemplateEdit : _formTemplate)
                .Replace("[PUT_URL]", putUrl)
                .Replace("[GET_URL]", getUrl)
                .Replace("[FORM_FIELDS]", string.Join(Environment.NewLine, parameters.Select(x => GetFormField(x, allowEdit))))
                .Replace("[DISABLED]", allowEdit ? "" : "disabled");

            return text;
        }

        private string GetFormField(Parameter paramater, bool allowEdit)
        {
            var existingSettings = Settings.DefaultParameters.FirstOrDefault(x => x.Key == paramater.Key);

            return (allowEdit ? _formFieldTemplateEdit : _formFieldTemplate)
                .Replace("[KEY]", paramater.Key)
                .Replace("[VALUE]", existingSettings?.Value ?? paramater.Value);
        }
    }
}
