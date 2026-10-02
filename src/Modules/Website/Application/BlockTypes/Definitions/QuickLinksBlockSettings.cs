using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record QuickLinksBlockSettings([MinLength(1), MaxLength(8)] IReadOnlyList<QuickLinkItemSettings> Items);
