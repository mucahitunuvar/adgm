namespace GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;

public sealed record UpsertEventScheduleTranslationInput(
    string LanguageCode,
    string? VenueName,
    string? VenueAddress,
    string? FeeInfo,
    string? Instructors,
    string? ProgramFlow,
    string? AccessibilityNote);
