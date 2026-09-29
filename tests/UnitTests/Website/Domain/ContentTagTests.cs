using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentTagTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidName_Succeeds()
    {
        var result = ContentTag.Create(Tr, "Gençlik", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Gençlik", result.Value.Name);
        Assert.Equal("genclik", result.Value.Slug);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_Fails(string? name)
    {
        var result = ContentTag.Create(Tr, name, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentTag.NameInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithNameLongerThanMax_Fails()
    {
        var tooLong = new string('a', ContentTag.MaxNameLength + 1);

        var result = ContentTag.Create(Tr, tooLong, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentTag.NameInvalid", result.Error.Code);
    }

    [Fact]
    public void Rename_WithValidName_UpdatesNameAndSlug()
    {
        var tag = ContentTag.Create(Tr, "Gençlik", UserId, Now).Value;

        var result = tag.Rename("Spor", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Spor", tag.Name);
        Assert.Equal("spor", tag.Slug);
        Assert.Equal(Now, tag.UpdatedAtUtc);
    }

    [Fact]
    public void Rename_WithEmptyName_Fails()
    {
        var tag = ContentTag.Create(Tr, "Gençlik", UserId, Now).Value;

        var result = tag.Rename("", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentTag.NameInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_NormalizesTurkishCharactersInSlugTheSameAsOtherSlugs()
    {
        var lower = ContentTag.Create(Tr, "gençlik", UserId, Now).Value;
        var upperFirst = ContentTag.Create(Tr, "Gençlik", UserId, Now).Value;

        Assert.Equal(lower.Slug, upperFirst.Slug);
    }
}
