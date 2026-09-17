using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.SharedKernel.Domain;

public class FileValidationPolicyTests
{
    private static FileValidationPolicy CreatePhotoPolicy() =>
        FileValidationPolicy.Create(["jpg", "png"], ["image/jpeg", "image/png"], maxSizeInBytes: 2 * 1024 * 1024);

    [Fact]
    public void Validate_WithAllowedExtensionContentTypeAndSize_Succeeds()
    {
        var policy = CreatePhotoPolicy();

        var result = policy.Validate("photo.jpg", "image/jpeg", sizeInBytes: 1024);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_WithDisallowedExtension_ReturnsValidationFailure()
    {
        var policy = CreatePhotoPolicy();

        var result = policy.Validate("document.pdf", "image/jpeg", sizeInBytes: 1024);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("FileStorage.InvalidExtension", result.Error.Code);
    }

    [Fact]
    public void Validate_WithDisallowedContentType_ReturnsValidationFailure()
    {
        var policy = CreatePhotoPolicy();

        var result = policy.Validate("photo.jpg", "application/pdf", sizeInBytes: 1024);

        Assert.True(result.IsFailure);
        Assert.Equal("FileStorage.InvalidContentType", result.Error.Code);
    }

    [Fact]
    public void Validate_WithSizeOverLimit_ReturnsValidationFailure()
    {
        var policy = CreatePhotoPolicy();

        var result = policy.Validate("photo.jpg", "image/jpeg", sizeInBytes: 3 * 1024 * 1024);

        Assert.True(result.IsFailure);
        Assert.Equal("FileStorage.FileTooLarge", result.Error.Code);
    }

    [Fact]
    public void Validate_ExtensionMatchIsCaseInsensitive()
    {
        var policy = CreatePhotoPolicy();

        var result = policy.Validate("photo.JPG", "image/jpeg", sizeInBytes: 1024);

        Assert.True(result.IsSuccess);
    }
}
