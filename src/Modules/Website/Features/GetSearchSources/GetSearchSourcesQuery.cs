using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSearchSources;

public sealed record GetSearchSourcesQuery : IRequest<Result<IReadOnlyList<SearchSourceSummaryResponse>>>;
