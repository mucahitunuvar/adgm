using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeletePartnerTranslation;

public sealed record DeletePartnerTranslationCommand(Guid Id, string LanguageCode, byte[] RowVersion) : IRequest<Result>;
