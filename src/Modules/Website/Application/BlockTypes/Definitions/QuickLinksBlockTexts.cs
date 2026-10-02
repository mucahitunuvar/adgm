using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record QuickLinksBlockTexts([MinLength(1), MaxLength(8)] IReadOnlyList<QuickLinkItemTexts> Items);
