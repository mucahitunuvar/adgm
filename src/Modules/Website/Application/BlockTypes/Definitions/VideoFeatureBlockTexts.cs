using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

// §4.2: the one block type whose link lives in the per-language Texts record rather than Settings.
public sealed record VideoFeatureBlockTexts(
    [MaxLength(100)] string? Eyebrow,
    [MaxLength(150)] string Title,
    [MaxLength(400)] string? Text,
    LinkTargetDto? Link,
    [MaxLength(50)] string? LinkLabel);
