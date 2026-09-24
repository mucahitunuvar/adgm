using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBankAccounts;

public sealed record UpdateSiteSettingsBankAccountsCommand(
    byte[] RowVersion, IReadOnlyList<UpdateSiteSettingsBankAccountInput> BankAccounts) : IRequest<Result>;
