namespace GenclikMerkezi.Modules.Employer.Application.Abstractions;

// Görev 2 (Employer public jobs master prompt): Company.AboutHtml is firma-submitted free-form HTML,
// never sanitized at write time (only max-length validated) - returning it as-is from an anonymous
// public endpoint would be a direct XSS vector. Website's own IHtmlContentSanitizer cannot be reused
// here (Employer must not depend on Website - master prompt §1), so this is a separate, deliberately
// smaller, Employer-owned whitelist (no images/iframes - AboutHtml has no media-embedding feature at
// all, unlike Website's rich content blocks).
public interface IHtmlContentSanitizer
{
    string Sanitize(string html);
}
