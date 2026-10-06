using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteThirdPartyScriptTranslation;

public sealed record DeleteThirdPartyScriptTranslationCommand(Guid Id, string LanguageCode, byte[] RowVersion) : IRequest<Result>;
