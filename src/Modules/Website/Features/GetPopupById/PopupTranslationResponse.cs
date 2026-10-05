namespace GenclikMerkezi.Modules.Website.Features.GetPopupById;

public sealed record PopupTranslationResponse(string LanguageCode, string? Title, string Body, string? ButtonLabel);
