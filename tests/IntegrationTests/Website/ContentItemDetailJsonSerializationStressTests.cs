using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using Microsoft.AspNetCore.Hosting;

namespace GenclikMerkezi.IntegrationTests.Website;

// GÖREV 2(c): disambiguates the two possible explanations for the full-suite-only
// PublicContentListAndDetailFlowTests failures (System.Text.Json.JsonException: "$.status" came back
// as a JSON Number, even though ContentItemDetailResponse.Status is a C# string and
// GetContentItemByIdQueryHandler always writes contentItem.Status.ToString()):
//
// (A) the SERVER actually wrote a bare numeric token for "status" on the wire under concurrent load
//     (a real production risk - every caller of this endpoint uses ReadFromJsonAsync<T>() the same way), or
// (B) the server always wrote the correct quoted string, and the exception only comes from the TEST
//     PROCESS's own client-side deserialization: every *FlowTests.cs file in this directory calls
//     response.Content.ReadFromJsonAsync<ContentItemDetailResponse>() with no explicit
//     JsonSerializerOptions, which means every one of them shares the same process-wide-static default
//     options instance (System.Net.Http.Json.HttpContentJsonExtensions' internal default). That static
//     instance is the ONE piece of state genuinely shared across the ~25 otherwise fully-isolated
//     CustomWebApplicationFactory instances (each its own host/DbContext/connection) that make up the
//     full suite - unlike production, which only ever has ONE host and never calls this client-side
//     helper on its own responses at all.
//
// This test uses a SINGLE host (matching production's deployment shape) and fires thousands of
// concurrent requests at the same endpoint. It captures the RAW response body string FIRST - which
// tells us definitively whether the server ever wrote a bad wire format (A) - and only afterwards
// attempts the same ReadFromJsonAsync<T>() every other test uses, to see whether (B) reproduces even
// against a single host once nothing else in the process is racing to resolve the same type for the
// first time. If this test stays green while the full suite's deserialization failures persist, that
// is itself the proof the full-suite failures are (B), not a production-reachable bug.
public class ContentItemDetailJsonSerializationStressTests : IClassFixture<ContentItemDetailJsonSerializationStressTests.SingleHostHighThroughputFactory>
{
    // Raises "authenticated" (this endpoint's rate-limit policy) above the default Testing-environment
    // limit (1000/min, Program.cs) so a multi-thousand-request burst in a single test doesn't itself
    // get 429'd - the rate limiter is not what this test is investigating.
    public sealed class SingleHostHighThroughputFactory : CustomWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseSetting("RateLimiting:AuthenticatedPermitLimit", "200000");
        }
    }

    private const int ConcurrentRequestCount = 3000;

    private readonly SingleHostHighThroughputFactory _factory;
    private readonly HttpClient _client;

    public ContentItemDetailJsonSerializationStressTests(SingleHostHighThroughputFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var email = $"admin-{Guid.NewGuid():N}@example.com";
        const string password = "AdminSifre123";
        await _factory.SeedAdminUserAsync(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.AccessToken;
    }

    private HttpRequestMessage Authorized(HttpMethod method, string path, string accessToken, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    [Fact]
    public async Task GetContentItemById_UnderThousandsOfConcurrentRequestsOnOneHost_NeverWritesNumericStatusOnTheWire()
    {
        var accessToken = await LoginAsAdminAsync();

        var typesResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await typesResponse.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        var newsTypeId = types!.Single(t => t.Key == "news").Id;

        var createRequest = new CreateContentItemRequest(
            newsTypeId, null, 1, false, null, null, "Stress Test Content", null, null, null,
            new CreateContentItemSeoInput(null, null, null, null, null, null, null, false));
        var createResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, createRequest));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        var contentItemId = created!.Id;

        var getPath = $"/api/v1/admin/website/contents/{contentItemId}";

        // Every task uses the SAME HttpClient (thread-safe for concurrent use) against the SAME,
        // already-running single host - no new factory/host is spun up per request.
        var tasks = Enumerable.Range(0, ConcurrentRequestCount).Select(async _ =>
        {
            var response = await _client.SendAsync(Authorized(HttpMethod.Get, getPath, accessToken));
            var rawBody = await response.Content.ReadAsStringAsync();
            return (response.StatusCode, rawBody);
        });

        var results = await Task.WhenAll(tasks);

        var nonOkResults = results.Where(r => r.StatusCode != System.Net.HttpStatusCode.OK).ToList();
        Assert.True(nonOkResults.Count == 0, $"{nonOkResults.Count} of {ConcurrentRequestCount} requests did not return 200 OK.");

        // (A) Checked directly against the raw wire bytes - a quoted string value looks like
        // "status":"Draft", a bare numeric token (the bug this investigates) would look like
        // "status":0. This check does not go through any JsonSerializerOptions at all.
        var numericStatusOccurrences = results.Count(r => System.Text.RegularExpressions.Regex.IsMatch(r.rawBody, "\"status\"\\s*:\\s*\\d"));
        Assert.True(
            numericStatusOccurrences == 0,
            $"{numericStatusOccurrences} of {ConcurrentRequestCount} raw response bodies had a numeric " +
            "\"status\" token on the wire - the server itself is writing the wrong JSON shape under load.");

        // (B) The same ReadFromJsonAsync<T>() call every *FlowTests.cs file in this directory uses,
        // against the SAME already-captured raw bodies, so a failure here with (A) having found zero
        // raw-wire violations would point at the client-side deserialization path itself, not the server.
        var deserializationFailures = new List<Exception>();
        foreach (var (_, rawBody) in results)
        {
            try
            {
                JsonSerializer.Deserialize<ContentItemDetailResponse>(rawBody);
            }
            catch (JsonException ex)
            {
                deserializationFailures.Add(ex);
            }
        }

        Assert.True(
            deserializationFailures.Count == 0,
            $"{deserializationFailures.Count} of {ConcurrentRequestCount} responses failed to deserialize " +
            $"even though their raw wire bytes were well-formed. First: {deserializationFailures.FirstOrDefault()}");
    }
}
