using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Employer;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.CreatePersonnelNeed;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
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

        var generalPool = await contract.GetGeneralPoolAsync(new PagedRequest());

        Assert.Contains(generalPool.Items, p => p.Id == personnelNeedId);
    }

    // HTTP akışı (register -> en-az-yüklü-danışman ataması -> pool) yerine repository'ler doğrudan
    // DI'dan çözülüp domain metodları çağrılıyor (SyncCandidateReadModelsFlowTests deseni) - bu testin
    // amacı yalnızca GetGeneralPoolAsync'in sayfalama/sıralama davranışı, danışman ataması değil; HTTP
    // akışını kullanmak JobReviewFlowTests'te görülen en-az-yüklü-danışman "mayını"na (paylaşılan
    // CustomWebApplicationFactory'de başka testlerin bıraktığı danışmanlar) gereksiz yere maruz bırakırdı.
    [Fact]
    public async Task GetGeneralPoolAsync_PagesResults_NewestFirst()
    {
        using var scope = _factory.Services.CreateScope();
        var companyRepository = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
        var personnelNeedRepository = scope.ServiceProvider.GetRequiredService<IPersonnelNeedRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(EmployerModuleMarker.UnitOfWorkKey);
        var contract = scope.ServiceProvider.GetRequiredService<IPersonnelNeedModuleContract>();

        var personnelNeedIds = new List<Guid>();
        var pooledAtUtc = DateTime.UtcNow;

        for (var i = 0; i < 3; i++)
        {
            var company = Company.Create(
                Guid.NewGuid(), $"Acme {i} A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), "Adres", null, "Ad", "Soyad", $"firma{Guid.NewGuid():N}@example.com", "05550000000",
                Guid.NewGuid(), Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(), false, null,
                DateTime.UtcNow);
            companyRepository.Add(company);

            var personnelNeed = PersonnelNeed.Create(
                company.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
                Guid.NewGuid(), null, [], [], [], [], DateTime.UtcNow);
            personnelNeed.Submit();
            // Her birine artan bir PooledAtUtc veriliyor ki "en yeni önce" sıralaması deterministik test edilebilsin.
            personnelNeed.PoolToGeneral(Guid.NewGuid(), pooledAtUtc.AddMinutes(i));
            personnelNeedRepository.Add(personnelNeed);

            personnelNeedIds.Add(personnelNeed.Id);
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var firstPage = await contract.GetGeneralPoolAsync(new PagedRequest { Page = 1, PageSize = 2 });

        Assert.Equal(2, firstPage.Items.Count);
        Assert.True(firstPage.TotalCount >= 3);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(2, firstPage.PageSize);
        Assert.True(firstPage.HasNextPage);

        // En yeni önce (PooledAtUtc descending): en son havuza atılan (personnelNeedIds[2]) ilk sayfada olmalı.
        Assert.Contains(firstPage.Items, p => p.Id == personnelNeedIds[2]);
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
