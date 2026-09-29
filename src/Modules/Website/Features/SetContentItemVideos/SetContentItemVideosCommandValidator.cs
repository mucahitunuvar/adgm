using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemVideos;

public sealed class SetContentItemVideosCommandValidator : AbstractValidator<SetContentItemVideosCommand>
{
    public SetContentItemVideosCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.VideoIds).NotNull();
    }
}
