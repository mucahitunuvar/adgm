using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeletePartner;

public sealed record DeletePartnerCommand(Guid Id) : IRequest<Result>;
