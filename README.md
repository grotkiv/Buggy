# Buggy

Track Azure work items with a notify icon in the notification area.

Makes it easy to access work items you are currently working on.

## Authentication

Currently the only supported Azure authentication method is by Personal Access Token.

Create a PAT with access to work items in Azure and set it using the following command:

``` console
dotnet user-secrets set "AzureProject:Pat" "your_pat_here" --project src/Buggy
```

This authentication type is meant for development purposes only, see: [Safe storage of app secrets in development in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows)

## Todo

- Separate Azure code to classlib
- Configurable WIQL Query
- Azure Authentication
