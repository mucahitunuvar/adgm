using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetCompany;

public sealed record GetCompanyQuery(Guid CompanyId) : IRequest<Result<GetCompanyResponse>>;
