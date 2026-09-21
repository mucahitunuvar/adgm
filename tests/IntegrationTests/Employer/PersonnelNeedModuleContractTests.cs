using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.CreatePersonnelNeed;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Identity.Features.Login;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Employer;

// IPersonnelNeedModuleContract'ın gerçek LocalDB'ye karşı doğrulanması (master prompt item 10) -
// CareerAdvisor Görev 6 (Genel Havuz sayfası) ve Matching Görev 7 (kapatma) henüz bu contract'ı
// tüketmiyor, bu yüzden burada doğrudan DI'dan çözülüp çağrılıyor (RegisterEmployerCompensationTests
// deseniyle aynı: _factory.Services.CreateScope()).
public class PersonnelNeedModuleContractTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PersonnelNeedModuleContractTests(CustomWebApplicationFactory factory)
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

    private async Task<string> CreateCareerAdvisorAndLoginAsync(string adminAccessToken)
    {
        var email = $"danisman-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/career-advisors")
        {
            Content = JsonContent.Create(new { email, password, firstName = "Ayşe", lastName = "Kaya", phoneNumber = (string?)null }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await response.Content.ReadFromJsonAsync<CreateCareerAdvisorResponse>();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.AccessToken;
    }

    private async Task<string> RegisterAndApproveEmployerAsync(string adminAccessToken)
    {
        var email = $"firma-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/employer/register",
            new
            {
                email,
                password,
                name = "Acme A.Ş.",
                sectorId = Guid.NewGuid(),
                foundedYear = (int?)null,
                employeeCount = (int?)null,
                websiteUrl = (string?)null,
                countryId = Guid.NewGuid(),
                provinceId = Guid.NewGuid(),
                districtId = Guid.NewGuid(),
                address = "Adres",
                aboutHtml = (string?)null,
                contactFirstName = "Ayşe",
                contactLastName = "Kaya",
                contactPhone = "05551234567",
                taxOfficeId = Guid.NewGuid(),
                taxNumber = Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(),
                marketingConsent = false,
            });
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterEmployerResponse>();

        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/companies/{registerBody!.CompanyId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(approveRequest)).StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.AccessToken;
    }

    private static object ValidPersonnelNeedPayload() => new
    {
        employmentTypeId = Guid.NewGuid(),
        workLocationTypeId = Guid.NewGuid(),
        positionId = Guid.NewGuid(),
        departmentId = Guid.NewGuid(),
        quantity = 3,
        provinceId = Guid.NewGuid(),
        experienceLevelId = Guid.NewGuid(),
        detailsText = "Acil ihtiyaç",
        genderPreferenceIds = Array.Empty<Guid>(),
        militaryStatusPreferenceIds = Array.Empty<Guid>(),
        educationLevelPreferenceIds = Array.Empty<Guid>(),
        drivingLicensePreferenceIds = Array.Empty<Guid>(),
    };

    private async Task<Guid> CreateAndSubmitPersonnelNeedAsync(string employerAccessToken)
    {
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/personnel-needs")
        {
            Content = JsonContent.Create(ValidPersonnelNeedPayload()),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreatePersonnelNeedResponse>();

        var submitRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/employer/personnel-needs/{createBody!.PersonnelNeedId}/submit");
        submitRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(submitRequest)).StatusCode);

        return createBody.PersonnelNeedId;
    }

    [Fact]
    public async Task GetGeneralPoolAsync_ReturnsPersonnelNeedsInGenelHavuzda()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var advisorAccessToken = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var employerAccessToken = await RegisterAndApproveEmployerAsync(adminAccessToken);
        var personnelNeedId = await CreateAndSubmitPersonnelNeedAsync(employerAccessToken);

        var poolRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/personnel-needs/{personnelNeedId}/pool");
        poolRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(poolRequest)).StatusCode);

        using var scope = _factory.Services.CreateScope();
        var contract = scope.ServiceProvider.GetRequiredService<IPersonnelNeedModuleContract>();

        var generalPool = await contract.GetGeneralPoolAsync();

        Assert.Contains(generalPool, p => p.Id == personnelNeedId);
    }

    [Fact]
    public async Task CloseAsync_TransitionsPersonnelNeedToKarsilandi()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var employerAccessToken = await RegisterAndApproveEmployerAsync(adminAccessToken);
        var personnelNeedId = await CreateAndSubmitPersonnelNeedAsync(employerAccessToken);
        var closedByAdvisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var contract = scope.ServiceProvider.GetRequiredService<IPersonnelNeedModuleContract>();

        var result = await contract.CloseAsync(personnelNeedId, closedByAdvisorId, candidateCvId);

        Assert.True(result.IsSuccess);

        var personnelNeedRepository = scope.ServiceProvider.GetRequiredService<IPersonnelNeedRepository>();
        var personnelNeed = await personnelNeedRepository.GetByIdAsync(personnelNeedId);

        Assert.NotNull(personnelNeed);
        Assert.Equal(PersonnelNeedStatus.Karsilandi, personnelNeed!.Status);
        Assert.Equal(closedByAdvisorId, personnelNeed.ClosedByAdvisorId);
        Assert.Equal(candidateCvId, personnelNeed.FulfilledByCandidateCvId);
    }
}
