using RefactorHelper.Models.Config;

namespace RefactorHelper.UIGenerator
{
    public class Formbuilder(RefactorHelperSettings settings)
    {
        protected string _formTemplate { get; set; } = File.ReadAllText($"{settings.ContentFolder}/Forms/FormTemplate.html");

        protected string _formTemplateEdit { get; set; } = File.ReadAllText($"{settings.ContentFolder}/Forms/FormTemplateEdit.html");

        protected string _formFieldTemplate { get; set; } = File.ReadAllText($"{settings.ContentFolder}/Forms/FormFieldTemplate.html");

        protected string _formFieldTemplateEdit { get; set; } = File.ReadAllText($"{settings.ContentFolder}/Forms/FormFieldTemplateEdit.html");

        protected RefactorHelperSettings Settings { get; set; } = settings;

        public string GetForm(List<Parameter> parameters, List<Parameter> savedParams, string putUrl, string getUrl, bool allowEdit)
        {
            var text = (allowEdit ? _formTemplateEdit : _formTemplate)
                .Replace("[PUT_URL]", putUrl)
                .Replace("[GET_URL]", getUrl)
                .Replace("[FORM_FIELDS]", string.Join(Environment.NewLine, savedParams.Select(x => GetFormField(x, parameters, allowEdit))))
                .Replace("[DISABLED]", allowEdit ? "" : "disabled");

            return text;
        }

        private string GetFormField(Parameter parameter, List<Parameter> parameters, bool allowEdit)
        {
            var existingSettings = parameters.FirstOrDefault(x => x.Key == parameter.Key);

            return (allowEdit ? _formFieldTemplateEdit : _formFieldTemplate)
                .Replace("[KEY]", parameter.Key)
                .Replace("[VALUE]", existingSettings?.Value ?? string.Empty);
        }
    }
}
