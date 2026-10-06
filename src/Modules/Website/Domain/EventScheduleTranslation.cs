using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.1 (Faz 4 Görev 1): every language-dependent field on EventSchedule, same owned-entity
// shape as ContentItemTranslation - factory + internal Update, no public setters. All fields are
// optional free text (informational), so normalization only trims and enforces a max length; none of
// them carries the "non-empty" rule ContentItemTranslation.Title/Summary do. ProgramFlow arrives
// already sanitized by the command handler (IHtmlContentSanitizer), the same contract
// ContentItemTranslation.Body uses - this type has no dependency on the sanitizer itself.
public sealed class EventScheduleTranslation : Entity
{
    public const int MaxVenueNameLength = 200;
    public const int MaxVenueAddressLength = 500;
    public const int MaxFeeInfoLength = 2000;
    public const int MaxInstructorsLength = 2000;
    public const int MaxAccessibilityNoteLength = 2000;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string VenueName { get; private set; } = string.Empty;

    public string VenueAddress { get; private set; } = string.Empty;

    public string FeeInfo { get; private set; } = string.Empty;

    public string Instructors { get; private set; } = string.Empty;

    public string ProgramFlow { get; private set; } = string.Empty;

    public string AccessibilityNote { get; private set; } = string.Empty;

    private EventScheduleTranslation(
        Guid id,
        LanguageCode languageCode,
        string venueName,
        string venueAddress,
        string feeInfo,
        string instructors,
        string programFlow,
        string accessibilityNote)
        : base(id)
    {
        LanguageCode = languageCode;
        VenueName = venueName;
        VenueAddress = venueAddress;
        FeeInfo = feeInfo;
        Instructors = instructors;
        ProgramFlow = programFlow;
        AccessibilityNote = accessibilityNote;
    }

    private EventScheduleTranslation()
    {
    }

    internal static Result<EventScheduleTranslation> Create(
        LanguageCode languageCode,
        string? venueName,
        string? venueAddress,
        string? feeInfo,
        string? instructors,
        string? programFlow,
        string? accessibilityNote)
    {
        var fieldsResult = NormalizeFields(venueName, venueAddress, feeInfo, instructors, programFlow, accessibilityNote);
        if (fieldsResult.IsFailure)
        {
            return Result.Failure<EventScheduleTranslation>(fieldsResult.Error);
        }

        var (normalizedVenueName, normalizedVenueAddress, normalizedFeeInfo, normalizedInstructors, normalizedProgramFlow, normalizedAccessibilityNote) =
            fieldsResult.Value;

        return Result.Success(new EventScheduleTranslation(
            Guid.NewGuid(), languageCode, normalizedVenueName, normalizedVenueAddress, normalizedFeeInfo, normalizedInstructors,
            normalizedProgramFlow, normalizedAccessibilityNote));
    }

    internal Result Update(
        string? venueName, string? venueAddress, string? feeInfo, string? instructors, string? programFlow, string? accessibilityNote)
    {
        var fieldsResult = NormalizeFields(venueName, venueAddress, feeInfo, instructors, programFlow, accessibilityNote);
        if (fieldsResult.IsFailure)
        {
            return fieldsResult;
        }

        (VenueName, VenueAddress, FeeInfo, Instructors, ProgramFlow, AccessibilityNote) = fieldsResult.Value;

        return Result.Success();
    }

    private static Result<(string VenueName, string VenueAddress, string FeeInfo, string Instructors, string ProgramFlow, string AccessibilityNote)>
        NormalizeFields(string? venueName, string? venueAddress, string? feeInfo, string? instructors, string? programFlow, string? accessibilityNote)
    {
        var venueNameResult = NormalizeOptionalText(venueName, MaxVenueNameLength, "VenueName");
        if (venueNameResult.IsFailure)
        {
            return Result.Failure<(string, string, string, string, string, string)>(venueNameResult.Error);
        }

        var venueAddressResult = NormalizeOptionalText(venueAddress, MaxVenueAddressLength, "VenueAddress");
        if (venueAddressResult.IsFailure)
        {
            return Result.Failure<(string, string, string, string, string, string)>(venueAddressResult.Error);
        }

        var feeInfoResult = NormalizeOptionalText(feeInfo, MaxFeeInfoLength, "FeeInfo");
        if (feeInfoResult.IsFailure)
        {
            return Result.Failure<(string, string, string, string, string, string)>(feeInfoResult.Error);
        }

        var instructorsResult = NormalizeOptionalText(instructors, MaxInstructorsLength, "Instructors");
        if (instructorsResult.IsFailure)
        {
            return Result.Failure<(string, string, string, string, string, string)>(instructorsResult.Error);
        }

        // ProgramFlow already went through IHtmlContentSanitizer in the command handler - only the
        // length guard applies here, no further trimming that could disturb sanitized markup.
        var programFlowValue = programFlow ?? string.Empty;

        var accessibilityNoteResult = NormalizeOptionalText(accessibilityNote, MaxAccessibilityNoteLength, "AccessibilityNote");
        if (accessibilityNoteResult.IsFailure)
        {
            return Result.Failure<(string, string, string, string, string, string)>(accessibilityNoteResult.Error);
        }

        return Result.Success((
            venueNameResult.Value, venueAddressResult.Value, feeInfoResult.Value, instructorsResult.Value, programFlowValue,
            accessibilityNoteResult.Value));
    }

    private static Result<string> NormalizeOptionalText(string? value, int maxLength, string fieldName)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length > maxLength)
        {
            return Result.Failure<string>(Error.Validation(
                $"EventScheduleTranslation.{fieldName}TooLong", $"{fieldName} must be at most {maxLength} characters."));
        }

        return Result.Success(trimmed);
    }
}
