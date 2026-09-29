using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentCategoryTranslation;

public sealed record DeleteContentCategoryTranslationCommand(Guid TypeId, Guid Id, string LanguageCode, byte[] RowVersion) : IRequest<Result>;
