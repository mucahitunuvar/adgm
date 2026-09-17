using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// Platform is free text on purpose (Candidate.md: Facebook/GitHub/Instagram/YouTube/Behance/Dribbble
// plus "bir cv için gerekli olan diğer sosyal medya hesapları") - not a lookup/enum, since the list
// is explicitly open-ended.
public sealed class SocialMediaLink : Entity
{
    public Guid CandidateCvId { get; private set; }

    public string Platform { get; private set; }

    public string Url { get; private set; }

    private SocialMediaLink(Guid id, Guid candidateCvId, string platform, string url)
        : base(id)
    {
        CandidateCvId = candidateCvId;
        Platform = platform;
        Url = url;
    }

    internal static SocialMediaLink Create(Guid candidateCvId, string platform, string url) =>
        new(Guid.NewGuid(), candidateCvId, platform, url);
}
