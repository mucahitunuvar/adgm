using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPopupById;

public sealed record GetPopupByIdQuery(Guid Id) : IRequest<Result<PopupDetailResponse>>;
