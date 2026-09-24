using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13: one social media link shown in the footer/contact page. Owned by SiteSettings and
// always replaced as a whole list (SiteSettings.ReplaceSocialLinks) - Platform has no uniqueness
// constraint, so there is no natural key to upsert against.
public sealed class SocialLink : Entity
{
    public const int MaxPlatformLength = 50;
    public const int MaxUrlLength = 500;

    public string Platform { get; private set; } = string.Empty;

    public string Url { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    private SocialLink(Guid id, string platform, string url, int sortOrder)
        : base(id)
    {
        Platform = platform;
        Url = url;
        SortOrder = sortOrder;
    }

    private SocialLink()
    {
    }

    public static SocialLink Create(string platform, string url, int sortOrder) =>
        new(Guid.NewGuid(), platform.Trim(), url.Trim(), sortOrder);
}
