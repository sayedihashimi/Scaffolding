// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Core.Scaffolders;
using Microsoft.DotNet.Scaffolding.Core.Steps;

namespace Microsoft.DotNet.Tools.Scaffold.AspNet.ScaffoldSteps {
    internal class MockStep : ScaffoldStep {
        public override Task<bool> ExecuteAsync(ScaffolderContext context, CancellationToken cancellationToken = default) {
            // no op
            return Task.FromResult(true);
        }
    }
}
