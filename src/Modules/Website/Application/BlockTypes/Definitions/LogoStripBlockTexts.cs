using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record LogoStripBlockTexts([MaxLength(150)] string? Title);
