using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemAttachments;

public sealed record SetContentItemAttachmentsCommand(Guid Id, byte[] RowVersion, IReadOnlyList<AttachmentInput> Items) : IRequest<Result>;
