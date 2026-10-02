using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §8.1 / Faz 2 Görev 4 master prompt §4.3: a PageLayout keeps two full block lists, draft and
// published, exactly like SiteSettings-style "edit then publish" flows elsewhere in the module - edits
// land in the draft, are previewed, then Publish copies the draft into the published list in one call.
// RowVersion is the same application-managed optimistic-concurrency token every other Website
// aggregate uses (regenerated on every mutation, compared by the command handler - not a
// database-generated rowversion column, since the module runs on both SqlServer and Sqlite, ADR-012).
//
// Whether a block type is known, allowed for this TargetKind, and whether its settings/texts/
// references are valid is the block type registry's job (Application layer) - this aggregate only
// enforces what it can check without that registry: block count and duplicate block ids.
public sealed class PageLayout : AggregateRoot
{
    public const int MaxBlocks = 30;

    private readonly List<LayoutBlock> _draftBlocks = [];
    private readonly List<LayoutBlock> _publishedBlocks = [];

    public PageLayoutTargetKind TargetKind { get; private set; }

    public Guid? ContentItemId { get; private set; }

    public IReadOnlyList<LayoutBlock> DraftBlocks => _draftBlocks.AsReadOnly();

    public IReadOnlyList<LayoutBlock> PublishedBlocks => _publishedBlocks.AsReadOnly();

    public bool HasUnpublishedChanges { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public Guid? PublishedByUserId { get; private set; }

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private PageLayout(Guid id, PageLayoutTargetKind targetKind, Guid? contentItemId, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        TargetKind = targetKind;
        ContentItemId = contentItemId;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private PageLayout()
    {
    }

    // Only ever seeded by migration (PageLayoutConfiguration.HasData), the same "exactly one row,
    // never created/deleted through an admin action" shape the three Menu rows use - exposed as a
    // factory anyway so domain tests can build one without reaching into EF seed data.
    public static PageLayout CreateHome(Guid id, Guid createdByUserId, DateTime createdAtUtc) =>
        new(id, PageLayoutTargetKind.Home, null, createdByUserId, createdAtUtc);

    // §4.3 "İçerik düzeni ilk kayıtta oluşur": created by ReplaceContentDraftBlocksCommandHandler the
    // first time an admin saves a draft for a ContentItem - never through a separate create endpoint.
    public static Result<PageLayout> CreateForContent(Guid contentItemId, Guid createdByUserId, DateTime createdAtUtc)
    {
        if (contentItemId == Guid.Empty)
        {
            return Result.Failure<PageLayout>(Error.Validation("PageLayout.ContentItemIdRequired", "A content item id is required."));
        }

        return Result.Success(new PageLayout(Guid.NewGuid(), PageLayoutTargetKind.Content, contentItemId, createdByUserId, createdAtUtc));
    }

    public Result ReplaceDraftBlocks(IReadOnlyList<LayoutBlock> blocks, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (blocks.Count > MaxBlocks)
        {
            return Result.Failure(Error.Validation("PageLayout.TooManyBlocks", $"A page layout can have at most {MaxBlocks} blocks."));
        }

        var duplicateId = blocks.GroupBy(b => b.Id).FirstOrDefault(g => g.Count() > 1);
        if (duplicateId is not null)
        {
            return Result.Failure(Error.Validation("PageLayout.DuplicateBlockId", $"Block id '{duplicateId.Key}' appears more than once."));
        }

        _draftBlocks.Clear();
        _draftBlocks.AddRange(blocks);
        HasUnpublishedChanges = true;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // §4.3 "POST .../publish - bu anda referans doğrulaması tekrar yapılır": the re-validation itself
    // is the Application layer's job (it alone can reach the block type registry and other
    // repositories) - by the time this runs, the caller has already confirmed the draft is valid.
    public Result Publish(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        _publishedBlocks.Clear();
        _publishedBlocks.AddRange(_draftBlocks.Select(b => b.Clone()));
        PublishedAtUtc = updatedAtUtc;
        PublishedByUserId = updatedByUserId;
        HasUnpublishedChanges = false;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result DiscardDraft(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        _draftBlocks.Clear();
        _draftBlocks.AddRange(_publishedBlocks.Select(b => b.Clone()));
        HasUnpublishedChanges = false;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
