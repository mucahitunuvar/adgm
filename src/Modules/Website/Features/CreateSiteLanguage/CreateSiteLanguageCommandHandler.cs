using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;

public sealed class CreateSiteLanguageCommandHandler(
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSiteLanguageCommand, Result<CreateSiteLanguageResponse>>
{
    public async Task<Result<CreateSiteLanguageResponse>> Handle(
        CreateSiteLanguageCommand request, CancellationToken cancellationToken)
    {
        var codeResult = LanguageCode.Create(request.Code);
        if (codeResult.IsFailure)
        {
            return Result.Failure<CreateSiteLanguageResponse>(codeResult.Error);
        }

        var existing = await siteLanguageRepository.GetByCodeAsync(codeResult.Value, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<CreateSiteLanguageResponse>(Error.Conflict(
                "SiteLanguage.CodeAlreadyExists", $"A site language with code '{codeResult.Value}' already exists."));
        }

        var siteLanguage = SiteLanguage.Create(
            codeResult.Value, request.Name, request.SortOrder, currentUserContext.UserId!.Value, DateTime.UtcNow);

        siteLanguageRepository.Add(siteLanguage);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateSiteLanguageResponse(
            siteLanguage.Id, siteLanguage.Code.Value, siteLanguage.Name, siteLanguage.SortOrder,
            siteLanguage.IsDefault, siteLanguage.IsActive));
    }
}
