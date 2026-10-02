using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record ProcessStepTexts([MaxLength(150)] string Title, [MaxLength(400)] string Text);
