using System.Text.Json;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.SubmitFormSubmission;

// ADR-024 §12.2 (Faz 3 Görev 4): the field set is dynamic (driven by the target FormDefinition's own
// Fields, not a fixed DTO shape), so this binds the raw HttpRequest/IFormCollection instead of typed
// [FromForm] parameters the way UploadMediaAssetEndpoint can for its small, fixed field set - every
// key besides the reserved guard/meta fields below is treated as a field answer, validated against the
// live FormDefinition by the command handler ("bilinmeyen alan anahtarları reddedilir").
internal static class SubmitFormSubmissionEndpoint
{
    private static readonly HashSet<string> ReservedFormKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "token", "turnstileToken", "website", "lang", "acceptedPrivacyNoticeVersion", "explicitConsents", "contentItemId",
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/public/forms/{key}/submissions",
                async (string key, HttpRequest httpRequest, ISender sender, CancellationToken cancellationToken) =>
                {
                    var form = await httpRequest.ReadFormAsync(cancellationToken);

                    var explicitConsents = ParseExplicitConsents(form["explicitConsents"]);
                    var contentItemId = Guid.TryParse(form["contentItemId"], out var parsedContentItemId)
                        ? parsedContentItemId
                        : (Guid?)null;
                    var acceptedPrivacyNoticeVersion = int.TryParse(form["acceptedPrivacyNoticeVersion"], out var parsedVersion)
                        ? parsedVersion
                        : (int?)null;

                    var answers = new Dictionary<string, IReadOnlyList<string>>();
                    foreach (var fieldName in form.Keys)
                    {
                        if (ReservedFormKeys.Contains(fieldName))
                        {
                            continue;
                        }

                        answers[fieldName] = form[fieldName].Where(v => v is not null).Select(v => v!).ToList();
                    }

                    var files = form.Files
                        .Select(f => new SubmitFormSubmissionFileInput(f.Name, f.OpenReadStream(), f.FileName, f.ContentType, f.Length))
                        .ToList();

                    var remoteIpAddress = httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString();

                    var command = new SubmitFormSubmissionCommand(
                        key, form["token"], form["turnstileToken"], form["website"], form["lang"], acceptedPrivacyNoticeVersion,
                        explicitConsents, contentItemId, answers, files, remoteIpAddress);

                    var result = await sender.Send(command, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-forms")
            .DisableAntiforgery()
            .WithName("SubmitFormSubmission")
            .WithTags("Website");
    }

    private static IReadOnlyList<SubmitFormSubmissionExplicitConsentInput> ParseExplicitConsents(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<SubmitFormSubmissionExplicitConsentInput>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
