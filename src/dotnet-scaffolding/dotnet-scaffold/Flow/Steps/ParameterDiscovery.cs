// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Scaffolding.ComponentModel;
using Microsoft.DotNet.Scaffolding.Helpers.Extensions;
using Microsoft.DotNet.Scaffolding.Helpers.General;
using Microsoft.DotNet.Scaffolding.Helpers.Services;
using Microsoft.DotNet.Scaffolding.Helpers.Services.Environment;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Steps
{
    internal class ParameterDiscovery
    {
        private readonly Parameter _parameter;
        private readonly IFileSystem _fileSystem;
        private readonly IEnvironmentService _environmentService;
        public ParameterDiscovery(
            Parameter parameter,
            IFileSystem fileSystem,
            IEnvironmentService environmentService)
        {
            _parameter = parameter;
            _fileSystem = fileSystem;
            _environmentService = environmentService;
        }
        public FlowStepState State { get; private set; }

        public async Task<string> DiscoverAsync(IFlowContext context)
        {
            var optionParameterAddition = _parameter.Required ? "(" : "(empty to skip, ";
            return await PromptAsync(context, $"Enter a new value for '{_parameter.DisplayName}' {optionParameterAddition}[sandybrown]<[/] to go back) : ");
        }

        private async Task<string> PromptAsync(IFlowContext context, string title)
        {
            //check if Parameter has a InteractivePickerType
            if (_parameter.PickerType is null)
            {
                var prompt = new TextPrompt<string>($"[lightseagreen]{title}[/]")
                .ValidationErrorMessage("bad value fix it please")
                .Validate(x =>
                {
                    if (x.Trim() == FlowNavigation.BackInputToken)
                    {
                        return ValidationResult.Success();
                    }

                    return Validate(context, x);
                })
                .AllowEmpty();

                await Task.Delay(1);
                return AnsiConsole.Prompt(prompt).Trim();
            }
            else
            {
                return await PromptInteractivePicker(context, _parameter.PickerType) ?? string.Empty;
            }
        }

        private async Task<string?> PromptInteractivePicker(IFlowContext context, InteractivePickerType? pickerType)
        {
            List<StepOption> stepOptions = [];
            var codeService = context.GetCodeService();
            var converter = GetDisplayName;
            switch (pickerType)
            {
                case InteractivePickerType.ClassPicker:
                    //ICodeService might be null if no InteractivePickerType.ProjectPicker was passed.
                    //will add better documentation so users will know what to expect.
                    if (codeService is null)
                    {
                        stepOptions = [];
                    }
                    else
                    {
                        stepOptions = await GetClassDisplayNamesAsync(codeService);
                    }

                    break;
                case InteractivePickerType.FilePicker:
                    //ICodeService might be null if no InteractivePickerType.ProjectPicker was passed.
                    //will add better documentation so users will know what to expect.
                    if (codeService is null)
                    {
                        stepOptions = [];
                    }
                    else
                    {
                        var allDocuments = (await codeService.GetAllDocumentsAsync()).ToList();
                        stepOptions = GetDocumentNames(allDocuments);
                    }

                    break;
                case InteractivePickerType.DbProviderPicker:
                    stepOptions = DbProviders;
                    break;
                case InteractivePickerType.ProjectPicker:
                    stepOptions = GetProjectFiles();
                    converter = GetDisplayNameForProjects;
                    break;
                case InteractivePickerType.YesNo:
                    stepOptions = [new() { Name = "Yes", Value = "true" }, new() { Name = "No", Value = "false" }];
                    converter = GetDisplayNameForYesNo;
                    break;
                case InteractivePickerType.CustomPicker:
                    stepOptions = GetCustomValues(_parameter.CustomPickerValues);
                    break;
            }

            if (!_parameter.Required)
            {
                stepOptions.Insert(0, new StepOption() { Name = "None", Value = string.Empty });
            }

            var prompt = new FlowSelectionPrompt<StepOption>()
                .Title($"[lightseagreen]{_parameter.DisplayName}: [/]")
                .Converter(converter)
                .AddChoices(stepOptions, navigation: context.Navigation);

            var result = prompt.Show();
            State = result.State;
            return result.Value?.Value;
        }

        private static List<StepOption> GetCustomValues(List<string>? customPickerValues)
        {
            if (customPickerValues is null || customPickerValues.Count == 0)
            {
                throw new InvalidOperationException("Missing 'Parameter.CustomPickerValues' values!.\nNeeded when using 'Parameter.InteractivePicker.CustomPicker'");
            }

            return customPickerValues.Select(x => new StepOption() { Name = x, Value = x }).ToList();
        }

        private static string GetDisplayName(StepOption stepOption)
        {
            bool displayNone = stepOption.Name.Equals("None", StringComparison.OrdinalIgnoreCase);
            return displayNone ? $"[sandybrown]{stepOption.Name} (empty to skip parameter)[/]" : $"{stepOption.Name} {stepOption.Value.ToSuggestion(withBrackets: true)}";
        }

        private string GetDisplayNameForProjects(StepOption stepOption)
        {
            var pathDisplay = stepOption.Value.MakeRelativePath(_environmentService.CurrentDirectory) ?? stepOption.Value;
            return $"{stepOption.Name} {pathDisplay.ToSuggestion(withBrackets: true)}";
        }

        private string GetDisplayNameForYesNo(StepOption stepOption) => stepOption.Name;

        private ValidationResult Validate(IFlowContext context, string promptVal)
        {
            if (!_parameter.Required && string.IsNullOrEmpty(promptVal))
            {
                return ValidationResult.Success();
            }

            if (!string.IsNullOrEmpty(promptVal) && !ParameterHelpers.CheckType(_parameter.Type, promptVal))
            {
                return ValidationResult.Error("Invalid input, please try again!");
            }

            return ValidationResult.Success();
        }

        private static async Task<List<StepOption>> GetClassDisplayNamesAsync(ICodeService codeService)
        {
            var allClassSymbols = await AnsiConsole
                .Status()
                .WithSpinner()
                .Start("Gathering project classes!", async statusContext =>
                {
                    //ICodeService might be null if no InteractivePickerType.ProjectPicker was passed.
                    //will add better documentation so users will know what to expect.
                    if (codeService is null)
                    {
                        return [];
                    }

                    return (await codeService.GetAllClassSymbolsAsync()).ToList();
                });

            List<StepOption> classNames = [];
            if (allClassSymbols != null && allClassSymbols.Count != 0)
            {
                allClassSymbols.ForEach(
                x =>
                {
                    if (x != null)
                    {
                        classNames.Add(new StepOption() { Name = x.MetadataName, Value = x.Name });
                    }
                });
            }

            return classNames;
        }

        internal static List<StepOption> GetDocumentNames(List<Document> documents)
        {
            List<StepOption> classNames = [];
            if (documents != null && documents.Count != 0)
            {
                documents.ForEach(
                x =>
                {
                    if (x != null)
                    {
                        string fileName = System.IO.Path.GetFileName(x.Name);
                        classNames.Add(new StepOption() { Name = fileName, Value = x.Name });
                    }
                });
            }

            return classNames;
        }

        internal List<StepOption> GetProjectFiles()
        {
            var workingDirectory = _environmentService.CurrentDirectory;
            if (!Path.IsPathRooted(workingDirectory))
            {
                workingDirectory = Path.GetFullPath(Path.Combine(_environmentService.CurrentDirectory, workingDirectory.Trim(Path.DirectorySeparatorChar)));
            }
            if (!_fileSystem.DirectoryExists(workingDirectory))
            {
                return [];
            }

            List<string> projects = _fileSystem.EnumerateFiles(workingDirectory, "*.csproj", SearchOption.AllDirectories).ToList();
            var slnFiles = _fileSystem.EnumerateFiles(workingDirectory, "*.sln", SearchOption.TopDirectoryOnly).ToList();
            List<string> projectsFromSlnFiles = GetProjectsFromSolutionFiles(slnFiles, workingDirectory);
            projects = AddUniqueProjects(projects, projectsFromSlnFiles);

            //should we search in all directories if nothing in top level? (yes, for now)
            if (projects.Count == 0)
            {
                projects = _fileSystem.EnumerateFiles(workingDirectory, "*.csproj", SearchOption.AllDirectories).ToList();
            }
            
            return projects.Select(x => new StepOption() { Name = GetProjectDisplayName(x), Value = x }).ToList();
        }

        internal List<string> GetProjectsFromSolutionFiles(List<string> solutionFiles, string workingDir)
        {
            List<string> projectPaths = [];
            foreach (var solutionFile in solutionFiles)
            {
                projectPaths.AddRange(GetProjectsFromSolutionFile(solutionFile, workingDir));
            }

            return projectPaths;
        }

        internal List<string> AddUniqueProjects(List<string> baseList, List<string> projectPathsToAdd)
        {
            var baseProjectNames = baseList.Select(x => Path.GetFileName(x));
            foreach(var projectPath in projectPathsToAdd)
            {
                var normalizedPath = StringUtil.NormalizePathSeparators(projectPath);
                var projectName = Path.GetFileName(normalizedPath);
                //not making a case-sensitive comparison, seems like macOS/Linux project names in the .sln file are not case sensitive. 
                if (!baseProjectNames.Contains(projectName, StringComparer.OrdinalIgnoreCase))
                {
                    baseList.Add(normalizedPath);
                }
            }

            return baseList;
        }

        internal IList<string> GetProjectsFromSolutionFile(string solutionFilePath, string workingDir)
        {
            List<string> projectPaths = new();
            if (string.IsNullOrEmpty(solutionFilePath))
            {
                return projectPaths;
            }

            string slnText = _fileSystem.ReadAllText(solutionFilePath);
            string projectPattern = @"Project\(""\{[A-F0-9\-]*\}""\)\s*=\s*""([^""]*)"",\s*""([^""]*)"",\s*""\{[A-F0-9\-]*\}""";
            var matches = Regex.Matches(slnText, projectPattern);
            foreach (Match match in matches)
            {
                string projectRelativePath = match.Groups[2].Value;
                string? solutionDirPath = Path.GetDirectoryName(solutionFilePath) ?? workingDir;
                string projectPath = Path.GetFullPath(projectRelativePath, solutionDirPath);
                projectPaths.Add(projectPath);
            }

            return projectPaths;
        }

        private static string GetProjectDisplayName(string projectPath)
        {
            return Path.GetFileNameWithoutExtension(projectPath);
        }

        private static List<StepOption>? _dbProviders;
        private static List<StepOption> DbProviders
        {
            get
            {
                _dbProviders ??=
                [
                    new() { Name = "SQL Server", Value = "sqlserver" },
                    new() { Name = "SQLite", Value = "sqlite" },
                    new() { Name = "PostgreSQL", Value = "postgres" },
                    new() { Name = "Cosmos DB", Value = "cosmos" }
                ];

                return _dbProviders;
            }
        }

        internal class StepOption
        {
            public required string Name { get; set; }
            public required string Value { get; set; }
        }
    }
}