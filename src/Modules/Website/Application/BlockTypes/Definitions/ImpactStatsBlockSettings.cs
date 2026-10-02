using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

// §4.2: an empty list means "all active metrics" - MetricIds is allowed to be empty, only bounded above.
public sealed record ImpactStatsBlockSettings([MaxLength(8)] IReadOnlyList<Guid> MetricIds);
