using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record FeatureMosaicBlockTexts([MinLength(1), MaxLength(5)] IReadOnlyList<FeatureMosaicItemTexts> Items);
