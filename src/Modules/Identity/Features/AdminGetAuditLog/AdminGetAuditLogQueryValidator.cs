using FluentValidation;
using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetAuditLog;

public sealed class AdminGetAuditLogQueryValidator : AbstractValidator<AdminGetAuditLogQuery>
{
    public AdminGetAuditLogQueryValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(q => q.ActionType)
            .Must(actionType => actionType is null || Enum.TryParse<AdminActionType>(actionType, ignoreCase: true, out _))
            .WithMessage("ActionType must be a valid admin action type.");

        RuleFor(q => q)
            .Must(q => q.FromUtc is null || q.ToUtc is null || q.FromUtc <= q.ToUtc)
            .WithMessage("FromUtc must not be later than ToUtc.");
    }
}
