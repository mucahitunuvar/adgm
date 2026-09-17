using System.Net;
using System.Net.Http.Json;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.ReferenceData;

public class GetDistrictsFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetDistrictsFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<LookupItemSummary> GetIstanbulAsync()
    {
        var provinces = await (await _client.GetAsync("/api/v1/reference-data/provinces?pageSize=100"))
            .Content.ReadFromJsonAsync<PagedResult<LookupItemSummary>>();
        return provinces!.Items.Single(p => p.Code == "34");
    }

    [Fact]
    public async Task GetDistricts_WithoutProvinceId_ReturnsAllSeededDistrictsTotalCount()
    {
        var response = await _client.GetAsync("/api/v1/reference-data/districts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<LookupItemSummary>>();
        Assert.Equal(975, body!.TotalCount);
    }

    [Fact]
    public async Task GetDistricts_WithProvinceId_ReturnsOnlyThatProvincesDistricts()
    {
        var istanbul = await GetIstanbulAsync();

        var response = await _client.GetAsync($"/api/v1/reference-data/districts?provinceId={istanbul.Id}&pageSize=100");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<LookupItemSummary>>();

        // Istanbul has far fewer than 975 districts, and strictly more than one, so this both
        // proves the filter is applied (TotalCount != the unfiltered 975) and gives a paged slice
        // small enough to fit in a single 100-item page.
        Assert.True(body!.TotalCount > 0);
        Assert.True(body.TotalCount < 975);
        Assert.Equal(body.TotalCount, body.Items.Count);
    }

    [Fact]
    public async Task GetDistricts_WithUnknownProvinceId_ReturnsEmptyResult()
    {
        var response = await _client.GetAsync($"/api/v1/reference-data/districts?provinceId={Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<LookupItemSummary>>();
        Assert.Equal(0, body!.TotalCount);
        Assert.Empty(body.Items);
    }

    [Fact]
    public async Task GetDistricts_WithOutOfRangePage_ClampsInsteadOfBadRequest()
    {
        var response = await _client.GetAsync("/api/v1/reference-data/districts?page=0&pageSize=1000");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<LookupItemSummary>>();
        Assert.Equal(1, body!.Page);
        Assert.Equal(100, body.PageSize);
    }
}
