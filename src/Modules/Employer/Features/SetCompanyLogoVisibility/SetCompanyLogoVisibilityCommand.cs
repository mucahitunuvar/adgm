using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.SetCompanyLogoVisibility;

public sealed record SetCompanyLogoVisibilityCommand(bool ShowLogoOnWebsite, byte[] RowVersion) : IRequest<Result>;
