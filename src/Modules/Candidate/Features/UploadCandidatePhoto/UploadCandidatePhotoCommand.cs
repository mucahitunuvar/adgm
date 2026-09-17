using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.UploadCandidatePhoto;

public sealed record UploadCandidatePhotoCommand(Guid CandidateCvId, Stream Content, string FileName, string ContentType)
    : IRequest<Result<string>>;
