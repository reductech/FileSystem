﻿using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.FileSystem
{

/// <summary>
/// Reads text from a file.
/// </summary>
[Alias("ReadFromFile")]
public sealed class FileRead : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var path = await Path.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (path.IsFailure)
            return path.ConvertFailure<StringStream>();

        var encoding = await Encoding.Run(stateMonad, cancellationToken);

        if (encoding.IsFailure)
            return encoding.ConvertFailure<StringStream>();

        var decompress = await Decompress.Run(stateMonad, cancellationToken);

        if (decompress.IsFailure)
            return decompress.ConvertFailure<StringStream>();

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this))
                .ConvertFailure<StringStream>();

        try
        {
            var fs = fileSystemResult.Value.File.OpenRead(path.Value);

            if (decompress.Value)
            {
                fs = new System.IO.Compression.GZipStream(
                    fs,
                    System.IO.Compression.CompressionMode.Decompress
                );
            }

            var stringStream = new StringStream(fs, encoding.Value);

            return stringStream;
        }
        catch (Exception e)
        {
            return new SingleError(new ErrorLocation(this), e, ErrorCode.ExternalProcessError);
        }
    }

    /// <summary>
    /// The name of the file to read.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <summary>
    /// How the file is encoded.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("UTF8 no BOM")]
    public IStep<EncodingEnum> Encoding { get; set; } =
        new EnumConstant<EncodingEnum>(EncodingEnum.UTF8);

    /// <summary>
    /// Whether to decompress this string
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("false")]
    public IStep<bool> Decompress { get; set; } = new BoolConstant(false);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<FileRead, StringStream>();
}

}