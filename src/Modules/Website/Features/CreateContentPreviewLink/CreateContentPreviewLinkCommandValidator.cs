using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentPreviewLink;

public sealed class CreateContentPreviewLinkCommandValidator : AbstractValidator<CreateContentPreviewLinkCommand>
{
    public CreateContentPreviewLinkCommandValidator()
    {
        RuleFor(c => c.DurationHours)
            .InclusiveBetween(1, CreateContentPreviewLinkCommandHandler.MaxDurationHours)
            .When(c => c.DurationHours is not null);
    }
}
