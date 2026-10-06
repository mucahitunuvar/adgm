namespace GenclikMerkezi.Modules.Website.Features.GetEventScheduleByContentItemId;

public sealed record EventScheduleTranslationResponse(
    string LanguageCode,
    string VenueName,
    string VenueAddress,
    string FeeInfo,
    string Instructors,
    string ProgramFlow,
    string AccessibilityNote);
