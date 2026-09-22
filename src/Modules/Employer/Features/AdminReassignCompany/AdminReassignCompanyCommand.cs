using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.AdminReassignCompany;

public sealed record AdminReassignCompanyCommand(Guid CompanyId, Guid? NewCareerAdvisorId) : IRequest<Result>;
