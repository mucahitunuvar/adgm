using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsSubmissions;

public sealed class UpdateSiteSettingsSubmissionsCommandValidator : AbstractValidator<UpdateSiteSettingsSubmissionsCommand>
{
    public UpdateSiteSettingsSubmissionsCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.SubmissionReferencePrefix).NotEmpty();
    }
}
