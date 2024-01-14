## RefactorHelper

A simple API Testing tool to compare the differences in response between 2 different origins.

* Easily check how your local environment compares to your test server
* Verify that any refactoring work did not introduce any accidental changes

## Status

[Build Status](https://github.com/Thom-Sip/ApiDiffChecker/actions/workflows/merge-main.yml/badge.svg)
[Nuget](https://img.shields.io/nuget/v/ApiDiffChecker)

## Setup

Setup dependency injection

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddApiDiffChecker(
        ApiDiffCheckerSettings.GetSettingsFromJson(
        jsonPath: $"{Environment.CurrentDirectory}/apiDiffChecker.json",
        baseUrl1: "https://localhost:44371",
        baseUrl2: "https://www.my-test-server.com"));
} 
```

Add endpoints used by ApiDiffChecker

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // ApiDiffChecker Endpoints
    app.AddApiDiffCheckerEndpoints();
}
```

Add a new profile to launchSettings.json

```json
{
    "ApiDiffChecker": {
        "commandName": "IISExpress",
        "launchBrowser": true,
        "launchUrl": "api-diff-checker",
        "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development"
        }
    }
}
```

## Powered by

* [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
* [HTMX](https://htmx.org/)
* [diff-match-patch](https://github.com/google/diff-match-patch)