using System.ComponentModel.DataAnnotations;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed record QuickLinkItemSettings([MaxLength(50)] string IconKey, LinkTargetDto Link);
