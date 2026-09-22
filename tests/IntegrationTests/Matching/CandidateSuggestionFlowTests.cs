using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Employer;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Matching.Features.CreateCandidateSuggestion;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Matching;

// Uçtan uca akış (ADR-022 §5-6): danışman A havuza atar -> danışman B önerir -> firmanın danışmanı
// (A) kabul eder -> PersonnelNeed.Status == Karsilandi. Görev 6'daki en-az-yüklü-danışman "mayını"ndan
// kaçınmak için (PersonnelNeedModuleContractTests.GetGeneralPoolAsync_PagesResults_NewestFirst deseni):
// Company/CandidateCv/PersonnelNeed ilişkileri DI'dan çözülen repository'lerle doğrudan tohumlanır
// (deterministik danışman ataması, en-az-yüklü seçim algoritmasına hiç bağımlı değil), yalnızca
// Create/Accept adımları gerçek HTTP çağrılarıyla test edilir.
public class CandidateSuggestionFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CandidateSuggestionFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<(string AccessToken, Guid CareerAdvisorId)> CreateCareerAdvisorAndLoginAsync(string adminAccessToken)
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
        var body = await response.Content.ReadFromJsonAsync<CreateCareerAdvisorResponse>();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return (login!.AccessToken, body!.CareerAdvisorId);
    }

    private async Task<(Guid PersonnelNeedId, Guid CandidateCvId)> SeedPooledPersonnelNeedAndCandidateAsync(
        Guid companyAdvisorId, Guid candidateAdvisorId)
    {
        using var scope = _factory.Services.CreateScope();

        var companyRepository = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
        var personnelNeedRepository = scope.ServiceProvider.GetRequiredService<IPersonnelNeedRepository>();
        var employerUnitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(EmployerModuleMarker.UnitOfWorkKey);

        var company = Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", $"firma-{Guid.NewGuid():N}@example.com", "05550000000", Guid.NewGuid(),
            Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(), false, companyAdvisorId, DateTime.UtcNow);
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        companyRepository.Add(company);

        var personnelNeed = PersonnelNeed.Create(
            company.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
            Guid.NewGuid(), null, [], [], [], [], DateTime.UtcNow);
        personnelNeed.Submit();
        personnelNeed.PoolToGeneral(companyAdvisorId, DateTime.UtcNow);
        personnelNeedRepository.Add(personnelNeed);

        await employerUnitOfWork.SaveChangesAsync(CancellationToken.None);

        var candidateCvRepository = scope.ServiceProvider.GetRequiredService<ICandidateCvRepository>();
        var candidateUnitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(CandidateModuleMarker.UnitOfWorkKey);

        var candidateCv = CandidateCv.Create(
            Guid.NewGuid(), "Ahmet", "Yılmaz", $"aday-{Guid.NewGuid():N}@example.com", null, candidateAdvisorId);
        candidateCvRepository.Add(candidateCv);

        await candidateUnitOfWork.SaveChangesAsync(CancellationToken.None);

        return (personnelNeed.Id, candidateCv.Id);
    }

    [Fact]
    public async Task CreateThenAccept_ClosesPersonnelNeed_AndFulfillsWithSuggestedCandidate()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (advisorAAccessToken, advisorAId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (advisorBAccessToken, advisorBId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);

        var (personnelNeedId, candidateCvId) = await SeedPooledPersonnelNeedAndCandidateAsync(advisorAId, advisorBId);

        // Danışman B kendi adayını öneriyor.
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/career-advisor/candidate-suggestions")
        {
            Content = JsonContent.Create(new { personnelNeedId, candidateCvId }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorBAccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreateCandidateSuggestionResponse>();

        // Firmanın kendi danışmanı (A) öneriyi kabul ediyor.
        var acceptRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/candidate-suggestions/{createBody!.CandidateSuggestionId}/accept");
        acceptRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAAccessToken);
        var acceptResponse = await _client.SendAsync(acceptRequest);
        Assert.Equal(HttpStatusCode.NoContent, acceptResponse.StatusCode);

        // Taze bir scope ile doğrula - HTTP çağrıları kendi scope'unda commit etti, seed'te kullanılan
        // repository'nin change tracker'ı bunu yansıtmaz (identity map eski, tracked entity'yi döner).
        using var verifyScope = _factory.Services.CreateScope();
        var verifyPersonnelNeedRepository = verifyScope.ServiceProvider.GetRequiredService<IPersonnelNeedRepository>();
        var closedPersonnelNeed = await verifyPersonnelNeedRepository.GetByIdAsync(personnelNeedId);

        Assert.NotNull(closedPersonnelNeed);
        Assert.Equal(PersonnelNeedStatus.Karsilandi, closedPersonnelNeed!.Status);
        Assert.Equal(advisorAId, closedPersonnelNeed.ClosedByAdvisorId);
        Assert.Equal(candidateCvId, closedPersonnelNeed.FulfilledByCandidateCvId);
    }

    [Fact]
    public async Task Accept_AsDifferentAdvisor_ReturnsForbidden()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (_, advisorAId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (advisorBAccessToken, advisorBId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (otherAdvisorAccessToken, _) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);

        var (personnelNeedId, candidateCvId) = await SeedPooledPersonnelNeedAndCandidateAsync(advisorAId, advisorBId);

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/career-advisor/candidate-suggestions")
        {
            Content = JsonContent.Create(new { personnelNeedId, candidateCvId }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorBAccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreateCandidateSuggestionResponse>();

        // Ne öneriyi yapan danışman (B) ne de rastgele bir üçüncü danışman kabul edemez - yalnızca
        // firmanın atanmış danışmanı (A).
        var acceptRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/candidate-suggestions/{createBody!.CandidateSuggestionId}/accept");
        acceptRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherAdvisorAccessToken);
        var acceptResponse = await _client.SendAsync(acceptRequest);

        Assert.Equal(HttpStatusCode.Forbidden, acceptResponse.StatusCode);
    }
}
