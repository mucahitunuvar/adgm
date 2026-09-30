using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DuplicateContentItem;

public sealed record DuplicateContentItemCommand(Guid Id) : IRequest<Result<DuplicateContentItemResponse>>;
