using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SetDefaultSiteLanguage;

public sealed record SetDefaultSiteLanguageCommand(Guid Id) : IRequest<Result>;
