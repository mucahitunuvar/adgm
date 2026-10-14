using FluentValidation;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Parsing;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetAuditLog;

public sealed class AdminGetAuditLogQueryValidator : AbstractValidator<AdminGetAuditLogQuery>
{
    public AdminGetAuditLogQueryValidator()
    {
        // Page/PageSize are no longer validated here: PagedRequest (SharedKernel) clamps
        // out-of-range values silently at construction instead of failing validation.
        RuleFor(q => q.ActionType)
            .Must(actionType => actionType is null || EnumParsing.TryParseDefined<AdminActionType>(actionType, out _))
            .WithMessage("ActionType must be a valid admin action type.");

        RuleFor(q => q)
            .Must(q => q.FromUtc is null || q.ToUtc is null || q.FromUtc <= q.ToUtc)
            .WithMessage("FromUtc must not be later than ToUtc.");
    }
}
