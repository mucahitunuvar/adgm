namespace GenclikMerkezi.Modules.Website.Features.SetContentItemVideos;

public sealed record SetContentItemVideosRequest(byte[] RowVersion, IReadOnlyList<Guid> VideoIds);
