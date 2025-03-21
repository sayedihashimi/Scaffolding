// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Scaffolding.CodeModification;
using Microsoft.DotNet.Scaffolding.Core.Builder;
using Microsoft.DotNet.Scaffolding.Core.ComponentModel;
using Microsoft.DotNet.Scaffolding.Core.Hosting;
using Microsoft.DotNet.Scaffolding.Core.Steps;
using Microsoft.DotNet.Scaffolding.Internal.Services;
using Microsoft.DotNet.Scaffolding.TextTemplating;
using Microsoft.DotNet.Tools.Scaffold.AspNet.Helpers;
using Microsoft.DotNet.Tools.Scaffold.AspNet.ScaffoldSteps;
using Microsoft.Extensions.DependencyInjection;
using Constants = Microsoft.DotNet.Tools.Scaffold.AspNet.Common.Constants;

namespace Microsoft.DotNet.Tools.Scaffold.AspNet;
public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateScaffoldBuilder();
        ConfigureServices(builder.Services);
        ConfigureSteps(builder.Services);
        CreateOptions(
            out var projectOption, out var prereleaseOption, out var fileNameOption, out var actionsOption,
            out var areaNameOption, out var modelNameOption, out var endpointsClassOption, out var databaseProviderOption,
            out var databaseProviderRequiredOption, out var identityDbProviderRequiredOption, out var dataContextClassOption, out var dataContextClassRequiredOption,
            out var openApiOption, out var pageTypeOption, out var controllerNameOption, out var viewsOption, out var overwriteOption,
            out var authOption, out var aadb2cInstanceOption, out var susiPolicyOption, out var signedOutCallbackOption, out var resetPasswordPolicyIdOption,
            out var editProfilePolicyIdOption, out var domainOption, out var callbackPathOption, out var useLocalDbOption);

        builder.AddScaffolder("sayedha-test")
            .WithDisplayName("Entra Auth")
            .WithCategory("Blazor")
            .WithDescription("Adds Entra Auth to the selected application")
            .WithOption(projectOption)
            .WithOption(authOption)
            // mock assumes individualb2c was selected
            .WithOption(aadb2cInstanceOption)
            .WithOption(susiPolicyOption)
            .WithOption(resetPasswordPolicyIdOption)
            .WithOption(editProfilePolicyIdOption)
            .WithOption(domainOption)
            .WithOption(callbackPathOption)
            .WithOption(useLocalDbOption)
            .WithStep<MockStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                var authSelected = context.GetOptionResult(authOption);
                var aadInstance = context.GetOptionResult(aadb2cInstanceOption);
                context.Properties.Add("authSelected", authSelected);
                System.Console.WriteLine($"auth selected: {context.GetOptionResult(authOption)}");
                System.Console.WriteLine($"aadb2cinstance: {context.GetOptionResult(aadb2cInstanceOption)}");
                System.Console.WriteLine($"susiPolicy: {context.GetOptionResult(susiPolicyOption)}");
                System.Console.WriteLine($"resetPasswordPolicyId: {context.GetOptionResult(resetPasswordPolicyIdOption)}");
                System.Console.WriteLine($"domain: {context.GetOptionResult(domainOption)}");
                System.Console.WriteLine($"callbackPath: {context.GetOptionResult(callbackPathOption)}");
                System.Console.WriteLine($"useLocalDb: {context.GetOptionResult(useLocalDbOption)}");
            });

        builder.AddScaffolder("blazor-empty")
            .WithDisplayName("Razor Component")
            .WithCategory("Blazor")
            .WithDescription("Add an empty razor component to a given project")
            .WithOption(projectOption)
            .WithOption(fileNameOption)
            .WithStep<DotnetNewScaffolderStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.ProjectPath = context.GetOptionResult(projectOption);
                step.FileName = context.GetOptionResult(fileNameOption);
                step.CommandName = Constants.DotnetCommands.RazorComponentCommandName;
            });

        builder.AddScaffolder("razorview-empty")
            .WithDisplayName("Razor View - Empty")
            .WithCategory("MVC")
            .WithDescription("Add an empty razor view to a given project")
            .WithOption(projectOption)
            .WithOption(fileNameOption)
            .WithStep<DotnetNewScaffolderStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.ProjectPath = context.GetOptionResult(projectOption);
                step.FileName = context.GetOptionResult(fileNameOption);
                step.CommandName = Constants.DotnetCommands.ViewCommandName;
            });

        builder.AddScaffolder("razorpage-empty")
            .WithDisplayName("Razor Page - Empty")
            .WithCategory("Razor Pages")
            .WithDescription("Add an empty razor page to a given project")
            .WithOption(projectOption)
            .WithOption(fileNameOption)
            .WithStep<DotnetNewScaffolderStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.ProjectPath = context.GetOptionResult(projectOption);
                step.NamespaceName = Path.GetFileNameWithoutExtension(step.ProjectPath);
                step.FileName = context.GetOptionResult(fileNameOption);
                step.CommandName = Constants.DotnetCommands.RazorPageCommandName;
            });

        builder.AddScaffolder("apicontroller")
            .WithDisplayName("API Controller")
            .WithCategory("API")
            .WithDescription("Add an empty API Controller to a given project")
            .WithOptions([projectOption, fileNameOption, actionsOption])
            .WithStep<EmptyControllerScaffolderStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.ProjectPath = context.GetOptionResult(projectOption);
                step.FileName = context.GetOptionResult(fileNameOption);
                step.Actions = context.GetOptionResult(actionsOption);
                step.CommandName = "apicontroller";
            });

        builder.AddScaffolder("mvccontroller")
            .WithDisplayName("MVC Controller")
            .WithCategory("MVC")
            .WithDescription("Add an empty MVC Controller to a given project")
            .WithOptions([projectOption, fileNameOption, actionsOption])
            .WithStep<EmptyControllerScaffolderStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.ProjectPath = context.GetOptionResult(projectOption);
                step.FileName = context.GetOptionResult(fileNameOption);
                step.Actions = context.GetOptionResult(actionsOption);
                step.CommandName = "mvccontroller";
            });

        builder.AddScaffolder("apicontroller-crud")
            .WithDisplayName("API Controller with actions, using Entity Framework (CRUD)")
            .WithCategory("API")
            .WithDescription("Create an API controller with REST actions to create, read, update, delete, and list entities")
            .WithOptions([projectOption, modelNameOption, controllerNameOption, dataContextClassRequiredOption, databaseProviderRequiredOption, prereleaseOption])
            .WithStep<ValidateEfControllerStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.Project = context.GetOptionResult(projectOption);
                step.Model = context.GetOptionResult(modelNameOption);
                step.DataContext = context.GetOptionResult(dataContextClassRequiredOption);
                step.DatabaseProvider = context.GetOptionResult(databaseProviderRequiredOption);
                step.Prerelease = context.GetOptionResult(prereleaseOption);
                step.ControllerType = "API";
                step.ControllerName = context.GetOptionResult(controllerNameOption);
            })
            .WithEfControllerAddPackagesStep()
            .WithDbContextStep()
            .WithConnectionStringStep()
            .WithEfControllerTextTemplatingStep()
            .WithEfControllerCodeChangeStep();

        builder.AddScaffolder("mvccontroller-crud")
            .WithDisplayName("MVC Controller with views, using Entity Framework (CRUD)")
            .WithCategory("MVC")
            .WithDescription("Create a MVC controller with read/write actions and views using Entity Framework")
            .WithOptions([projectOption, modelNameOption, controllerNameOption, viewsOption, dataContextClassRequiredOption, databaseProviderRequiredOption, prereleaseOption])
            .WithStep<ValidateEfControllerStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.Project = context.GetOptionResult(projectOption);
                step.Model = context.GetOptionResult(modelNameOption);
                step.DataContext = context.GetOptionResult(dataContextClassRequiredOption);
                step.DatabaseProvider = context.GetOptionResult(databaseProviderRequiredOption);
                step.Prerelease = context.GetOptionResult(prereleaseOption);
                step.ControllerType = "MVC";
                step.ControllerName = context.GetOptionResult(controllerNameOption);
            })
            .WithEfControllerAddPackagesStep()
            .WithDbContextStep()
            .WithConnectionStringStep()
            .WithEfControllerTextTemplatingStep()
            .WithEfControllerCodeChangeStep()
            .WithMvcViewsStep();

        builder.AddScaffolder("blazor-crud")
            .WithDisplayName("Razor Components with EntityFrameworkCore (CRUD)")
            .WithCategory("Blazor")
            .WithDescription("Generates Razor Components using Entity Framework for Create, Delete, Details, Edit and List operations for the given model")
            .WithOptions([projectOption, modelNameOption, dataContextClassRequiredOption, databaseProviderRequiredOption, pageTypeOption, prereleaseOption])
            .WithStep<ValidateBlazorCrudStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.Project = context.GetOptionResult(projectOption);
                step.Model = context.GetOptionResult(modelNameOption);
                step.DataContext = context.GetOptionResult(dataContextClassRequiredOption);
                step.DatabaseProvider = context.GetOptionResult(databaseProviderRequiredOption);
                step.Prerelease = context.GetOptionResult(prereleaseOption);
                step.Page = context.GetOptionResult(pageTypeOption);
            })
            .WithBlazorCrudAddPackagesStep()
            .WithDbContextStep()
            .WithConnectionStringStep()
            .WithBlazorCrudTextTemplatingStep()
            .WithBlazorCrudCodeChangeStep();

        builder.AddScaffolder("razorpages-crud")
            .WithDisplayName("Razor Pages with Entity Framework (CRUD)")
            .WithCategory("Razor Pages")
            .WithDescription("Generates Razor pages using Entity Framework for Create, Delete, Details, Edit and List operations for the given model")
            .WithOptions([projectOption, modelNameOption, dataContextClassRequiredOption, databaseProviderRequiredOption, pageTypeOption, prereleaseOption])
            .WithStep<ValidateRazorPagesStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.Project = context.GetOptionResult(projectOption);
                step.Model = context.GetOptionResult(modelNameOption);
                step.DataContext = context.GetOptionResult(dataContextClassRequiredOption);
                step.DatabaseProvider = context.GetOptionResult(databaseProviderRequiredOption);
                step.Prerelease = context.GetOptionResult(prereleaseOption);
                step.Page = context.GetOptionResult(pageTypeOption);
            })
            .WithRazorPagesAddPackagesStep()
            .WithDbContextStep()
            .WithConnectionStringStep()
            .WithRazorPagesTextTemplatingStep()
            .WithRazorPagesCodeChangeStep();

        builder.AddScaffolder("views")
            .WithDisplayName("Razor Views")
            .WithCategory("MVC")
            .WithDescription("Generates Razor views for Create, Delete, Details, Edit and List operations for the given model")
            .WithOptions([projectOption, modelNameOption, pageTypeOption])
            .WithStep<ValidateViewsStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.Project = context.GetOptionResult(projectOption);
                step.Model = context.GetOptionResult(modelNameOption);
                step.Page = context.GetOptionResult(pageTypeOption);
            })
            .WithViewsTextTemplatingStep()
            .WithViewsAddFileStep();

        builder.AddScaffolder("minimalapi")
            .WithDisplayName("Minimal API")
            .WithCategory("API")
            .WithDescription("Generates an endpoints file (with CRUD API endpoints) given a model and optional DbContext.")
            .WithOptions([projectOption, modelNameOption, endpointsClassOption, openApiOption, dataContextClassOption, databaseProviderOption, prereleaseOption])
            .WithStep<ValidateMinimalApiStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.Project = context.GetOptionResult(projectOption);
                step.Model = context.GetOptionResult(modelNameOption);
                step.DataContext = context.GetOptionResult(dataContextClassOption);
                step.DatabaseProvider = context.GetOptionResult(databaseProviderOption);
                step.Prerelease = context.GetOptionResult(prereleaseOption);
                step.OpenApi = context.GetOptionResult(openApiOption);
                step.Endpoints = context.GetOptionResult(endpointsClassOption);
            })
            .WithMinimalApiAddPackagesStep()
            .WithDbContextStep()
            .WithConnectionStringStep()
            .WithMinimalApiTextTemplatingStep()
            .WithMinimalApiCodeChangeStep();

        builder.AddScaffolder("area")
            .WithDisplayName("Area")
            .WithCategory("MVC")
            .WithDescription("Creates a MVC Area folder structure.")
            .WithOptions([projectOption, areaNameOption])
            .WithStep<AreaScaffolderStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.Project = context.GetOptionResult(projectOption);
                step.Name = context.GetOptionResult(areaNameOption);
            });

        builder.AddScaffolder("blazor-identity")
            .WithDisplayName("Blazor Identity")
            .WithCategory("Blazor")
            .WithCategory("Identity")
            .WithDescription("Add blazor identity to a project.")
            .WithOptions([projectOption, dataContextClassRequiredOption, identityDbProviderRequiredOption, overwriteOption, prereleaseOption])
            .WithStep<ValidateIdentityStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.Project = context.GetOptionResult(projectOption);
                step.DataContext = context.GetOptionResult(dataContextClassRequiredOption);
                step.DatabaseProvider = context.GetOptionResult(identityDbProviderRequiredOption);
                step.Prerelease = context.GetOptionResult(prereleaseOption);
                step.Overwrite = context.GetOptionResult(overwriteOption);
                step.BlazorScenario = true;
            })
            .WithBlazorIdentityAddPackagesStep()
            .WithIdentityDbContextStep()
            .WithConnectionStringStep()
            .WithBlazorIdentityTextTemplatingStep()
            .WithBlazorIdentityCodeChangeStep();

        builder.AddScaffolder("identity")
            .WithDisplayName("ASP.NET Core Identity")
            .WithCategory("Identity")
            .WithDescription("Add ASP.NET Core identity to a project.")
            .WithOptions([projectOption, dataContextClassRequiredOption, identityDbProviderRequiredOption, overwriteOption, prereleaseOption])
            .WithStep<ValidateIdentityStep>(config =>
            {
                var step = config.Step;
                var context = config.Context;
                step.Project = context.GetOptionResult(projectOption);
                step.DataContext = context.GetOptionResult(dataContextClassRequiredOption);
                step.DatabaseProvider = context.GetOptionResult(identityDbProviderRequiredOption);
                step.Prerelease = context.GetOptionResult(prereleaseOption);
                step.Overwrite = context.GetOptionResult(overwriteOption);
            })
            .WithIdentityAddPackagesStep()
            .WithIdentityDbContextStep()
            .WithConnectionStringStep()
            .WithIdentityTextTemplatingStep()
            .WithIdentityCodeChangeStep();

        var runner = builder.Build();
        var telemetryWrapper = builder.ServiceProvider?.GetRequiredService<IFirstPartyToolTelemetryWrapper>();
        telemetryWrapper?.ConfigureFirstTimeTelemetry();
        runner.RunAsync(args).Wait();
        telemetryWrapper?.Flush();
    }

    static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEnvironmentService, EnvironmentService>();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddTelemetry("dotnetScaffoldAspnet");
        services.AddSingleton<IFirstPartyToolTelemetryWrapper, FirstPartyToolTelemetryWrapper>();
    }

    static void ConfigureSteps(IServiceCollection services)
    {
        var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        var scaffoldStepTypes = executingAssembly.GetTypes().Where(x => x.IsSubclassOf(typeof(ScaffoldStep)));
        foreach (var type in scaffoldStepTypes)
        {
            services.AddTransient(type);
        }

        //ScaffoldSteps from other assemblies/projects
        services.AddTransient<CodeModificationStep>();
        services.AddTransient<AddPackagesStep>();
        services.AddTransient<AddConnectionStringStep>();
        services.AddTransient<TextTemplatingStep>();
    }

    static void CreateOptions(
        out ScaffolderOption<string> projectOption,
        out ScaffolderOption<bool> prereleaseOption,
        out ScaffolderOption<string> fileNameOption,
        out ScaffolderOption<bool> actionsOption,
        out ScaffolderOption<string> areaNameOption,
        out ScaffolderOption<string> modelNameOption,
        out ScaffolderOption<string> endpointsClassOption,
        out ScaffolderOption<string> databaseProviderOption,
        out ScaffolderOption<string> databaseProviderRequiredOption,
        out ScaffolderOption<string> identityDbProviderRequiredOption,
        out ScaffolderOption<string> dataContextClassOption,
        out ScaffolderOption<string> dataContextClassRequiredOption,
        out ScaffolderOption<bool> openApiOption,
        out ScaffolderOption<string> pageTypeOption,
        out ScaffolderOption<string> controllerNameOption,
        out ScaffolderOption<bool> viewsOption,
        out ScaffolderOption<bool> overwriteOption,
        out ScaffolderOption<string>authOption,
        out ScaffolderOption<string> aadb2cInstanceOption,
        out ScaffolderOption<string> susiPolicyOption,
        out ScaffolderOption<string> signedOutCallbackOption,
        out ScaffolderOption<string> resetPasswordPolicyId,
        out ScaffolderOption<string> editProfilePolicyIdOption,
        out ScaffolderOption<string>domainOption,
        out ScaffolderOption<string> callbackPathOption,
        out ScaffolderOption<bool>useLocalDbOption)
    {
        projectOption = new ScaffolderOption<string>
        {
            DisplayName = ".NET project file",
            CliOption = Constants.CliOptions.ProjectCliOption,
            Description = ".NET project to be used for scaffolding (.csproj file)",
            Required = true,
            PickerType = InteractivePickerType.ProjectPicker
        };

        prereleaseOption = new ScaffolderOption<bool>
        {
            DisplayName = "Include Prerelease packages?",
            CliOption = Constants.CliOptions.PrereleaseCliOption,
            Description = "Include prerelease package versions when installing latest Aspire components",
            Required = false,
            PickerType = InteractivePickerType.YesNo
        };

        fileNameOption = new ScaffolderOption<string>
        {
            DisplayName = "File name",
            CliOption = Constants.CliOptions.NameOption,
            Description = "File name for new file being created with 'dotnet new'",
            Required = true,
        };

        actionsOption = new ScaffolderOption<bool>
        {
            DisplayName = "Read/Write Actions?",
            CliOption = Constants.CliOptions.ActionsOption,
            Description = "Create controller with read/write actions?",
            Required = true,
            PickerType = InteractivePickerType.YesNo
        };

        controllerNameOption = new ScaffolderOption<string>
        {
            DisplayName = "Controller Name",
            CliOption = Constants.CliOptions.ControllerNameOption,
            Description = "Name for the controller being created",
            Required = true
        };

        areaNameOption = new ScaffolderOption<string>
        {
            DisplayName = "Area Name",
            CliOption = Constants.CliOptions.NameOption,
            Description = "Name for the area being created",
            Required = true
        };

        modelNameOption = new ScaffolderOption<string>
        {
            DisplayName = "Model Name",
            CliOption = Constants.CliOptions.ModelCliOption,
            Description = "Name for the model class to be used for scaffolding",
            Required = true,
            PickerType = InteractivePickerType.ClassPicker
        };

        endpointsClassOption = new ScaffolderOption<string>
        {
            DisplayName = "Endpoints File Name",
            CliOption = Constants.CliOptions.EndpointsOption,
            Description = "",
            Required = true
        };

        dataContextClassOption = new ScaffolderOption<string>
        {
            DisplayName = "Data Context Class",
            CliOption = Constants.CliOptions.DataContextOption,
            Description = "",
            Required = false
        };

        dataContextClassRequiredOption = new ScaffolderOption<string>
        {
            DisplayName = "Data Context Class",
            CliOption = Constants.CliOptions.DataContextOption,
            Description = "",
            Required = true
        };

        openApiOption = new ScaffolderOption<bool>
        {
            DisplayName = "Open API Enabled",
            CliOption = Constants.CliOptions.OpenApiOption,
            Description = "",
            Required = false,
            PickerType = InteractivePickerType.YesNo
        };

        databaseProviderOption = new ScaffolderOption<string>
        {
            DisplayName = "Database Provider",
            CliOption = Constants.CliOptions.DbProviderOption,
            Description = "",
            Required = false,
            PickerType = InteractivePickerType.CustomPicker,
            CustomPickerValues = AspNetDbContextHelper.DbContextTypeDefaults.Keys.ToArray()
        };

        databaseProviderRequiredOption = new ScaffolderOption<string>
        {
            DisplayName = "Database Provider",
            CliOption = Constants.CliOptions.DbProviderOption,
            Description = "",
            Required = true,
            PickerType = InteractivePickerType.CustomPicker,
            CustomPickerValues = AspNetDbContextHelper.DbContextTypeDefaults.Keys.ToArray()
        };

        identityDbProviderRequiredOption = new ScaffolderOption<string>
        {
            DisplayName = "Database Provider",
            CliOption = Constants.CliOptions.DbProviderOption,
            Description = "",
            Required = true,
            PickerType = InteractivePickerType.CustomPicker,
            CustomPickerValues = AspNetDbContextHelper.IdentityDbContextTypeDefaults.Keys.ToArray()
        };

        pageTypeOption = new ScaffolderOption<string>
        {
            DisplayName = "Page Type",
            CliOption = Constants.CliOptions.PageTypeOption,
            Description = "The CRUD page(s) to scaffold",
            Required = true,
            PickerType = InteractivePickerType.CustomPicker,
            CustomPickerValues = BlazorCrudHelper.CRUDPages
        };

        viewsOption = new ScaffolderOption<bool>
        {
            DisplayName = "With Views?",
            CliOption = Constants.CliOptions.ViewsOption,
            Description = "Add CRUD razor views (.cshtml)",
            Required = true,
            PickerType = InteractivePickerType.YesNo
        };

        overwriteOption = new ScaffolderOption<bool>
        {
            DisplayName = "Overwrite existing files?",
            CliOption = Constants.CliOptions.OverwriteOption,
            Description = "Option to enable overwriting existing files",
            Required = true,
            PickerType = InteractivePickerType.YesNo
        };

        authOption = new ScaffolderOption<string> {
            DisplayName = "Auth Type",
            CliOption = Constants.CliOptions.AuthOption,
            Description = "Entra auth type",
            Required = true,
            PickerType = InteractivePickerType.CustomPicker,
            CustomPickerValues = new string[] { "IndividualB2C", "SingleOrg", "MultiOrg" }
        };

        aadb2cInstanceOption = new ScaffolderOption<string> {
            DisplayName = "AAdB2CInstance",
            CliOption = Constants.CliOptions.AadB2CInstanceOption,
            Description = "The Azure Active Directory B2C instance to connect to (use with IndividualB2C auth).",
        };

        susiPolicyOption = new ScaffolderOption<string> {
            DisplayName = "SignUpSignInPolicyId",
            CliOption = Constants.CliOptions.SusiOptionPolicyIdOption,
            Description = "The sign-in and sign-up policy ID for this project (use with IndividualB2C auth).",
        };

        signedOutCallbackOption = new ScaffolderOption<string> {
            DisplayName = "SignedOutCallbackPath",
            CliOption = Constants.CliOptions.SignedOutCallbackOption,
            Description = "The global signout callback (use with IndividualB2C auth).",
        };

        resetPasswordPolicyId = new ScaffolderOption<string> {
            DisplayName = "ResetPasswordPolicyId",
            CliOption = "--resetPasswordPolicyId",
            Description = "The reset password policy ID for this project (use with IndividualB2C auth).",
        };

        editProfilePolicyIdOption = new ScaffolderOption<string> {
            DisplayName = "EditProfilePolicyId",
            CliOption = "--editProfilePolicyId",
            Description = "The edit profile policy ID for this project (use with IndividualB2C auth).",
        };

        domainOption = new ScaffolderOption<string> {
            DisplayName = "Domain",
            CliOption = "--domain",
            Description = "The domain for the directory tenant (use with SingleOrg or IndividualB2C auth).",
        };

        callbackPathOption= new ScaffolderOption<string> {
            DisplayName = "CallbackPath",
            CliOption = "--callbackPath",
            Description = "The request path within the application's base path of the redirect URI (use with SingleOrg or IndividualB2C auth).",
        };

        useLocalDbOption = new ScaffolderOption<bool> {
            DisplayName = "Use LocalDb instead of SQLite?",
            CliOption = "--uselocaldb",
            Description = "Whether to use LocalDB instead of SQLite. This option only applies if --auth Individual or --auth IndividualB2C is specified.",
            PickerType = InteractivePickerType.YesNo
        };

        //signedOutCallbackOption = new ScaffolderOption<string> {
        //    DisplayName = "",
        //    CliOption = "",
        //    Description = "",
        //    Required = true
        //};
    }
}

