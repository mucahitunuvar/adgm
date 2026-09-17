using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.SharedKernel.Results;

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

    private async Task<PagedResult<LookupItemSummary>> GetPageAsync(string route)
    {
        var response = await _client.GetAsync(route);
        return (await response.Content.ReadFromJsonAsync<PagedResult<LookupItemSummary>>())!;
    }

    // Walks every page (server-side pageSize is capped at PagedRequest.MaxPageSize) and returns the
    // full list - for tests that need to assert "this specific item is somewhere in the list", not
    // just a total count, now that every list endpoint is paginated.
    private async Task<List<LookupItemSummary>> GetAllItemsAsync(string route, bool includeInactive = false)
    {
        var items = new List<LookupItemSummary>();
        var page = 1;

        while (true)
        {
            var separator = route.Contains('?') ? "&" : "?";
            var url = $"{route}{separator}page={page}&pageSize=100{(includeInactive ? "&activeOnly=false" : string.Empty)}";
            var body = await GetPageAsync(url);
            items.AddRange(body.Items);

            if (!body.HasNextPage)
            {
                break;
            }

            page++;
        }

        return items;
    }

    [Fact]
    public async Task GetCountries_IsAnonymousAndContainsTurkiye()
    {
        var response = await _client.GetAsync("/api/v1/reference-data/countries");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await GetAllItemsAsync("/api/v1/reference-data/countries");
        Assert.Contains(items, i => i.Code == "TR" && i.DisplayName == "Türkiye");
    }

    [Fact]
    public async Task GetDistricts_ReturnsAllSeededDistricts()
    {
        var body = await GetPageAsync("/api/v1/reference-data/districts");

        Assert.Equal(975, body.TotalCount);
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
        var body = await GetPageAsync($"/api/v1/reference-data/{routeSegment}");

        Assert.Equal(expectedCount, body.TotalCount);
    }

    [Fact]
    public async Task DeactivateTaxOffice_PreservesRowAndProvinceLink()
    {
        var accessToken = await LoginAsAdminAsync();

        var provinces = await GetAllItemsAsync("/api/v1/reference-data/provinces");
        var istanbul = provinces.Single(p => p.Code == "34");

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
        var allTaxOffices = await GetAllItemsAsync("/api/v1/reference-data/tax-offices", includeInactive: true);
        var deactivated = allTaxOffices.Single(t => t.Id == id);
        Assert.False(deactivated.IsActive);
        Assert.Equal("Referans Bütünlüğü Testi Vergi Dairesi", deactivated.DisplayName);

        // The parent Province itself must be entirely unaffected by a child TaxOffice's soft-delete.
        var provincesAfter = await GetAllItemsAsync("/api/v1/reference-data/provinces");
        Assert.Contains(provincesAfter, p => p.Id == istanbul.Id && p.IsActive);
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

        var items = await GetAllItemsAsync("/api/v1/reference-data/sectors");
        Assert.Contains(items, i => i.Id == id && i.DisplayName == "Test Sektörü");

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/reference-data/sectors/{id}")
        {
            Content = JsonContent.Create(new { displayName = "Güncellenmiş Sektör", sortOrder = 2 }),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var afterUpdate = await GetAllItemsAsync("/api/v1/reference-data/sectors");
        Assert.Contains(afterUpdate, i => i.Id == id && i.DisplayName == "Güncellenmiş Sektör");

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/reference-data/sectors/{id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var activeOnly = await GetAllItemsAsync("/api/v1/reference-data/sectors");
        Assert.DoesNotContain(activeOnly, i => i.Id == id);

        var includingInactive = await GetAllItemsAsync("/api/v1/reference-data/sectors", includeInactive: true);
        Assert.Contains(includingInactive, i => i.Id == id && !i.IsActive);
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

        var provinces = await GetAllItemsAsync("/api/v1/reference-data/provinces");
        var istanbul = provinces.Single(p => p.Code == "34");

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

    // work-location-types (not sectors): always has 3 seeded rows regardless of test execution
    // order, unlike sectors, which other tests in this class create/deactivate at runtime.
    [Fact]
    public async Task GetWorkLocationTypes_ResponseShape_MatchesPagedResultContract()
    {
        var body = await GetPageAsync("/api/v1/reference-data/work-location-types?page=1&pageSize=2");

        Assert.Equal(1, body.Page);
        Assert.Equal(2, body.PageSize);
        Assert.Equal(3, body.TotalCount);
        Assert.Equal(2, body.TotalPages);
        Assert.Equal(2, body.Items.Count);
        Assert.True(body.HasNextPage);
        Assert.False(body.HasPreviousPage);
    }

    [Fact]
    public async Task GetLookup_WithOutOfRangePageSize_ClampsInsteadOfBadRequest()
    {
        var response = await _client.GetAsync("/api/v1/reference-data/currencies?pageSize=0");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<LookupItemSummary>>();
        Assert.Equal(1, body!.PageSize);
    }
}
