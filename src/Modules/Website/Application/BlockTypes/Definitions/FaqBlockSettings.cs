using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record FaqBlockSettings(string ContentTypeKey, Guid? CategoryId, [Range(1, 30)] int Count);
