using GenclikMerkezi.Modules.Employer.Features.GetCompany;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetMyCompany;

public sealed record GetMyCompanyQuery : IRequest<Result<GetCompanyResponse>>;
