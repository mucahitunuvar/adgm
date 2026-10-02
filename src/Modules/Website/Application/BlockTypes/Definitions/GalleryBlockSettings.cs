using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record GalleryBlockSettings([MinLength(1), MaxLength(30)] IReadOnlyList<Guid> MediaIds);
