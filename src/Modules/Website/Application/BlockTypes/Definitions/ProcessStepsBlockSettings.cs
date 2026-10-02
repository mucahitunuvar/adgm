using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record ProcessStepsBlockSettings([Range(2, 8)] int StepCount);
