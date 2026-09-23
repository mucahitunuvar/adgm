using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;

public sealed class UploadMediaAssetCommandValidator : AbstractValidator<UploadMediaAssetCommand>
{
    public UploadMediaAssetCommandValidator()
    {
        RuleFor(c => c.FileName).NotEmpty();
        RuleFor(c => c.ContentType).NotEmpty();
    }
}
