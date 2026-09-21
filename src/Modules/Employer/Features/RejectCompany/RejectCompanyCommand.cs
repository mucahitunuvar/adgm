using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.RejectCompany;

public sealed record RejectCompanyCommand(Guid CompanyId, string Reason) : IRequest<Result>;
