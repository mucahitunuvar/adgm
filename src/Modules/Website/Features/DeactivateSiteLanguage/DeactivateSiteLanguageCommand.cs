using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateSiteLanguage;

public sealed record DeactivateSiteLanguageCommand(Guid Id) : IRequest<Result>;
