using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateMediaAsset;

public sealed class UpdateMediaAssetCommandValidator : AbstractValidator<UpdateMediaAssetCommand>
{
    public UpdateMediaAssetCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
