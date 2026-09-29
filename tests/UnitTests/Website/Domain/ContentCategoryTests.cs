using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentCategoryTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = ContentCategory.Create(ContentTypeId, null, 1, Tr, "Kültür", null, EmptySeo, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsActive);
        Assert.Single(result.Value.Translations);
        Assert.Equal("kultur", result.Value.Translations[0].Slug);
    }

    [Fact]
    public void Create_WithEmptyName_Fails()
    {
        var result = ContentCategory.Create(ContentTypeId, null, 1, Tr, "  ", null, EmptySeo, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentCategoryTranslation.NameInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithParentId_Succeeds()
    {
        var parentId = Guid.NewGuid();

        var result = ContentCategory.Create(ContentTypeId, parentId, 1, Tr, "Alt Kategori", null, EmptySeo, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(parentId, result.Value.ParentId);
    }

    [Fact]
    public void UpdateCore_ChangesSortOrderAndTouchesRowVersion()
    {
        var category = ContentCategory.Create(ContentTypeId, null, 1, Tr, "Kültür", null, EmptySeo, UserId, Now).Value;
        var originalRowVersion = category.RowVersion;

        var result = category.UpdateCore(5, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, category.SortOrder);
        Assert.NotEqual(originalRowVersion, category.RowVersion);
    }

    [Fact]
    public void SetTranslation_ForNewLanguage_AddsTranslation()
    {
        var category = ContentCategory.Create(ContentTypeId, null, 1, Tr, "Kültür", null, EmptySeo, UserId, Now).Value;

        var result = category.SetTranslation(En, "Culture", null, EmptySeo, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, category.Translations.Count);
    }

    [Fact]
    public void SetTranslation_ForExistingLanguage_UpdatesInPlace()
    {
        var category = ContentCategory.Create(ContentTypeId, null, 1, Tr, "Kültür", null, EmptySeo, UserId, Now).Value;

        var result = category.SetTranslation(Tr, "Sanat", null, EmptySeo, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(category.Translations);
        Assert.Equal("Sanat", category.Translations[0].Name);
    }

    [Fact]
    public void RemoveTranslation_ForDefaultLanguage_Fails()
    {
        var category = ContentCategory.Create(ContentTypeId, null, 1, Tr, "Kültür", null, EmptySeo, UserId, Now).Value;

        var result = category.RemoveTranslation(Tr, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentCategory.CannotDeleteDefaultTranslation", result.Error.Code);
    }

    [Fact]
    public void RemoveTranslation_ForNonDefaultLanguage_Succeeds()
    {
        var category = ContentCategory.Create(ContentTypeId, null, 1, Tr, "Kültür", null, EmptySeo, UserId, Now).Value;
        category.SetTranslation(En, "Culture", null, EmptySeo, UserId, Now);

        var result = category.RemoveTranslation(En, Tr, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(category.Translations);
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var category = ContentCategory.Create(ContentTypeId, null, 1, Tr, "Kültür", null, EmptySeo, UserId, Now).Value;
        category.Deactivate(UserId, Now);

        var result = category.Activate(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.True(category.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var category = ContentCategory.Create(ContentTypeId, null, 1, Tr, "Kültür", null, EmptySeo, UserId, Now).Value;

        var result = category.Deactivate(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.False(category.IsActive);
    }
}
