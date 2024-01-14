## API Diff Checker

[![Build Status](https://github.com/Thom-Sip/ApiDiffChecker/actions/workflows/build.yml/badge.svg)](https://github.com/Thom-Sip/ApiDiffChecker/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/ApiDiffChecker)](https://www.nuget.org/packages/ApiDiffChecker)

A simple API Testing tool to compare the differences in responses between 2 different origins.

* Easily check how your local environment compares to your test server
* Verify that any refactoring work did not introduce any accidental changes

## Setup

1. Edit Program.cs

```csharp
using ApiDiffChecker;
```

```csharp

if (builder.Environment.IsDevelopment())
{
    // Setup API Diff Checker Dependency Injection
    builder.Services.AddApiDiffChecker();
} 
```

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Initialize API Diff Checker
    app.ApiDiffCheckerInitialize();
}
```
2. Start your WebApi and browse to **/api-diff-checker**

3. Open the Settings Page and setup your default parameters and Runs

4. Click: Save Settings to disk

5. Click: Run All

## Powered by

* [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
* [HTMX](https://htmx.org/)
* [diff-match-patch](https://github.com/google/diff-match-patch)
