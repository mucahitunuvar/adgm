using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissionById;

public sealed record GetFormSubmissionByIdQuery(Guid Id) : IRequest<Result<FormSubmissionDetailResponse>>;
