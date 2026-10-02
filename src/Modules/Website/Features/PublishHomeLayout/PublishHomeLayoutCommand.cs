using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.PublishHomeLayout;

public sealed record PublishHomeLayoutCommand(byte[] RowVersion) : IRequest<Result>;
