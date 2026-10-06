using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ThirdPartyScriptProviderTests
{
    private static readonly IReadOnlyList<string> AllowedHosts = ["www.googletagmanager.com", "connect.facebook.net"];

    [Theory]
    [InlineData("G-ABCD1234")]
    [InlineData("g-abcd1234")]
    public void CreateGoogleAnalytics4_WithValidMeasurementId_Succeeds(string measurementId)
    {
        var result = ThirdPartyScriptProvider.CreateGoogleAnalytics4(measurementId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ThirdPartyScriptProviderKind.GoogleAnalytics4, result.Value.Kind);
        Assert.Equal("G-ABCD1234", result.Value.MeasurementId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("GTM-ABCD12")]
    [InlineData("G-AB")]
    public void CreateGoogleAnalytics4_WithInvalidMeasurementId_Fails(string? measurementId)
    {
        var result = ThirdPartyScriptProvider.CreateGoogleAnalytics4(measurementId);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScriptProvider.MeasurementIdInvalid", result.Error.Code);
    }

    [Fact]
    public void CreateGoogleTagManager_WithValidContainerId_Succeeds()
    {
        var result = ThirdPartyScriptProvider.CreateGoogleTagManager("GTM-ABCD12");

        Assert.True(result.IsSuccess);
        Assert.Equal(ThirdPartyScriptProviderKind.GoogleTagManager, result.Value.Kind);
        Assert.Equal("GTM-ABCD12", result.Value.ContainerId);
    }

    [Theory]
    [InlineData("G-ABCD1234")]
    [InlineData("GTM-AB")]
    public void CreateGoogleTagManager_WithInvalidContainerId_Fails(string containerId)
    {
        var result = ThirdPartyScriptProvider.CreateGoogleTagManager(containerId);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScriptProvider.ContainerIdInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("12345678901234567890")]
    public void CreateMetaPixel_WithValidPixelId_Succeeds(string pixelId)
    {
        var result = ThirdPartyScriptProvider.CreateMetaPixel(pixelId);

        Assert.True(result.IsSuccess);
        Assert.Equal(pixelId, result.Value.PixelId);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012345678901")]
    [InlineData("abcdefghij")]
    public void CreateMetaPixel_WithInvalidPixelId_Fails(string pixelId)
    {
        var result = ThirdPartyScriptProvider.CreateMetaPixel(pixelId);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScriptProvider.PixelIdInvalid", result.Error.Code);
    }

    [Fact]
    public void CreateExternalScript_WithAllowedHttpsHost_Succeeds()
    {
        var result = ThirdPartyScriptProvider.CreateExternalScript(
            "https://www.googletagmanager.com/gtag/js", async: true, defer: false, AllowedHosts);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://www.googletagmanager.com/gtag/js", result.Value.Src);
        Assert.True(result.Value.Async);
        Assert.False(result.Value.Defer);
    }

    [Fact]
    public void CreateExternalScript_WithNonHttpsUrl_Fails()
    {
        var result = ThirdPartyScriptProvider.CreateExternalScript(
            "http://www.googletagmanager.com/gtag/js", async: false, defer: false, AllowedHosts);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScriptProvider.SrcInvalid", result.Error.Code);
    }

    [Fact]
    public void CreateExternalScript_WithHostNotInAllowList_Fails()
    {
        var result = ThirdPartyScriptProvider.CreateExternalScript(
            "https://evil.example.com/script.js", async: false, defer: false, AllowedHosts);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScriptProvider.HostNotAllowed", result.Error.Code);
    }

    [Fact]
    public void CreateExternalScript_WithEmptyAllowList_AlwaysFails()
    {
        var result = ThirdPartyScriptProvider.CreateExternalScript(
            "https://www.googletagmanager.com/gtag/js", async: false, defer: false, []);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScriptProvider.HostNotAllowed", result.Error.Code);
    }
}
