using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicCompanyProfile;

public sealed record GetPublicCompanyProfileQuery(Guid CompanyId) : IRequest<Result<GetPublicCompanyProfileResponse>>;
