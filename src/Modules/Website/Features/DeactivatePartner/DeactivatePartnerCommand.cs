using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeactivatePartner;

public sealed record DeactivatePartnerCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
