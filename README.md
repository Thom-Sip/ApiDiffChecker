## API Diff Checker

[![Build Status](https://github.com/Thom-Sip/ApiDiffChecker/actions/workflows/build.yml/badge.svg)](https://github.com/Thom-Sip/ApiDiffChecker/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/ApiDiffChecker)](https://www.nuget.org/packages/ApiDiffChecker)

A simple API Testing tool to compare the differences in responses between 2 different origins.

* Easily check how your local environment compares to your test server
* Verify that any refactoring work did not introduce any accidental changes

## Setup

1. Usings

```csharp
using ApiDiffChecker;
```

2. Setup dependency injection

```csharp

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddApiDiffChecker();
} 
```

3. Initialize the App

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // ApiDiffChecker Endpoints
    app.AddApiDiffCheckerEndpoints();
}
```
4. Start your WebApi and browse to **/api-diff-checker**

5. Open the Settings Page and setup your default parameters and Runs

6. Click: Save Settings to disk

7. Click: Run All


## Powered by

* [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
* [HTMX](https://htmx.org/)
* [diff-match-patch](https://github.com/google/diff-match-patch)
