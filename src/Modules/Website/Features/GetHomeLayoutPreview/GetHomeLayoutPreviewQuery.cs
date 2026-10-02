using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetHomeLayoutPreview;

public sealed record GetHomeLayoutPreviewQuery(string? Lang) : IRequest<Result<GetHomeLayoutPreviewResponse>>;
