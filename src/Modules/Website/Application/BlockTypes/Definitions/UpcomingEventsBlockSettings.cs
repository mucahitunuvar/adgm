using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record UpcomingEventsBlockSettings([MinLength(1)] IReadOnlyList<string> ContentTypeKeys, [Range(1, 12)] int Count);
