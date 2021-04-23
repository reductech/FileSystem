﻿using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class DirectoryCreateTests : StepTestBase<DirectoryCreate, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Create Directory",
                    new DirectoryCreate { Path = Constant("MyPath") },
                    Unit.Default
                ).WithFileSystem()
                .WithExpectedFileSystem(expectedFinalDirectories: new List<string>() { "MyPath" });
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                    "Create Directory",
                    "CreateDirectory Path: 'MyPath'",
                    Unit.Default
                )
                .WithFileSystem()
                .WithExpectedFileSystem(expectedFinalDirectories: new List<string>() { "MyPath" });
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Error returned",
                new DirectoryCreate { Path = Constant("MyPath") },
                new ErrorBuilder(
                    new Exception("Ultimate Test Exception"),
                    ErrorCode.ExternalProcessError
                )
            ).WithFileSystemMock(
                x => x.Setup(fs => fs.Directory.CreateDirectory("MyPath"))
                    .Throws(new Exception("Ultimate Test Exception"))
            );
        }
    }
}

}
