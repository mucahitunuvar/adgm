namespace GenclikMerkezi.Modules.Website.Application.LinkTargets;

public sealed record LinkTargetResolution(string? Href, LinkTargetUnresolvedReason UnresolvedReason)
{
    public bool IsResolved => Href is not null;

    public static LinkTargetResolution Resolved(string href) => new(href, LinkTargetUnresolvedReason.None);

    public static LinkTargetResolution Unresolved(LinkTargetUnresolvedReason reason) => new(null, reason);
}
