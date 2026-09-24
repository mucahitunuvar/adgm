using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBankAccounts;

public sealed class UpdateSiteSettingsBankAccountsCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsBankAccountsCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsBankAccountsCommand request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken);
        var isNew = settings is null;
        settings ??= SiteSettings.CreateDefault();

        if (!isNew && !request.RowVersion.SequenceEqual(settings.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "SiteSettings.ConcurrencyConflict", "Site settings were changed by someone else. Reload and try again."));
        }

        var bankAccounts = new List<BankAccount>();
        foreach (var input in request.BankAccounts)
        {
            var ibanResult = Iban.Create(input.Iban);
            if (ibanResult.IsFailure)
            {
                return ibanResult;
            }

            if (!Enum.TryParse<BankAccountCurrency>(input.Currency, out var currency))
            {
                return Result.Failure(Error.Validation("BankAccount.InvalidCurrency", $"Currency '{input.Currency}' is not supported."));
            }

            bankAccounts.Add(BankAccount.Create(ibanResult.Value, input.BankName, input.AccountHolder, currency, input.Description, input.SortOrder, input.IsActive));
        }

        if (isNew)
        {
            siteSettingsRepository.Add(settings);
        }

        settings.ReplaceBankAccounts(bankAccounts, currentUserContext.UserId!.Value, DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
