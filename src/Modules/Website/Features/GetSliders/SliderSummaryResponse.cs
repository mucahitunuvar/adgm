namespace GenclikMerkezi.Modules.Website.Features.GetSliders;

public sealed record SliderSummaryResponse(Guid Id, string Key, string Name, int SlideCount, byte[] RowVersion);
