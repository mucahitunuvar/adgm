namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Faz 5 Görev 6): pure assembly of schema.org JSON-LD objects - no repository access,
// every input already resolved by the caller (title/description already in the requested language,
// URLs already absolute). Each object is a plain Dictionary<string, object?> rather than a typed
// record with [JsonPropertyName("@type")] attributes: Domain must not depend on a serialization
// framework (AGENTS.md §7), and schema.org's "@type"/"@context" keys are not valid C# identifiers
// anyway. "Boş alanlar yazılmaz" (§1) is implemented throughout by simply never adding the key,
// never by adding it with a null/empty value - callers (GetPublicContentById, GetPublicContents,
// GetPublicSite) serialize the returned dictionaries as-is; System.Text.Json writes their keys
// verbatim, unaffected by any camelCase property-naming policy configured for typed responses.
public static class StructuredDataBuilder
{
    private const string Context = "https://schema.org";

    // Always added to the public detail response, regardless of ContentType.SchemaKind.
    public static IReadOnlyDictionary<string, object?> BuildBreadcrumbList(IReadOnlyList<(string Name, string AbsoluteUrl)> items)
    {
        var itemListElement = items
            .Select((item, index) => new Dictionary<string, object?>
            {
                ["@type"] = "ListItem",
                ["position"] = index + 1,
                ["name"] = item.Name,
                ["item"] = item.AbsoluteUrl,
            })
            .ToList();

        return new Dictionary<string, object?>
        {
            ["@context"] = Context,
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = itemListElement,
        };
    }

    // schemaKind must be Article or NewsArticle - callers only call this for those two ContentSchemaKind
    // values (§1 "NewsArticle/Article"). "author verisi yoksa yazılmaz": this project's ContentItem has
    // no author field, so "author" is never added - there is no parameter for it.
    public static IReadOnlyDictionary<string, object?> BuildArticle(
        ContentSchemaKind schemaKind,
        string headline,
        string description,
        string? imageAbsoluteUrl,
        DateTime datePublishedUtc,
        DateTime? dateModifiedUtc,
        string pageAbsoluteUrl,
        string publisherName,
        string? publisherLogoAbsoluteUrl)
    {
        var result = new Dictionary<string, object?>
        {
            ["@context"] = Context,
            ["@type"] = schemaKind.ToString(),
            ["headline"] = headline,
            ["description"] = description,
            ["datePublished"] = ToIso8601(datePublishedUtc),
            ["mainEntityOfPage"] = new Dictionary<string, object?> { ["@type"] = "WebPage", ["@id"] = pageAbsoluteUrl },
            ["publisher"] = BuildPublisher(publisherName, publisherLogoAbsoluteUrl),
        };

        if (!string.IsNullOrWhiteSpace(imageAbsoluteUrl))
        {
            result["image"] = imageAbsoluteUrl;
        }

        if (dateModifiedUtc is { } dateModified)
        {
            result["dateModified"] = ToIso8601(dateModified);
        }

        return result;
    }

    // ADR-024 §15/§1 (Faz 4 §11.3's rule carried over): OnlineLink never appears here under any
    // condition - only the event's own public page URL feeds VirtualLocation when Online. "yüz yüze/
    // hibritte Place, salt online'da VirtualLocation" - Hybrid gets Place, exactly like InPerson, never
    // both.
    public static IReadOnlyDictionary<string, object?> BuildEvent(
        string name,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        bool isCancelled,
        EventFormat format,
        string venueName,
        string venueAddress,
        string description,
        string? imageAbsoluteUrl,
        string eventPageAbsoluteUrl)
    {
        var result = new Dictionary<string, object?>
        {
            ["@context"] = Context,
            ["@type"] = "Event",
            ["name"] = name,
            ["startDate"] = ToIso8601(startsAtUtc),
            ["endDate"] = ToIso8601(endsAtUtc),
            ["eventStatus"] = isCancelled ? "https://schema.org/EventCancelled" : "https://schema.org/EventScheduled",
            ["eventAttendanceMode"] = BuildAttendanceMode(format),
            ["location"] = format == EventFormat.Online
                ? new Dictionary<string, object?> { ["@type"] = "VirtualLocation", ["url"] = eventPageAbsoluteUrl }
                : new Dictionary<string, object?> { ["@type"] = "Place", ["name"] = venueName, ["address"] = venueAddress },
            ["description"] = description,
            ["url"] = eventPageAbsoluteUrl,
        };

        if (!string.IsNullOrWhiteSpace(imageAbsoluteUrl))
        {
            result["image"] = imageAbsoluteUrl;
        }

        return result;
    }

    // Added to the public list response only when ContentType.SchemaKind is FaqPage (§1 "yalnızca
    // FaqPage türünde"). answerHtml is converted to plain text via SearchTextBuilder, same tag/entity
    // stripping Görev 1 already uses for search indexing.
    public static IReadOnlyDictionary<string, object?> BuildFaqPage(IReadOnlyList<(string Question, string AnswerHtml)> items)
    {
        var mainEntity = items
            .Select(item => new Dictionary<string, object?>
            {
                ["@type"] = "Question",
                ["name"] = item.Question,
                ["acceptedAnswer"] = new Dictionary<string, object?>
                {
                    ["@type"] = "Answer",
                    ["text"] = SearchTextBuilder.StripHtml(item.AnswerHtml),
                },
            })
            .ToList();

        return new Dictionary<string, object?>
        {
            ["@context"] = Context,
            ["@type"] = "FAQPage",
            ["mainEntity"] = mainEntity,
        };
    }

    // GetPublicSite's "organization" field - always present (not part of the detail/list jsonLd
    // array), "boş alanlar yazılmaz" applies to every field below name/url.
    public static IReadOnlyDictionary<string, object?> BuildOrganization(
        string name,
        string absoluteUrl,
        string? logoAbsoluteUrl,
        IReadOnlyList<string> sameAsAbsoluteUrls,
        string? telephone,
        string? email,
        string? address)
    {
        var result = new Dictionary<string, object?>
        {
            ["@context"] = Context,
            ["@type"] = "Organization",
            ["name"] = name,
            ["url"] = absoluteUrl,
        };

        if (!string.IsNullOrWhiteSpace(logoAbsoluteUrl))
        {
            result["logo"] = logoAbsoluteUrl;
        }

        if (sameAsAbsoluteUrls.Count > 0)
        {
            result["sameAs"] = sameAsAbsoluteUrls;
        }

        var hasTelephone = !string.IsNullOrWhiteSpace(telephone);
        var hasEmail = !string.IsNullOrWhiteSpace(email);
        if (hasTelephone || hasEmail)
        {
            var contactPoint = new Dictionary<string, object?> { ["@type"] = "ContactPoint" };
            if (hasTelephone)
            {
                contactPoint["telephone"] = telephone;
            }

            if (hasEmail)
            {
                contactPoint["email"] = email;
            }

            result["contactPoint"] = contactPoint;
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            result["address"] = address;
        }

        return result;
    }

    private static Dictionary<string, object?> BuildPublisher(string name, string? logoAbsoluteUrl)
    {
        var publisher = new Dictionary<string, object?> { ["@type"] = "Organization", ["name"] = name };
        if (!string.IsNullOrWhiteSpace(logoAbsoluteUrl))
        {
            publisher["logo"] = new Dictionary<string, object?> { ["@type"] = "ImageObject", ["url"] = logoAbsoluteUrl };
        }

        return publisher;
    }

    private static string BuildAttendanceMode(EventFormat format) => format switch
    {
        EventFormat.Online => "https://schema.org/OnlineEventAttendanceMode",
        EventFormat.Hybrid => "https://schema.org/MixedEventAttendanceMode",
        _ => "https://schema.org/OfflineEventAttendanceMode",
    };

    private static string ToIso8601(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ssZ");
}
