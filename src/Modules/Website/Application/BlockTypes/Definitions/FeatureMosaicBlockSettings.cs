using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record FeatureMosaicBlockSettings([MinLength(1), MaxLength(5)] IReadOnlyList<FeatureMosaicItemSettings> Items);
