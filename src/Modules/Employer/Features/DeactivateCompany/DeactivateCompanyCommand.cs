using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.DeactivateCompany;

public sealed record DeactivateCompanyCommand(Guid CompanyId) : IRequest<Result>;
