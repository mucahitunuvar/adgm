using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateContentType;

public sealed record DeactivateContentTypeCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
