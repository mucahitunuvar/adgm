using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SiteLanguageTests
{
    private static SiteLanguage CreateLanguage(string code = "tr") =>
        SiteLanguage.Create(LanguageCode.Create(code).Value, "Türkçe", 1, Guid.NewGuid(), DateTime.UtcNow);

    [Fact]
    public void Create_DefaultsToActiveAndNotDefault()
    {
        var language = CreateLanguage();

        Assert.True(language.IsActive);
        Assert.False(language.IsDefault);
    }

    [Fact]
    public void Rename_WithValidName_Succeeds()
    {
        var language = CreateLanguage();

        var result = language.Rename("Türkçe (Yenilendi)", 5, Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal("Türkçe (Yenilendi)", language.Name);
        Assert.Equal(5, language.SortOrder);
        Assert.NotNull(language.UpdatedAtUtc);
    }

    [Fact]
    public void Rename_WithEmptyName_Fails()
    {
        var language = CreateLanguage();

        var result = language.Rename("   ", 1, Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteLanguage.NameRequired", result.Error.Code);
    }

    [Fact]
    public void Deactivate_WhenNotDefault_Succeeds()
    {
        var language = CreateLanguage();

        var result = language.Deactivate(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.False(language.IsActive);
    }

    [Fact]
    public void Deactivate_WhenDefault_Fails()
    {
        var language = CreateLanguage();
        language.MarkAsDefault(Guid.NewGuid(), DateTime.UtcNow);

        var result = language.Deactivate(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteLanguage.CannotDeactivateDefault", result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.True(language.IsActive);
    }

    [Fact]
    public void MarkAsDefault_WhenActive_Succeeds()
    {
        var language = CreateLanguage();

        var result = language.MarkAsDefault(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.True(language.IsDefault);
    }

    [Fact]
    public void MarkAsDefault_WhenInactive_Fails()
    {
        var language = CreateLanguage();
        language.Deactivate(Guid.NewGuid(), DateTime.UtcNow);

        var result = language.MarkAsDefault(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteLanguage.InactiveCannotBeDefault", result.Error.Code);
        Assert.False(language.IsDefault);
    }

    [Fact]
    public void UnmarkAsDefault_ClearsDefaultFlag()
    {
        var language = CreateLanguage();
        language.MarkAsDefault(Guid.NewGuid(), DateTime.UtcNow);

        language.UnmarkAsDefault(Guid.NewGuid(), DateTime.UtcNow);

        Assert.False(language.IsDefault);
    }
}
