using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.UploadCandidateCvFile;

public sealed record UploadCandidateCvFileCommand(Guid CandidateCvId, Stream Content, string FileName, string ContentType)
    : IRequest<Result<string>>;
