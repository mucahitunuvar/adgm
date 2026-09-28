using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentTypeTranslation;

public sealed record DeleteContentTypeTranslationCommand(Guid Id, string LanguageCode, byte[] RowVersion) : IRequest<Result>;
