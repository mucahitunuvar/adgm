using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.ApproveCompany;

public sealed record ApproveCompanyCommand(Guid CompanyId) : IRequest<Result>;
