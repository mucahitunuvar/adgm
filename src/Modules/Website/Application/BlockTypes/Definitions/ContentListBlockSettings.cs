using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record ContentListBlockSettings(
    string ContentTypeKey, [Range(1, 12)] int Count, bool FeaturedOnly, Guid? CategoryId, string View);
