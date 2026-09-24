namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §16 (Görev 3): whitelist-based HTML sanitization for rich text content (Faz 1's
// ContentItem body, and anywhere else free-form HTML from an admin editor needs to be stored).
// Never trusts the frontend editor - see Infrastructure.Sanitization.HtmlSanitizerContentSanitizer
// for the actual whitelist, defined in exactly one place.
public interface IHtmlContentSanitizer
{
    string Sanitize(string html);
}
