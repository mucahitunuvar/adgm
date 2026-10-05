using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ActivatePopup;

public sealed record ActivatePopupCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
