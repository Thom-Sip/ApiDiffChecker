﻿using ApiDiffChecker.Features.SwaggerProcessor;
using ApiDiffChecker.Features.UIGenerator;

namespace ApiDiffChecker.Models.State
{
    public class ApiDiffCheckerState
    {
        public bool Initialized { get; set; }

        public string SwaggerJson { get; set; } = string.Empty;

        public int CurrentRequest { get; set; }

        public int? CurrentRun { get; set; }

        public HtmlTemplate BaseHtmlTemplate { get; set; } = new();

        public SwaggerProcessorOutput SwaggerOutput { get; set; } = new();

        public List<RequestWrapper> Data { get; set; } = [];

        public RequestWrapper GetCurrentRequest() => Data[CurrentRequest];
    }
}
