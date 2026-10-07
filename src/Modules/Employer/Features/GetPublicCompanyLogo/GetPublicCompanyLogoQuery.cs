using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicCompanyLogo;

public sealed record GetPublicCompanyLogoQuery(Guid CompanyId) : IRequest<Result<GetPublicCompanyLogoResult>>;
