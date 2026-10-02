using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record ProcessStepsBlockTexts([MaxLength(150)] string Title, IReadOnlyList<ProcessStepTexts> Steps);
