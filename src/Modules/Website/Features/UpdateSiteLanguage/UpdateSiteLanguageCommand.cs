using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteLanguage;

public sealed record UpdateSiteLanguageCommand(Guid Id, string Name, int SortOrder) : IRequest<Result>;
