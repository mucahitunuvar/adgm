using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ActivateSiteLanguage;

public sealed record ActivateSiteLanguageCommand(Guid Id) : IRequest<Result>;
