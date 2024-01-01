using Microsoft.AspNetCore.Http;
using RefactorHelper.Models;
using RefactorHelper.Models.Config;
using RefactorHelper.Models.Uigenerator;

namespace RefactorHelper.UIGenerator
{
    public class Formbuilder(RefactorHelperSettings settings, RefactorHelperState state)
    {
        protected string _formTemplate { get; } = File.ReadAllText($"{settings.ContentFolder}/Forms/FormTemplate.html");

        protected string _formTemplateEdit { get; } = File.ReadAllText($"{settings.ContentFolder}/Forms/FormTemplateEdit.html");

        protected string _formFieldTemplate { get; } = File.ReadAllText($"{settings.ContentFolder}/Forms/FormFieldTemplate.html");

        protected string _formFieldTemplateEdit { get; } = File.ReadAllText($"{settings.ContentFolder}/Forms/FormFieldTemplateEdit.html");

        protected RefactorHelperSettings Settings { get; } = settings;

        protected RefactorHelperState State { get; } = state;

        public string GetFormFragment(FormType formType, bool allowEdit, int? runId = null)
        {
            var getUrl = $"{Url.Fragment.FormPut}/{formType}";
            var putUrl = $"{Url.Fragment.FormGet}/{formType}?allowEdit={!allowEdit}";
            var addUrl = $"{Url.Fragment.FormPut}/{formType}/add";

            if (runId != null)
            {
                getUrl = $"{Url.Fragment.FormPut}/{formType}?runId={runId}";
                putUrl = $"{Url.Fragment.FormGet}/{formType}?runId={runId}&allowEdit={!allowEdit}";
                addUrl = $"{Url.Fragment.FormPut}/{formType}?runId={runId}&add";
            }

            return GetForm(
                    GetFormData(formType, runId),
                    GetDefaultValues(formType),
                    getUrl, putUrl, allowEdit, 
                    AllowAdd: formType == FormType.Replacevalues,
                    addRowUrl: addUrl);
        }

        private List<Parameter> GetFormData(FormType formType, int? runId)
        {
            if (runId != null)
            {
                return formType switch
                {
                    FormType.QueryParameters => Settings.Runs[runId.Value].QueryParameters,
                    FormType.UrlParameters => Settings.Runs[runId.Value].UrlParameters,
                    FormType.Replacevalues => Settings.Runs[runId.Value].PropertiesToReplace,
                    _ => throw new NotImplementedException()
                };
            }

            return formType switch
            {
                FormType.QueryParameters => Settings.DefaultRunSettings.QueryParameters,
                FormType.UrlParameters => Settings.DefaultRunSettings.UrlParameters,
                FormType.Replacevalues => Settings.DefaultRunSettings.PropertiesToReplace,
                _ => throw new NotImplementedException()
            };
        }

        private List<Parameter> GetDefaultValues(FormType formType)
        {
            return formType switch
            {
                FormType.QueryParameters => State.SwaggerOutput.QueryParameters,
                FormType.UrlParameters => State.SwaggerOutput.UrlParameters,
                FormType.Replacevalues => Settings.DefaultRunSettings.PropertiesToReplace,
                _ => throw new NotImplementedException()
            }; ;
        }

        public string GetForm(List<Parameter> parameters, List<Parameter> savedParams, string putUrl, string getUrl, bool allowEdit, bool AllowAdd, string addRowUrl)
        {
            var text = (allowEdit ? _formTemplateEdit : _formTemplate)
                .Replace("[PUT_URL]", putUrl)
                .Replace("[GET_URL]", getUrl)
                .Replace("[FORM_FIELDS]", string.Join(Environment.NewLine, savedParams.Select(x => GetFormField(x, parameters, allowEdit))))
                .Replace("[DISABLED]", allowEdit ? "" : "disabled")
                .Replace("[ADD_NEW_BUTTON]", AllowAdd ? GetAddRowButton(addRowUrl) : "");

            return text;
        }

        private static string GetAddRowButton(string url)
        {
            return $"<button hx-put=\"{url}\">New</button>";
        }

        private string GetFormField(Parameter parameter, List<Parameter> parameters, bool allowEdit)
        {
            var existingSettings = parameters.FirstOrDefault(x => x.Key == parameter.Key);

            return (allowEdit ? _formFieldTemplateEdit : _formFieldTemplate)
                .Replace("[KEY]", parameter.Key)
                .Replace("[VALUE]", existingSettings?.Value ?? string.Empty);
        }

        public void SaveForm(FormType formType, IFormCollection form, int? runId)
        {
            var run = GetRun(runId);

            switch (formType)
            {
                case FormType.UrlParameters:
                    run.UrlParameters = SetParameterSettings(form);
                    break;

                case FormType.QueryParameters:
                    run.QueryParameters = SetParameterSettings(form);
                    break;

                case FormType.Replacevalues:
                    run.PropertiesToReplace = SetParameterSettings(form);
                    break;
            }
        }

        public void AddRow(FormType formType, IFormCollection form, int? runId)
        {
            var run = GetRun(runId);

            switch (formType)
            {
                case FormType.Replacevalues:
                    run.PropertiesToReplace.Add(new Parameter("", ""));
                    break;

                case FormType.UrlParameters:
                    run.UrlParameters.Add(new Parameter("", ""));
                    break;

                case FormType.QueryParameters:
                    run.QueryParameters.Add(new Parameter("", ""));
                    break;
            }
        }

        private Run GetRun(int? runId)
        {
            if (runId == null)
                return Settings.DefaultRunSettings;

            return Settings.Runs[runId.Value];
        }

        private static List<Parameter> SetParameterSettings(IFormCollection form)
        {
            return form.Where(x => !string.IsNullOrWhiteSpace(x.Value))
                       .Select(x => new Parameter(x.Key, x.Value.ToString()))
                       .ToList();
        }
    }
}
