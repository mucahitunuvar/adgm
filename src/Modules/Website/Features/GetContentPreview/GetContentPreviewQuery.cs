using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentPreview;

public sealed record GetContentPreviewQuery(string Token) : IRequest<Result<ContentPreviewResponse>>;
