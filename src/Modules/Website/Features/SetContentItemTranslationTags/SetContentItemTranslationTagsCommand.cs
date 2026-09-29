using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemTranslationTags;

public sealed record SetContentItemTranslationTagsCommand(
    Guid Id, string LanguageCode, byte[] RowVersion, IReadOnlyList<string> TagNames) : IRequest<Result>;
