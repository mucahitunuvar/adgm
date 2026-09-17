using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.SearchCandidateCvs;

public sealed record SearchCandidateCvsQuery(string? SearchText) : PagedRequest, IRequest<Result<SearchCandidateCvsResponse>>;
