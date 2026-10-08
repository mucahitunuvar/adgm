using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItemRevisionByNumber;

public sealed record GetContentItemRevisionByNumberQuery(Guid ContentItemId, int RevisionNumber)
    : IRequest<Result<ContentItemRevisionDetailResponse>>;
