using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScriptTranslation;

public sealed record UpdateThirdPartyScriptTranslationCommand(
    Guid Id, string LanguageCode, byte[] RowVersion, string? Name, string? Purpose) : IRequest<Result>;
