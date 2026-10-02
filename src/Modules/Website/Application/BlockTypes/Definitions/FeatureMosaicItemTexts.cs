using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record FeatureMosaicItemTexts([MaxLength(100)] string? Eyebrow, [MaxLength(150)] string Title);
