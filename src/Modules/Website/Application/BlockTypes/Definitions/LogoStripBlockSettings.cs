using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record LogoStripBlockSettings([Range(1, 30)] int MaxItems);
