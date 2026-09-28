using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentItemTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 24, 12, 0, 0, DateTimeKind.Utc);

    private static Result<ContentItem> CreateItem(bool contentTypeSupportsDetailImage = false, Guid? detailImageMediaId = null) =>
        ContentItem.Create(
            ContentTypeId, contentTypeSupportsDetailImage, 1, false, null, detailImageMediaId,
            Tr, "Yeni Haber", null, "haberler", null, "<p>gövde</p>", EmptySeo, UserId, Now);

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = CreateItem();

        Assert.True(result.IsSuccess);
        Assert.Equal(ContentItemStatus.Draft, result.Value.Status);
        Assert.Single(result.Value.Translations);
        Assert.Equal("haberler/yeni-haber", result.Value.Translations[0].FullPath);
    }

    [Fact]
    public void Create_WithDetailImageButTypeDoesNotSupportIt_Fails()
    {
        var result = CreateItem(contentTypeSupportsDetailImage: false, detailImageMediaId: Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.DetailImageNotSupported", result.Error.Code);
    }

    [Fact]
    public void Create_WithDetailImageAndTypeSupportsIt_Succeeds()
    {
        var result = CreateItem(contentTypeSupportsDetailImage: true, detailImageMediaId: Guid.NewGuid());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void SetTranslation_AddsNewLanguage()
    {
        var item = CreateItem().Value;

        var result = item.SetTranslation(En, "News", null, "news", null, "<p>body</p>", EmptySeo, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, item.Translations.Count);
        Assert.Contains(item.Translations, t => t.LanguageCode == En && t.FullPath == "news/news");
    }

    [Fact]
    public void RemoveTranslation_DefaultLanguage_Fails()
    {
        var item = CreateItem().Value;
        item.SetTranslation(En, "News", null, "news", null, null, EmptySeo, UserId, Now);

        var result = item.RemoveTranslation(Tr, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.CannotDeleteDefaultTranslation", result.Error.Code);
    }

    [Fact]
    public void RemoveTranslation_NonDefaultLanguage_Succeeds()
    {
        var item = CreateItem().Value;
        item.SetTranslation(En, "News", null, "news", null, null, EmptySeo, UserId, Now);

        var result = item.RemoveTranslation(En, Tr, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(item.Translations);
    }

    [Theory]
    [InlineData(ContentItemStatus.Draft, true)]
    [InlineData(ContentItemStatus.Unpublished, true)]
    [InlineData(ContentItemStatus.Published, false)]
    [InlineData(ContentItemStatus.Archived, false)]
    public void Publish_OnlyAllowedFromDraftOrUnpublished(ContentItemStatus fromStatus, bool expectSuccess)
    {
        var item = CreateItem().Value;
        MoveTo(item, fromStatus);

        var result = item.Publish(null, null, contentTypeIsActive: true, Tr, UserId, Now);

        Assert.Equal(expectSuccess, result.IsSuccess);
        if (expectSuccess)
        {
            Assert.Equal(ContentItemStatus.Published, item.Status);
            Assert.Equal(UserId, item.PublishedByUserId);
            Assert.Equal(Now, item.PublishedAtUtc);
        }
    }

    [Fact]
    public void Publish_WhenContentTypeInactive_Fails()
    {
        var item = CreateItem().Value;

        var result = item.Publish(null, null, contentTypeIsActive: false, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.ContentTypeInactive", result.Error.Code);
    }

    [Fact]
    public void Publish_WithUnpublishAtUtcBeforeNow_Fails()
    {
        var item = CreateItem().Value;

        var result = item.Publish(null, Now.AddMinutes(-1), contentTypeIsActive: true, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.UnpublishBeforePublish", result.Error.Code);
    }

    [Fact]
    public void Publish_WithUnpublishAtUtcBeforePublishAtUtc_Fails()
    {
        var item = CreateItem().Value;
        var publishAt = Now.AddDays(1);

        var result = item.Publish(publishAt, publishAt.AddMinutes(-1), contentTypeIsActive: true, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.UnpublishBeforePublish", result.Error.Code);
    }

    [Fact]
    public void Unpublish_OnlyAllowedFromPublished()
    {
        var item = CreateItem().Value;

        Assert.True(item.Unpublish(UserId, Now).IsFailure);

        item.Publish(null, null, true, Tr, UserId, Now);
        var result = item.Unpublish(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ContentItemStatus.Unpublished, item.Status);
    }

    [Theory]
    [InlineData(ContentItemStatus.Draft, false)]
    [InlineData(ContentItemStatus.Published, true)]
    [InlineData(ContentItemStatus.Unpublished, true)]
    [InlineData(ContentItemStatus.Archived, false)]
    public void Archive_AllowedFromPublishedOrUnpublished(ContentItemStatus fromStatus, bool expectSuccess)
    {
        var item = CreateItem().Value;
        MoveTo(item, fromStatus);

        var result = item.Archive(UserId, Now);

        Assert.Equal(expectSuccess, result.IsSuccess);
        if (expectSuccess)
        {
            Assert.Equal(ContentItemStatus.Archived, item.Status);
        }
    }

    [Fact]
    public void Unarchive_FromArchived_GoesToUnpublished_NeverDirectlyToPublished()
    {
        var item = CreateItem().Value;
        MoveTo(item, ContentItemStatus.Archived);

        var result = item.Unarchive(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ContentItemStatus.Unpublished, item.Status);
    }

    [Fact]
    public void Unarchive_FromNonArchived_Fails()
    {
        var item = CreateItem().Value;

        var result = item.Unarchive(UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.InvalidStatusTransition", result.Error.Code);
    }

    [Fact]
    public void Schedule_OnlyAllowedWhenPublished()
    {
        var item = CreateItem().Value;

        var beforePublish = item.Schedule(null, Now.AddDays(1), UserId, Now);
        Assert.True(beforePublish.IsFailure);

        item.Publish(null, null, true, Tr, UserId, Now);
        var afterPublish = item.Schedule(null, Now.AddDays(1), UserId, Now);
        Assert.True(afterPublish.IsSuccess);
        Assert.Equal(Now.AddDays(1), item.UnpublishAtUtc);
    }

    [Fact]
    public void IsVisible_TrueOnlyWhenPublishedAndWithinSchedule()
    {
        var item = CreateItem().Value;
        Assert.False(item.IsVisible(Now));

        item.Publish(null, null, true, Tr, UserId, Now);
        Assert.True(item.IsVisible(Now));

        item.Schedule(Now.AddDays(1), null, UserId, Now);
        Assert.False(item.IsVisible(Now));
        Assert.True(item.IsVisible(Now.AddDays(2)));
    }

    [Fact]
    public void IsVisible_FalseAfterUnpublishAtUtcPasses()
    {
        var item = CreateItem().Value;
        item.Publish(null, null, true, Tr, UserId, Now);
        item.Schedule(null, Now.AddDays(1), UserId, Now);

        Assert.True(item.IsVisible(Now));
        Assert.False(item.IsVisible(Now.AddDays(2)));
    }

    private static void MoveTo(ContentItem item, ContentItemStatus status)
    {
        switch (status)
        {
            case ContentItemStatus.Draft:
                return;
            case ContentItemStatus.Published:
                item.Publish(null, null, true, Tr, UserId, Now);
                return;
            case ContentItemStatus.Unpublished:
                item.Publish(null, null, true, Tr, UserId, Now);
                item.Unpublish(UserId, Now);
                return;
            case ContentItemStatus.Archived:
                item.Publish(null, null, true, Tr, UserId, Now);
                item.Archive(UserId, Now);
                return;
        }
    }
}
