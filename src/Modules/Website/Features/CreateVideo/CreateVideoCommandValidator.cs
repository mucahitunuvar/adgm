using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateVideo;

public sealed class CreateVideoCommandValidator : AbstractValidator<CreateVideoCommand>
{
    public CreateVideoCommandValidator()
    {
        RuleFor(c => c.YouTubeUrl).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.DefaultLanguageTitle).NotEmpty();
    }
}
