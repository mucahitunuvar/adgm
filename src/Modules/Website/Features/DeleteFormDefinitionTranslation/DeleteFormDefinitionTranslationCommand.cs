using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteFormDefinitionTranslation;

public sealed record DeleteFormDefinitionTranslationCommand(Guid Id, string LanguageCode, byte[] RowVersion) : IRequest<Result>;
