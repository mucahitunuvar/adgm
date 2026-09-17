using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.ReferenceData;

// Exercises ADR-016 Decision 3's generic admin CRUD mechanism end-to-end against a real database -
// in particular, whether FluentValidation's open-generic validator registration
// (CreateLookupItemCommandValidator<TLookup>) actually gets resolved by ValidationBehavior for a
// closed generic request like CreateLookupItemCommand<Sector>, which was not obvious enough to
// simply assume.
public class LookupCrudFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LookupCrudFlowTests(CustomWebApplicationFactory factory)
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

    [Fact]
    public async Task GetCountries_IsAnonymousAndContainsTurkiye()
    {
        var response = await _client.GetAsync("/api/v1/reference-data/countries");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<LookupItemSummary>>();
        Assert.NotNull(items);
        Assert.Contains(items!, i => i.Code == "TR" && i.DisplayName == "Türkiye");
    }

    [Fact]
    public async Task GetDistricts_ReturnsAllSeededDistricts()
    {
        var response = await _client.GetAsync("/api/v1/reference-data/districts");
        var items = await response.Content.ReadFromJsonAsync<List<LookupItemSummary>>();

        Assert.Equal(975, items!.Count);
    }

    [Theory]
    [InlineData("provinces", 81)]
    [InlineData("languages", 30)]
    [InlineData("currencies", 3)]
    [InlineData("work-location-types", 3)]
    [InlineData("employment-types", 2)]
    [InlineData("disability-categories", 7)]
    [InlineData("diploma-grading-systems", 2)]
    [InlineData("reference-types", 5)]
    [InlineData("school-categories", 4)]
    [InlineData("tax-offices", 161)]
    [InlineData("skills", 0)]
    public async Task GetLookup_ReturnsExpectedSeededCount(string routeSegment, int expectedCount)
    {
        var response = await _client.GetAsync($"/api/v1/reference-data/{routeSegment}");
        var items = await response.Content.ReadFromJsonAsync<List<LookupItemSummary>>();

        Assert.Equal(expectedCount, items!.Count);
    }

    [Fact]
    public async Task DeactivateTaxOffice_PreservesRowAndProvinceLink()
    {
        var accessToken = await LoginAsAdminAsync();

        var provinces = await (await _client.GetAsync("/api/v1/reference-data/provinces"))
            .Content.ReadFromJsonAsync<List<LookupItemSummary>>();
        var istanbul = provinces!.Single(p => p.Code == "34");

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reference-data/tax-offices")
        {
            Content = JsonContent.Create(new
            {
                code = $"TO-{Guid.NewGuid():N}"[..20],
                displayName = "Referans Bütünlüğü Testi Vergi Dairesi",
                sortOrder = 0,
                provinceId = istanbul.Id,
            }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = created!["id"];

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/reference-data/tax-offices/{id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Soft-delete: the row must still exist (not a real DELETE) so anything already holding
        // this id as a business reference can still resolve it - only IsActive changes.
        var allTaxOffices = await (await _client.GetAsync("/api/v1/reference-data/tax-offices?activeOnly=false"))
            .Content.ReadFromJsonAsync<List<LookupItemSummary>>();
        var deactivated = allTaxOffices!.Single(t => t.Id == id);
        Assert.False(deactivated.IsActive);
        Assert.Equal("Referans Bütünlüğü Testi Vergi Dairesi", deactivated.DisplayName);

        // The parent Province itself must be entirely unaffected by a child TaxOffice's soft-delete.
        var provincesAfter = await (await _client.GetAsync("/api/v1/reference-data/provinces"))
            .Content.ReadFromJsonAsync<List<LookupItemSummary>>();
        Assert.Contains(provincesAfter!, p => p.Id == istanbul.Id && p.IsActive);
    }

    [Fact]
    public async Task SeedType_HasNoMutationEndpoint()
    {
        // The path itself is a real route (GET is mapped there) but no POST is mapped for it, so
        // ASP.NET Core's routing correctly reports 405 (the resource exists, the method doesn't),
        // not 404.
        var response = await _client.PostAsJsonAsync(
            "/api/v1/reference-data/countries", new { code = "XX", displayName = "Test", sortOrder = 0 });

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task CreateSector_ThenGetUpdateDeactivate_FullLifecycle()
    {
        var accessToken = await LoginAsAdminAsync();
        var code = $"SEC-{Guid.NewGuid():N}"[..20];

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reference-data/sectors")
        {
            Content = JsonContent.Create(new { code, displayName = "Test Sektörü", sortOrder = 1 }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = created!["id"];

        var listResponse = await _client.GetAsync("/api/v1/reference-data/sectors");
        var items = await listResponse.Content.ReadFromJsonAsync<List<LookupItemSummary>>();
        Assert.Contains(items!, i => i.Id == id && i.DisplayName == "Test Sektörü");

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/reference-data/sectors/{id}")
        {
            Content = JsonContent.Create(new { displayName = "Güncellenmiş Sektör", sortOrder = 2 }),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var afterUpdate = await (await _client.GetAsync("/api/v1/reference-data/sectors"))
            .Content.ReadFromJsonAsync<List<LookupItemSummary>>();
        Assert.Contains(afterUpdate!, i => i.Id == id && i.DisplayName == "Güncellenmiş Sektör");

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/reference-data/sectors/{id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var activeOnly = await (await _client.GetAsync("/api/v1/reference-data/sectors?activeOnly=true"))
            .Content.ReadFromJsonAsync<List<LookupItemSummary>>();
        Assert.DoesNotContain(activeOnly!, i => i.Id == id);

        var includingInactive = await (await _client.GetAsync("/api/v1/reference-data/sectors?activeOnly=false"))
            .Content.ReadFromJsonAsync<List<LookupItemSummary>>();
        Assert.Contains(includingInactive!, i => i.Id == id && !i.IsActive);
    }

    [Fact]
    public async Task CreateSector_WithEmptyCode_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reference-data/sectors")
        {
            Content = JsonContent.Create(new { code = "", displayName = "Test", sortOrder = 0 }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSector_WithDuplicateCode_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var code = $"DUP-{Guid.NewGuid():N}"[..20];

        async Task<HttpResponseMessage> CreateAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reference-data/sectors")
            {
                Content = JsonContent.Create(new { code, displayName = "Sektör", sortOrder = 0 }),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return await _client.SendAsync(request);
        }

        var first = await CreateAsync();
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await CreateAsync();
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task CreateSector_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reference-data/sectors")
        {
            Content = JsonContent.Create(new { code = "X", displayName = "X", sortOrder = 0 }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateTaxOffice_WithValidProvince_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();

        var provinces = await (await _client.GetAsync("/api/v1/reference-data/provinces"))
            .Content.ReadFromJsonAsync<List<LookupItemSummary>>();
        var istanbul = provinces!.Single(p => p.Code == "34");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reference-data/tax-offices")
        {
            Content = JsonContent.Create(new
            {
                code = $"TO-{Guid.NewGuid():N}"[..20],
                displayName = "Test Vergi Dairesi",
                sortOrder = 0,
                provinceId = istanbul.Id,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateTaxOffice_WithUnknownProvince_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reference-data/tax-offices")
        {
            Content = JsonContent.Create(new
            {
                code = $"TO-{Guid.NewGuid():N}"[..20],
                displayName = "Test Vergi Dairesi",
                sortOrder = 0,
                provinceId = Guid.NewGuid(),
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
