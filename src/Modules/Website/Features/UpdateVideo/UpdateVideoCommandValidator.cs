using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateVideo;

public sealed class UpdateVideoCommandValidator : AbstractValidator<UpdateVideoCommand>
{
    public UpdateVideoCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.YouTubeUrl).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
    }
}
