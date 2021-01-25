# digital-archive-api

The server-side API (a.k.a backend) which supplies data to The HistoryMakers Digital Video Archive web application. 
It is written in C# using [.NET 5.0](https://dotnet.microsoft.com/download) and compiled with Visual Studio 2019
using the standard .NET Core Web API template.  The API most closely resembles a stateless view controller using JSON as the serialization protocol. 


## Installation

The following will work under normal circumstances:

1. Clone repository to local machine
2. Open solution file with Visual Studio 2019
3. Run debugger - Visual Studio will automatically install dependency packages prior to compiling the application.

## Build / Runtime Environments

The appropriate `appsettings` transformation file is selected based on the value of the value of the `ASPNETCORE_ENVIRONMENT` environment variable. For further information see: [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1). The `appsettings.json` must contain the endpoint URIs and API keys for your fully configured Azure backing resources.

## Publishing to Azure

> **IMPORTANT: As of December 2020 the publishing procedure has changed.**
>
> The API **MUST** include a compiled copy of the Angular client prior to publishing to Azure. Refer to the client [README.md](https://github.com/oral-history-video-archive/digital-archive-www/) for further details on how to compile the client for a specific Azure target.

For the following example, we will assume that `/digital-archive-api` and `/digital-archive-www` are the file system root paths of the API and Angular Client repositories respectively.

1. Delete the `/dist` folder under `digital-archive-api/DigitalArchiveAPI/Angular`
1. Compile the Angular client for a desired Azure target per the client build instructions. 
1. Copy or move the compiled Angular client distribution folder from `/digital-archive-www/dist` to the `digital-archive-api/DigitalArchiveAPI/Angular`
    * The resulting folder structure should look like `digital-archive-api/DigitalArchiveAPI/Angular/dist`
1. In Visual Studio 2019, right click the **DigitalArchiveAPI** project and select **Publish...**
1. Select the appropriate publishing profile for your Azure Web App
1. Click **Publish**

**If you do not currently have cached credentials you will be prompted to log in to Azure**

**WARNING: the `appsettings.json` files contain Azure secrets which should NEVER be shared with the public**