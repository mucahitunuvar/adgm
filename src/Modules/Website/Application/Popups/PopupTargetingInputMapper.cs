using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Parsing;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Popups;

// Shared by CreatePopupCommandHandler/UpdatePopupCommandHandler - both need the identical
// dto-to-domain-VO conversion for the Targeting field.
internal static class PopupTargetingInputMapper
{
    public static Result<PopupTargeting> ToTargeting(PopupTargetingInput input)
    {
        if (!EnumParsing.TryParseDefined<PopupTargetingKind>(input.Kind, out var kind))
        {
            return Result.Failure<PopupTargeting>(Error.Validation("Popup.TargetingKindInvalid", $"Unknown targeting kind '{input.Kind}'."));
        }

        return kind switch
        {
            PopupTargetingKind.AllPages => PopupTargeting.CreateAllPages(),
            PopupTargetingKind.HomeOnly => PopupTargeting.CreateHomeOnly(),
            PopupTargetingKind.Contents => PopupTargeting.CreateForContents(input.ContentItemIds),
            PopupTargetingKind.Paths => PopupTargeting.CreateForPaths(input.Paths),
            _ => Result.Failure<PopupTargeting>(Error.Validation("Popup.TargetingKindInvalid", $"Unknown targeting kind '{input.Kind}'.")),
        };
    }
}
