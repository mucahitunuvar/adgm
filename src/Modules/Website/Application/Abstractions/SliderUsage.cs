namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// Mirrors VideoUsage's shape for the same reason (§2 "kontrol portu kurulur: ISliderUsageChecker"):
// one place a Slider is referenced from. Only a PageLayout's hero-slider block will ever reference a
// Slider (Görev 4), so there is no provider/composite fan-out here either.
public sealed record SliderUsage(string SourceKey, Guid SourceId, string Description, string? Url);
