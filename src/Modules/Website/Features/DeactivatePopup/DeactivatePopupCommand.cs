using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeactivatePopup;

public sealed record DeactivatePopupCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
