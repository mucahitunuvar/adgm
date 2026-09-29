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

    private static Result<ContentItem> CreateItem(
        bool contentTypeSupportsDetailImage = false, Guid? detailImageMediaId = null, Guid? parentId = null,
        IReadOnlyList<string>? ancestorSlugs = null) =>
        ContentItem.Create(
            ContentTypeId, parentId, contentTypeSupportsDetailImage, 1, false, null, detailImageMediaId,
            Tr, "Yeni Haber", null, "haberler", ancestorSlugs ?? [], null, "<p>gövde</p>", EmptySeo, UserId, Now);

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
    public void Create_WithAncestorSlugs_ComputesNestedFullPath()
    {
        var result = CreateItem(ancestorSlugs: ["ust-kategori", "alt-kategori"]);

        Assert.True(result.IsSuccess);
        Assert.Equal("haberler/ust-kategori/alt-kategori/yeni-haber", result.Value.Translations[0].FullPath);
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

        var result = item.SetTranslation(En, "News", null, "news", [], null, "<p>body</p>", EmptySeo, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, item.Translations.Count);
        Assert.Contains(item.Translations, t => t.LanguageCode == En && t.FullPath == "news/news");
    }

    [Fact]
    public void RemoveTranslation_DefaultLanguage_Fails()
    {
        var item = CreateItem().Value;
        item.SetTranslation(En, "News", null, "news", [], null, null, EmptySeo, UserId, Now);

        var result = item.RemoveTranslation(Tr, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.CannotDeleteDefaultTranslation", result.Error.Code);
    }

    [Fact]
    public void RemoveTranslation_NonDefaultLanguage_Succeeds()
    {
        var item = CreateItem().Value;
        item.SetTranslation(En, "News", null, "news", [], null, null, EmptySeo, UserId, Now);

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

        var result = item.Publish(null, null, contentTypeIsActive: true, parentIsPublished: true, Tr, UserId, Now);

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

        var result = item.Publish(null, null, contentTypeIsActive: false, parentIsPublished: true, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.ContentTypeInactive", result.Error.Code);
    }

    [Fact]
    public void Publish_WithUnpublishedParent_Fails()
    {
        var item = CreateItem(parentId: Guid.NewGuid()).Value;

        var result = item.Publish(null, null, contentTypeIsActive: true, parentIsPublished: false, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.ParentNotPublished", result.Error.Code);
    }

    [Fact]
    public void Publish_WithPublishedParent_Succeeds()
    {
        var item = CreateItem(parentId: Guid.NewGuid()).Value;

        var result = item.Publish(null, null, contentTypeIsActive: true, parentIsPublished: true, Tr, UserId, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Publish_WithUnpublishAtUtcBeforeNow_Fails()
    {
        var item = CreateItem().Value;

        var result = item.Publish(null, Now.AddMinutes(-1), contentTypeIsActive: true, parentIsPublished: true, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.UnpublishBeforePublish", result.Error.Code);
    }

    [Fact]
    public void Publish_WithUnpublishAtUtcBeforePublishAtUtc_Fails()
    {
        var item = CreateItem().Value;
        var publishAt = Now.AddDays(1);

        var result = item.Publish(publishAt, publishAt.AddMinutes(-1), contentTypeIsActive: true, parentIsPublished: true, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.UnpublishBeforePublish", result.Error.Code);
    }

    [Fact]
    public void Unpublish_OnlyAllowedFromPublished()
    {
        var item = CreateItem().Value;

        Assert.True(item.Unpublish(0, UserId, Now).IsFailure);

        item.Publish(null, null, true, true, Tr, UserId, Now);
        var result = item.Unpublish(0, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ContentItemStatus.Unpublished, item.Status);
    }

    [Fact]
    public void Unpublish_WithPublishedChildren_Fails()
    {
        var item = CreateItem().Value;
        item.Publish(null, null, true, true, Tr, UserId, Now);

        var result = item.Unpublish(2, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.HasPublishedChildren", result.Error.Code);
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

        var result = item.Archive(0, UserId, Now);

        Assert.Equal(expectSuccess, result.IsSuccess);
        if (expectSuccess)
        {
            Assert.Equal(ContentItemStatus.Archived, item.Status);
        }
    }

    [Fact]
    public void Archive_WithPublishedChildren_Fails()
    {
        var item = CreateItem().Value;
        item.Publish(null, null, true, true, Tr, UserId, Now);

        var result = item.Archive(1, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.HasPublishedChildren", result.Error.Code);
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

        item.Publish(null, null, true, true, Tr, UserId, Now);
        var afterPublish = item.Schedule(null, Now.AddDays(1), UserId, Now);
        Assert.True(afterPublish.IsSuccess);
        Assert.Equal(Now.AddDays(1), item.UnpublishAtUtc);
    }

    [Fact]
    public void IsVisible_TrueOnlyWhenPublishedAndWithinSchedule()
    {
        var item = CreateItem().Value;
        Assert.False(item.IsVisible(Now));

        item.Publish(null, null, true, true, Tr, UserId, Now);
        Assert.True(item.IsVisible(Now));

        item.Schedule(Now.AddDays(1), null, UserId, Now);
        Assert.False(item.IsVisible(Now));
        Assert.True(item.IsVisible(Now.AddDays(2)));
    }

    [Fact]
    public void IsVisible_FalseAfterUnpublishAtUtcPasses()
    {
        var item = CreateItem().Value;
        item.Publish(null, null, true, true, Tr, UserId, Now);
        item.Schedule(null, Now.AddDays(1), UserId, Now);

        Assert.True(item.IsVisible(Now));
        Assert.False(item.IsVisible(Now.AddDays(2)));
    }

    [Fact]
    public void SetParent_UpdatesParentIdAndTouchesRowVersion()
    {
        var item = CreateItem().Value;
        var originalRowVersion = item.RowVersion;
        var parentId = Guid.NewGuid();

        item.SetParent(parentId, UserId, Now);

        Assert.Equal(parentId, item.ParentId);
        Assert.NotEqual(originalRowVersion, item.RowVersion);
    }

    [Fact]
    public void SetCategories_WithValidIds_ReplacesWholeList()
    {
        var item = CreateItem().Value;
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();

        var result = item.SetCategories([categoryId1, categoryId2], UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal([categoryId1, categoryId2], item.CategoryIds);
    }

    [Fact]
    public void SetCategories_DeduplicatesIds()
    {
        var item = CreateItem().Value;
        var categoryId = Guid.NewGuid();

        var result = item.SetCategories([categoryId, categoryId], UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(item.CategoryIds);
    }

    [Fact]
    public void SetCategories_ExceedingMax_Fails()
    {
        var item = CreateItem().Value;
        var tooMany = Enumerable.Range(0, ContentItem.MaxCategories + 1).Select(_ => Guid.NewGuid()).ToList();

        var result = item.SetCategories(tooMany, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.TooManyCategories", result.Error.Code);
    }

    [Fact]
    public void SetCategories_CalledAgain_ReplacesRatherThanAppends()
    {
        var item = CreateItem().Value;
        item.SetCategories([Guid.NewGuid()], UserId, Now);
        var newCategoryId = Guid.NewGuid();

        item.SetCategories([newCategoryId], UserId, Now);

        Assert.Equal([newCategoryId], item.CategoryIds);
    }

    [Fact]
    public void SetTranslationTags_WithValidIds_ReplacesWholeList()
    {
        var item = CreateItem().Value;
        var tagId = Guid.NewGuid();

        var result = item.SetTranslationTags(Tr, [tagId], UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal([tagId], item.Translations[0].TagIds);
    }

    [Fact]
    public void SetTranslationTags_ExceedingMax_Fails()
    {
        var item = CreateItem().Value;
        var tooMany = Enumerable.Range(0, ContentItemTranslation.MaxTags + 1).Select(_ => Guid.NewGuid()).ToList();

        var result = item.SetTranslationTags(Tr, tooMany, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItemTranslation.TooManyTags", result.Error.Code);
    }

    [Fact]
    public void SetTranslationTags_ForMissingLanguage_Fails()
    {
        var item = CreateItem().Value;

        var result = item.SetTranslationTags(En, [Guid.NewGuid()], UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.TranslationNotFound", result.Error.Code);
    }

    [Fact]
    public void SetGallery_WithValidItems_ReplacesWholeList()
    {
        var item = CreateItem().Value;
        var galleryItem = ContentItemGalleryItem.Create(Guid.NewGuid(), 0, []);

        var result = item.SetGallery([galleryItem], UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(item.GalleryItems);
    }

    [Fact]
    public void SetGallery_ExceedingMax_Fails()
    {
        var item = CreateItem().Value;
        var tooMany = Enumerable.Range(0, ContentItem.MaxGalleryItems + 1)
            .Select(i => ContentItemGalleryItem.Create(Guid.NewGuid(), i, []))
            .ToList();

        var result = item.SetGallery(tooMany, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.TooManyGalleryItems", result.Error.Code);
    }

    [Fact]
    public void SetGallery_WithDuplicateMediaAsset_Fails()
    {
        var item = CreateItem().Value;
        var mediaAssetId = Guid.NewGuid();
        var items = new List<ContentItemGalleryItem>
        {
            ContentItemGalleryItem.Create(mediaAssetId, 0, []),
            ContentItemGalleryItem.Create(mediaAssetId, 1, []),
        };

        var result = item.SetGallery(items, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.DuplicateGalleryMediaAsset", result.Error.Code);
    }

    [Fact]
    public void SetGallery_CalledAgain_ReplacesRatherThanAppends()
    {
        var item = CreateItem().Value;
        item.SetGallery([ContentItemGalleryItem.Create(Guid.NewGuid(), 0, [])], UserId, Now);
        var newItem = ContentItemGalleryItem.Create(Guid.NewGuid(), 0, []);

        item.SetGallery([newItem], UserId, Now);

        Assert.Equal([newItem.MediaAssetId], item.GalleryItems.Select(g => g.MediaAssetId));
    }

    [Fact]
    public void SetVideos_WithValidIds_ReplacesWholeList()
    {
        var item = CreateItem().Value;
        var videoId = Guid.NewGuid();

        var result = item.SetVideos([videoId], UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal([videoId], item.VideoIds);
    }

    [Fact]
    public void SetVideos_ExceedingMax_Fails()
    {
        var item = CreateItem().Value;
        var tooMany = Enumerable.Range(0, ContentItem.MaxVideos + 1).Select(_ => Guid.NewGuid()).ToList();

        var result = item.SetVideos(tooMany, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.TooManyVideos", result.Error.Code);
    }

    [Fact]
    public void SetVideos_WithDuplicateId_Fails()
    {
        var item = CreateItem().Value;
        var videoId = Guid.NewGuid();

        var result = item.SetVideos([videoId, videoId], UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.DuplicateVideo", result.Error.Code);
    }

    [Fact]
    public void SetAttachments_WithValidItems_ReplacesWholeList()
    {
        var item = CreateItem().Value;
        var attachment = ContentItemAttachment.Create(Guid.NewGuid(), 0, []);

        var result = item.SetAttachments([attachment], UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(item.Attachments);
    }

    [Fact]
    public void SetAttachments_ExceedingMax_Fails()
    {
        var item = CreateItem().Value;
        var tooMany = Enumerable.Range(0, ContentItem.MaxAttachments + 1)
            .Select(i => ContentItemAttachment.Create(Guid.NewGuid(), i, []))
            .ToList();

        var result = item.SetAttachments(tooMany, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.TooManyAttachments", result.Error.Code);
    }

    [Fact]
    public void SetAttachments_WithDuplicateMediaAsset_Fails()
    {
        var item = CreateItem().Value;
        var mediaAssetId = Guid.NewGuid();
        var items = new List<ContentItemAttachment>
        {
            ContentItemAttachment.Create(mediaAssetId, 0, []),
            ContentItemAttachment.Create(mediaAssetId, 1, []),
        };

        var result = item.SetAttachments(items, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.DuplicateAttachmentMediaAsset", result.Error.Code);
    }

    private static void MoveTo(ContentItem item, ContentItemStatus status)
    {
        switch (status)
        {
            case ContentItemStatus.Draft:
                return;
            case ContentItemStatus.Published:
                item.Publish(null, null, true, true, Tr, UserId, Now);
                return;
            case ContentItemStatus.Unpublished:
                item.Publish(null, null, true, true, Tr, UserId, Now);
                item.Unpublish(0, UserId, Now);
                return;
            case ContentItemStatus.Archived:
                item.Publish(null, null, true, true, Tr, UserId, Now);
                item.Archive(0, UserId, Now);
                return;
        }
    }
}
