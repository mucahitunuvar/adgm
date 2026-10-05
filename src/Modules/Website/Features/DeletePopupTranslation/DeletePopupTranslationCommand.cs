using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeletePopupTranslation;

public sealed record DeletePopupTranslationCommand(Guid Id, string LanguageCode, byte[] RowVersion) : IRequest<Result>;
