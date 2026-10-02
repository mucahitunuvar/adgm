using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record CtaBlockSettings([MinLength(1), MaxLength(3)] IReadOnlyList<CtaButtonSettings> Buttons);
