﻿using ApiDiffChecker.Features.Comparer;
using ApiDiffChecker.Features.RequestHandler;
using ApiDiffChecker.Features.SwaggerProcessor;
using ApiDiffChecker.Models.Enums;

namespace ApiDiffChecker.Models.State
{
    public class RequestWrapper
    {
        public required int Id { get; set; }

        public required RequestState State { get; set; }

        public bool Changed { get; set; }

        public bool Executed => TestResult != null;

        public required RequestDetails Request { get; set; }

        public RequestHandlerResultPair? TestResult { get; set; }

        public CompareResultPair? CompareResultPair { get; set; }

        public void Clear()
        {
            TestResult = null;
            CompareResultPair = null;
            State = RequestState.Pending;
        }
    }
}
