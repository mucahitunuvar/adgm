using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetSiteLanguages;

public class GetSiteLanguagesQueryHandlerTests
{
    private readonly FakeSiteLanguageRepository _repository = new();

    [Fact]
    public async Task Handle_ReturnsAllLanguagesOrderedBySortOrder()
    {
        var english = SiteLanguage.Create(LanguageCode.Create("en").Value, "English", 2, Guid.NewGuid(), DateTime.UtcNow);
        var turkish = SiteLanguage.Create(LanguageCode.Create("tr").Value, "Türkçe", 1, Guid.NewGuid(), DateTime.UtcNow);
        _repository.Seed(english);
        _repository.Seed(turkish);

        var handler = new GetSiteLanguagesQueryHandler(_repository);
        var result = await handler.Handle(new GetSiteLanguagesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal("tr", result.Value.Items[0].Code);
        Assert.Equal("en", result.Value.Items[1].Code);
    }
}
