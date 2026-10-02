using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record CtaBlockTexts(
    [MaxLength(100)] string? Eyebrow, [MaxLength(150)] string Title, [MinLength(1), MaxLength(3)] IReadOnlyList<CtaButtonTexts> Buttons);
