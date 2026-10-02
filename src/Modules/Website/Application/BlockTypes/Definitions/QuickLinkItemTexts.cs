using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record QuickLinkItemTexts([MaxLength(100)] string Label, [MaxLength(300)] string? Description);
