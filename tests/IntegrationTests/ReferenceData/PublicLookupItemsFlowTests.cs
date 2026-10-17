using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.ReferenceData;

// Madde 39 (Faz 3 Koşu 3C): anonim, dar (yalnız id+ad) public reference-data uçları - /public/jobs
// filtre dropdown'larının ihtiyaç duyduğu 5 set.
public class PublicLookupItemsFlowTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("provinces")]
    [InlineData("positions")]
    [InlineData("departments")]
    [InlineData("employment-types")]
    [InlineData("work-location-types")]
    public async Task GetPublicReferenceData_ForEachFilterSet_ReturnsOk(string routeSegment)
    {
        var response = await _client.GetAsync($"/api/v1/public/reference-data/{routeSegment}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Same bug class as GetPublicJobs (confirmed via OptionalQueryParameter's own callers): an empty
    // "page"/"pageSize" used to crash the Guid?/int? binder; now treated as "not provided" (defaults
    // apply), and a malformed one is a 400 ProblemDetails, not a 500.
    [Fact]
    public async Task GetPublicProvinces_WithEmptyPageAndPageSize_UsesDefaults()
    {
        var response = await _client.GetAsync("/api/v1/public/reference-data/provinces?page=&pageSize=");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, json.GetProperty("page").GetInt32());
        Assert.Equal(20, json.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task GetPublicProvinces_WithMalformedPage_ReturnsBadRequestNotServerError()
    {
        var response = await _client.GetAsync("/api/v1/public/reference-data/provinces?page=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Query.InvalidPage", json.GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetPublicProvinces_WithMalformedPageSize_ReturnsBadRequestNotServerError()
    {
        var response = await _client.GetAsync("/api/v1/public/reference-data/provinces?pageSize=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicPositions_ReturnsOnlyIdAndName_ExcludingInactiveAndOtherFields()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ReferenceDataDbContext>();
            dbContext.Positions.Add(Position.Create($"PUB-{Guid.NewGuid():N}"[..12], "Herkese Açık Pozisyon", 1));

            var inactive = Position.Create($"PUB-{Guid.NewGuid():N}"[..12], "Pasif Pozisyon", 2);
            inactive.Deactivate();
            dbContext.Positions.Add(inactive);

            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/v1/public/reference-data/positions?pageSize=100");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = json.GetProperty("items").EnumerateArray().ToList();

        Assert.Contains(items, i => i.GetProperty("name").GetString() == "Herkese Açık Pozisyon");
        Assert.DoesNotContain(items, i => i.GetProperty("name").GetString() == "Pasif Pozisyon");

        var sample = items.First(i => i.GetProperty("name").GetString() == "Herkese Açık Pozisyon");
        string[] forbiddenProperties = ["code", "isActive", "sortOrder"];
        foreach (var property in forbiddenProperties)
        {
            Assert.False(sample.TryGetProperty(property, out _), $"Public lookup item unexpectedly contains '{property}'.");
        }
    }
}
