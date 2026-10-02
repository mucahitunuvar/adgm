using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record ImageTextBlockTexts([MaxLength(150)] string Title, string Body, [MaxLength(50)] string? LinkLabel);
