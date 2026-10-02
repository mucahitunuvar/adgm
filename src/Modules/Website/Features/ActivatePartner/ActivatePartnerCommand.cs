using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ActivatePartner;

public sealed record ActivatePartnerCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
