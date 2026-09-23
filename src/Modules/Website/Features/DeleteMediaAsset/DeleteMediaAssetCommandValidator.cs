using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeleteMediaAsset;

public sealed class DeleteMediaAssetCommandValidator : AbstractValidator<DeleteMediaAssetCommand>
{
    public DeleteMediaAssetCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
