using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteVideoTranslation;

public sealed record DeleteVideoTranslationCommand(Guid Id, string LanguageCode, byte[] RowVersion) : IRequest<Result>;
