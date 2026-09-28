using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ActivateContentType;

public sealed record ActivateContentTypeCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
