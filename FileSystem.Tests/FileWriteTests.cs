﻿using System;
using System.Collections.Generic;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class FileWriteTests : StepTestBase<FileWrite, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Write file",
                    new FileWrite
                    {
                        Path = Constant("Filename.txt"), Stream = Constant("Hello World")
                    },
                    Unit.Default
                ).WithFileSystem()
                .WithExpectedFileSystem(
                    expectedFinalFiles: new[] { ("/Filename.txt", "Hello World") }
                );

            yield return new StepCase(
                    "Write file compressed",
                    new Sequence<Unit>()
                    {
                        InitialSteps = new List<IStep<Unit>>()
                        {
                            new FileWrite
                            {
                                Path     = Constant("Filename.txt"),
                                Stream   = Constant("Hello World"),
                                Compress = Constant(true)
                            },
                            new Log<StringStream>()
                            {
                                Value = new FileRead()
                                {
                                    Path       = Constant("Filename.txt"),
                                    Decompress = Constant(true)
                                }
                            },
                            new DeleteItem() //Must delete the file so it's not in the final state
                            {
                                Path = Constant("Filename.txt"),
                            }
                        },
                        FinalStep = new DoNothing()
                    },
                    Unit.Default,
                    "Hello World"
                )
                .WithFileSystem()
                .WithExpectedFileSystem();
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "IFileSystem Error",
                new FileWrite { Stream = Constant("Data"), Path = Constant("Destination") },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "File.Create Error",
                new FileWrite { Stream = Constant("Data"), Path = Constant("Destination") },
                ErrorCode.ExternalProcessError.ToErrorBuilder("Ultimate Test Exception")
            ).WithFileSystemMock(
                x => x.Setup(fs => fs.File.Create("Destination"))
                    .Throws(new Exception("Ultimate Test Exception"))
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

}