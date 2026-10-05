using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeletePopup;

public sealed record DeletePopupCommand(Guid Id) : IRequest<Result>;
