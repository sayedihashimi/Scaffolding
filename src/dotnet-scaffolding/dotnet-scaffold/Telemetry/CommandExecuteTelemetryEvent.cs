// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.DotNet.Scaffolding.Core.ComponentModel;
using Microsoft.DotNet.Scaffolding.Internal.Extensions;
using Microsoft.DotNet.Scaffolding.Internal.Telemetry;

namespace Microsoft.DotNet.Tools.Scaffold.Telemetry;

internal class CommandExecuteTelemetryEvent : TelemetryEventBase
{
    private const string TelemetryEventName = "CommandExecute";
    public CommandExecuteTelemetryEvent(DotNetToolInfo dotnetToolInfo, CommandInfo commandInfo, int? exitCode, string? chosenCategory = null) : base(TelemetryEventName)
    {
        SetProperty("PackageName", dotnetToolInfo.PackageName, isPII: true);
        SetProperty("Version", dotnetToolInfo.Version);
        SetProperty("ToolName", dotnetToolInfo.Command, isPII: true);
        SetProperty("ToolLevel", dotnetToolInfo.IsGlobalTool ? TelemetryConstants.GlobalTool : TelemetryConstants.LocalTool);
        SetProperty("CommandName", commandInfo.Name, isPII: true);
        SetProperty("AllCommandCategories", commandInfo.DisplayCategories, isPII: true);
        if (!string.IsNullOrEmpty(chosenCategory))
        {
            SetProperty("ChosenCategory", chosenCategory, isPII: true);
        }

        SetProperty("Result", exitCode is null ? TelemetryConstants.Unknown : exitCode == 0 ? TelemetryConstants.Success : TelemetryConstants.Failure);
    }
}
