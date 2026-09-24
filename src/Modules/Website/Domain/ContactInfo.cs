using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13: public contact details shown across the site (footer, contact page). Every field is
// optional (a fresh installation may not have all channels configured yet); only Email gets a format
// check since it is the one field where garbage input would actually break something downstream.
public sealed partial class ContactInfo : ValueObject
{
    public const int MaxAddressLength = 500;
    public const int MaxShortFieldLength = 100;
    public const int MaxUrlLength = 500;

    public string Address { get; } = string.Empty;

    public string Phone { get; } = string.Empty;

    public string Email { get; } = string.Empty;

    public string WhatsApp { get; } = string.Empty;

    public string MapEmbedUrl { get; } = string.Empty;

    private ContactInfo(string address, string phone, string email, string whatsApp, string mapEmbedUrl)
    {
        Address = address;
        Phone = phone;
        Email = email;
        WhatsApp = whatsApp;
        MapEmbedUrl = mapEmbedUrl;
    }

    public static Result<ContactInfo> Create(string? address, string? phone, string? email, string? whatsApp, string? mapEmbedUrl)
    {
        var normalizedAddress = (address ?? string.Empty).Trim();
        var normalizedPhone = (phone ?? string.Empty).Trim();
        var normalizedEmail = (email ?? string.Empty).Trim();
        var normalizedWhatsApp = (whatsApp ?? string.Empty).Trim();
        var normalizedMapEmbedUrl = (mapEmbedUrl ?? string.Empty).Trim();

        if (normalizedAddress.Length > MaxAddressLength)
        {
            return Result.Failure<ContactInfo>(Error.Validation(
                "ContactInfo.AddressTooLong", $"Address must be at most {MaxAddressLength} characters."));
        }

        if (normalizedPhone.Length > MaxShortFieldLength)
        {
            return Result.Failure<ContactInfo>(Error.Validation(
                "ContactInfo.PhoneTooLong", $"Phone must be at most {MaxShortFieldLength} characters."));
        }

        if (normalizedEmail.Length > 0 && !EmailPattern().IsMatch(normalizedEmail))
        {
            return Result.Failure<ContactInfo>(Error.Validation("ContactInfo.InvalidEmail", "Email format is invalid."));
        }

        if (normalizedWhatsApp.Length > MaxShortFieldLength)
        {
            return Result.Failure<ContactInfo>(Error.Validation(
                "ContactInfo.WhatsAppTooLong", $"WhatsApp must be at most {MaxShortFieldLength} characters."));
        }

        if (normalizedMapEmbedUrl.Length > MaxUrlLength)
        {
            return Result.Failure<ContactInfo>(Error.Validation(
                "ContactInfo.MapEmbedUrlTooLong", $"Map embed URL must be at most {MaxUrlLength} characters."));
        }

        return Result.Success(new ContactInfo(normalizedAddress, normalizedPhone, normalizedEmail, normalizedWhatsApp, normalizedMapEmbedUrl));
    }

    public static ContactInfo CreateEmpty() => new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Address;
        yield return Phone;
        yield return Email;
        yield return WhatsApp;
        yield return MapEmbedUrl;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
