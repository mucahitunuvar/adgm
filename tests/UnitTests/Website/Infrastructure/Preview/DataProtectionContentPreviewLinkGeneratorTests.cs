using GenclikMerkezi.Modules.Website.Infrastructure.Preview;
using Microsoft.AspNetCore.DataProtection;

namespace GenclikMerkezi.UnitTests.Website.Infrastructure.Preview;

public class DataProtectionContentPreviewLinkGeneratorTests
{
    private static DataProtectionContentPreviewLinkGenerator CreateGenerator() =>
        new(DataProtectionProvider.Create("GenclikMerkezi.Tests"));

    [Fact]
    public void ValidateToken_ForFreshlyGeneratedToken_ReturnsContentItemIdAndLanguage()
    {
        var generator = CreateGenerator();
        var contentItemId = Guid.NewGuid();

        var token = generator.GenerateToken(contentItemId, "tr", TimeSpan.FromHours(1));
        var result = generator.ValidateToken(token);

        Assert.True(result.IsSuccess);
        Assert.Equal(contentItemId, result.Value.ContentItemId);
        Assert.Equal("tr", result.Value.LanguageCode);
    }

    [Fact]
    public void ValidateToken_WithoutLanguageCode_ReturnsNullLanguage()
    {
        var generator = CreateGenerator();
        var contentItemId = Guid.NewGuid();

        var token = generator.GenerateToken(contentItemId, null, TimeSpan.FromHours(1));
        var result = generator.ValidateToken(token);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.LanguageCode);
    }

    [Fact]
    public async Task ValidateToken_AfterExpiration_Fails()
    {
        var generator = CreateGenerator();
        var token = generator.GenerateToken(Guid.NewGuid(), "tr", TimeSpan.FromMilliseconds(1));
        await Task.Delay(50);

        var result = generator.ValidateToken(token);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentPreview.InvalidToken", result.Error.Code);
    }

    [Fact]
    public void ValidateToken_WithTamperedPayload_Fails()
    {
        var generator = CreateGenerator();
        var token = generator.GenerateToken(Guid.NewGuid(), "tr", TimeSpan.FromHours(1));
        var tampered = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

        var result = generator.ValidateToken(tampered);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentPreview.InvalidToken", result.Error.Code);
    }

    [Fact]
    public void ValidateToken_WithTokenFromDifferentProtectorInstance_ButSamePurposeAndApplication_StillValidates()
    {
        // A fresh generator sharing the same ephemeral provider instance represents two requests
        // hitting the same running application - the token must still validate across handler calls.
        var provider = DataProtectionProvider.Create("GenclikMerkezi.Tests");
        var first = new DataProtectionContentPreviewLinkGenerator(provider);
        var second = new DataProtectionContentPreviewLinkGenerator(provider);
        var contentItemId = Guid.NewGuid();

        var token = first.GenerateToken(contentItemId, "en", TimeSpan.FromHours(1));
        var result = second.ValidateToken(token);

        Assert.True(result.IsSuccess);
        Assert.Equal(contentItemId, result.Value.ContentItemId);
    }

    [Fact]
    public void ValidateToken_ForGarbageInput_Fails()
    {
        var generator = CreateGenerator();

        var result = generator.ValidateToken("not-a-real-token");

        Assert.True(result.IsFailure);
        Assert.Equal("ContentPreview.InvalidToken", result.Error.Code);
    }
}
