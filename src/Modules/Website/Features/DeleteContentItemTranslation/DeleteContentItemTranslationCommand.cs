using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentItemTranslation;

public sealed record DeleteContentItemTranslationCommand(Guid Id, string LanguageCode, byte[] RowVersion) : IRequest<Result>;
