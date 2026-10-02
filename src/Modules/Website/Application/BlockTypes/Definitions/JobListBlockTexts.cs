using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record JobListBlockTexts([MaxLength(150)] string Title, [MaxLength(50)] string? MoreLabel);
