using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;

public sealed record CreateSiteLanguageCommand(string Code, string Name, int SortOrder)
    : IRequest<Result<CreateSiteLanguageResponse>>;
