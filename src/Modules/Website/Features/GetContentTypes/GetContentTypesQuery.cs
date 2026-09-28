using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentTypes;

public sealed record GetContentTypesQuery : IRequest<Result<IReadOnlyList<ContentTypeSummaryResponse>>>;
