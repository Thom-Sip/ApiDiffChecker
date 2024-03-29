﻿using Microsoft.AspNetCore.Http;
using ApiDiffChecker.Models;
using ApiDiffChecker.Models.Settings;
using ApiDiffChecker.Models.State;
using ApiDiffChecker.Models.Enums;

namespace ApiDiffChecker.Features.UIGenerator
{
    public class FormGeneratorService(ApiDiffCheckerSettings settings, ApiDiffCheckerState state)
    {
        protected string _formTemplate { get; } = File.ReadAllText($"{settings.FormsContentFolder}/FormTemplate.html");

        protected string _formTemplateEdit { get; } = File.ReadAllText($"{settings.FormsContentFolder}/FormTemplateEdit.html");

        protected string _formFieldTemplate { get; } = File.ReadAllText($"{settings.FormsContentFolder}/FormFieldTemplate.html");

        protected string _formFieldTemplateEdit { get; } = File.ReadAllText($"{settings.FormsContentFolder}/FormFieldTemplateEdit.html");

        protected string _formFieldTemplateEditWithKey { get; } = File.ReadAllText($"{settings.FormsContentFolder}/FormFieldTemplateEditKeyAndValue.html");

        protected ApiDiffCheckerSettings Settings { get; } = settings;

        protected ApiDiffCheckerState State { get; } = state;

        public string GetFormFragment(FormType formType, bool allowEdit, int? runId = null)
        {
            var getUrl = $"{Url.Fragment.FormPut}/{formType}";
            var putUrl = $"{Url.Fragment.FormGet}/{formType}?allowEdit={!allowEdit}";
            var addUrl = $"{Url.Fragment.FormPut}/{formType}/add";

            if (runId != null)
            {
                getUrl = $"{Url.Fragment.FormPut}/{formType}?runId={runId}";
                putUrl = $"{Url.Fragment.FormGet}/{formType}?runId={runId}&allowEdit={!allowEdit}";
                addUrl = $"{Url.Fragment.FormPut}/{formType}/add?runId={runId}";
            }

            return GetForm(
                    GetFormData(formType, runId),
                    GetDefaultValues(formType, allowEdit, runId),
                    getUrl, putUrl, allowEdit,
                    AllowAdd: formType == FormType.Replacevalues,
                    addRowUrl: addUrl,
                    formType == FormType.Replacevalues ? _formFieldTemplateEditWithKey : _formFieldTemplateEdit,
                    removeRowUrl: $"{Url.Fragment.FormDeleteRow}/{formType}?rowId=");
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

        private List<Parameter> GetDefaultValues(FormType formType, bool allowEdit, int? runId)
        {
            return formType switch
            {
                FormType.QueryParameters => State.SwaggerOutput.QueryParameters,
                FormType.UrlParameters => State.SwaggerOutput.UrlParameters,
                FormType.Replacevalues => GetReplaceValues(runId, allowEdit),
                _ => throw new NotImplementedException()
            }; ;
        }

        private List<Parameter> GetReplaceValues(int? runId, bool allowEdit)
        {
            var result = runId == null
                ? Settings.DefaultRunSettings.PropertiesToReplace
                : Settings.Runs[runId.Value].PropertiesToReplace;

            if (result.Count == 0 && allowEdit)
                result.Add(new Parameter("", ""));

            return result;
        }

        public string GetForm(List<Parameter> parameters, List<Parameter> savedParams, string putUrl, string getUrl, bool allowEdit, bool AllowAdd, string addRowUrl, string editTemplate, string removeRowUrl)
        {
            var text = (allowEdit ? _formTemplateEdit : _formTemplate)
                .Replace("[PUT_URL]", putUrl)
                .Replace("[GET_URL]", getUrl)
                .Replace("[FORM_FIELDS]", string.Join(Environment.NewLine, savedParams.Select(x => GetFormField(x, parameters, allowEdit, editTemplate, removeRowUrl))))
                .Replace("[DISABLED]", allowEdit ? "" : "disabled")
                .Replace("[ADD_NEW_BUTTON]", AllowAdd ? GetAddRowButton(addRowUrl) : "");

            return text;
        }

        private static string GetAddRowButton(string url)
        {
            return $"<div class=\"form-button-container\"><button hx-put=\"{url}\">+ Add row</button></div><br/>";
        }

        private string GetFormField(Parameter parameter, List<Parameter> parameters, bool allowEdit, string editTemplate, string removeRowUrl)
        {
            var existingSettings = parameters.FirstOrDefault(x => x.Key == parameter.Key);

            return (allowEdit ? editTemplate : _formFieldTemplate)
                .Replace("[KEY_KEY]", $"key_{parameters.IndexOf(parameter)}")
                .Replace("[KEY_VALUE]", existingSettings?.Key ?? string.Empty)
                .Replace("[VALUE_KEY]", $"value_{parameters.IndexOf(parameter)}")
                .Replace("[VALUE_VALUE]", existingSettings?.Value ?? string.Empty)
                .Replace("[KEY]", parameter.Key)
                .Replace("[VALUE]", existingSettings?.Value ?? string.Empty)
                .Replace("[DELETE_ROW_URL]", $"{removeRowUrl}{parameters.IndexOf(parameter)}");
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
                    run.PropertiesToReplace = SetParameterSettingsWithKeys(form);
                    break;
            }
        }

        public void AddRow(FormType formType, int? runId)
        {
            var run = GetRun(runId);

            switch (formType)
            {
                case FormType.UrlParameters:
                    run.UrlParameters.Add(new Parameter("", ""));
                    break;

                case FormType.QueryParameters:
                    run.QueryParameters.Add(new Parameter("", ""));
                    break;

                case FormType.Replacevalues:
                    run.PropertiesToReplace.Add(new Parameter("", ""));
                    break;
            }
        }

        public void DeleteRow(FormType formType, int? runId, int rowId)
        {
            var run = GetRun(runId);

            switch (formType)
            {
                case FormType.UrlParameters:
                    run.UrlParameters.RemoveAt(rowId);
                    break;

                case FormType.QueryParameters:
                    run.QueryParameters.RemoveAt(rowId);
                    break;

                case FormType.Replacevalues:
                    run.PropertiesToReplace.RemoveAt(rowId);
                    break;
            }
        }

        private Run GetRun(int? runId)
        {
            if (runId == null)
                return Settings.DefaultRunSettings;

            return Settings.Runs[runId.Value];
        }

        private static List<Parameter> SetParameterSettingsWithKeys(IFormCollection form)
        {
            var result = new List<Parameter>();
            for (int i = 0; i < form.Count; i += 2)
            {
                var key = form.Skip(i).FirstOrDefault();
                var value = form.Skip(i + 1).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(key.Value.ToString()))
                    continue;

                result.Add(new Parameter(key.Value.ToString(), value.Value.ToString()));
            }

            return result;
        }

        private static List<Parameter> SetParameterSettings(IFormCollection form)
        {
            return form.Where(x => !string.IsNullOrWhiteSpace(x.Key))
                       .Select(x => new Parameter(x.Key, x.Value.ToString()))
                       .ToList();
        }
    }
}
