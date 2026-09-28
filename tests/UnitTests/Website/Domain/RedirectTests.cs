using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class RedirectTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateAutomatic_WithValidInput_Succeeds()
    {
        var targetId = Guid.NewGuid();

        var result = Redirect.CreateAutomatic(Tr, "eski-yol", targetId, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("eski-yol", result.Value.FromPath);
        Assert.True(result.Value.IsAutomatic);
        Assert.Equal(RedirectTargetKind.ContentItem, result.Value.TargetKind);
        Assert.Equal(targetId, result.Value.TargetContentItemId);
        Assert.Equal(RedirectStatusCode.MovedPermanently, result.Value.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("///")]
    public void Create_WithEmptyFromPath_Fails(string? fromPath)
    {
        var result = Redirect.Create(
            Tr, fromPath, RedirectTargetKind.Path, null, "hedef-yol", RedirectStatusCode.MovedPermanently, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.FromPathRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithContentItemTargetAndNoId_Fails()
    {
        var result = Redirect.Create(
            Tr, "eski-yol", RedirectTargetKind.ContentItem, null, null, RedirectStatusCode.MovedPermanently, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetContentItemIdRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithContentItemTarget_Succeeds()
    {
        var targetId = Guid.NewGuid();

        var result = Redirect.Create(
            Tr, "eski-yol", RedirectTargetKind.ContentItem, targetId, null, RedirectStatusCode.Found, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(targetId, result.Value.TargetContentItemId);
        Assert.Null(result.Value.TargetPath);
        Assert.False(result.Value.IsAutomatic);
    }

    [Fact]
    public void Create_WithEmptyTargetPath_Fails()
    {
        var result = Redirect.Create(
            Tr, "eski-yol", RedirectTargetKind.Path, null, "  ", RedirectStatusCode.MovedPermanently, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetPathRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithHttpAbsoluteTargetPath_Fails()
    {
        var result = Redirect.Create(
            Tr, "eski-yol", RedirectTargetKind.Path, null, "http://example.com", RedirectStatusCode.MovedPermanently, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetPathMustBeHttps", result.Error.Code);
    }

    [Fact]
    public void Create_WithHttpsAbsoluteTargetPath_Succeeds()
    {
        var result = Redirect.Create(
            Tr, "eski-yol", RedirectTargetKind.Path, null, "https://example.com/hedef", RedirectStatusCode.MovedPermanently, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/hedef", result.Value.TargetPath);
    }

    [Fact]
    public void Create_WithRelativeTargetPath_NormalizesSlashes()
    {
        var result = Redirect.Create(
            Tr, "eski-yol", RedirectTargetKind.Path, null, "/yeni-yol/", RedirectStatusCode.MovedPermanently, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("yeni-yol", result.Value.TargetPath);
    }

    [Fact]
    public void Create_WithTargetPathEqualToFromPath_Fails()
    {
        var result = Redirect.Create(
            Tr, "ayni-yol", RedirectTargetKind.Path, null, "ayni-yol", RedirectStatusCode.MovedPermanently, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetCannotBeFromPath", result.Error.Code);
    }

    [Fact]
    public void Update_OnManualRedirect_ChangesTargetAndStatusCode()
    {
        var redirect = Redirect.Create(
            Tr, "eski-yol", RedirectTargetKind.Path, null, "hedef-1", RedirectStatusCode.MovedPermanently, UserId, Now).Value;
        var newTargetId = Guid.NewGuid();

        var result = redirect.Update(RedirectTargetKind.ContentItem, newTargetId, null, RedirectStatusCode.Found, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(RedirectTargetKind.ContentItem, redirect.TargetKind);
        Assert.Equal(newTargetId, redirect.TargetContentItemId);
        Assert.Null(redirect.TargetPath);
        Assert.Equal(RedirectStatusCode.Found, redirect.StatusCode);
        Assert.Equal(UserId, redirect.UpdatedByUserId);
        Assert.Equal(Now, redirect.UpdatedAtUtc);
    }

    [Fact]
    public void Update_OnAutomaticRedirect_Fails()
    {
        var redirect = Redirect.CreateAutomatic(Tr, "eski-yol", Guid.NewGuid(), UserId, Now).Value;

        var result = redirect.Update(RedirectTargetKind.Path, null, "yeni-yol", RedirectStatusCode.MovedPermanently, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.CannotEditAutomaticRedirect", result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void Update_WithTargetPathEqualToFromPath_Fails()
    {
        var redirect = Redirect.Create(
            Tr, "eski-yol", RedirectTargetKind.Path, null, "hedef-1", RedirectStatusCode.MovedPermanently, UserId, Now).Value;

        var result = redirect.Update(RedirectTargetKind.Path, null, "eski-yol", RedirectStatusCode.MovedPermanently, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetCannotBeFromPath", result.Error.Code);
    }

    [Fact]
    public void RecordHit_IncrementsHitCountAndUpdatesLastHitAtUtc()
    {
        var redirect = Redirect.CreateAutomatic(Tr, "eski-yol", Guid.NewGuid(), UserId, Now).Value;

        redirect.RecordHit(Now.AddMinutes(5));
        redirect.RecordHit(Now.AddMinutes(10));

        Assert.Equal(2, redirect.HitCount);
        Assert.Equal(Now.AddMinutes(10), redirect.LastHitAtUtc);
    }
}
