using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Employment.Features.CreateEmployment;
using GenclikMerkezi.Modules.Employment.Features.GetEmploymentNotes;
using GenclikMerkezi.Modules.Employment.Features.GetEmploymentsForCandidate;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Employment;

// Uçtan uca akış (PROJECT.md §12): danışman işe yerleşmeyi manuel kaydeder -> not ekler -> sonlandırır
// -> hem adayın hem danışmanın kendi query'lerinde doğru durumu görür. InterviewFlowTests'teki
// en-az-yüklü-danışman "mayını" kaçınma deseniyle aynı: adayın CareerAdvisorId'si HTTP register sonrası
// DI'dan çözülen bir repository ile deterministik olarak zorlanır. CompanyId/PositionId/InterviewId
// Employment tarafından doğrulanmayan, düz cross-module referanslardır (Employer/ReferenceData/
// Interview kayıtları gerçekten var olmak zorunda değil) - bu yüzden gerçek bir firma/pozisyon/görüşme
// oluşturmaya gerek yok, rastgele Guid'ler yeterli.
public class EmploymentFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EmploymentFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<(string AccessToken, Guid CandidateCvId)> RegisterCandidateAndForceAdvisorAsync(Guid careerAdvisorId)
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password, firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null });
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterCandidateResponse>();

        using var scope = _factory.Services.CreateScope();
        var candidateCvRepository = scope.ServiceProvider.GetRequiredService<ICandidateCvRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(CandidateModuleMarker.UnitOfWorkKey);

        var candidateCv = await candidateCvRepository.GetByIdAsync(registerBody!.CandidateCvId);
        candidateCv!.AssignCareerAdvisor(careerAdvisorId);
        await unitOfWork.SaveChangesAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return (login!.AccessToken, registerBody.CandidateCvId);
    }

    [Fact]
    public async Task CreateAddNoteAndEnd_ReflectsInCandidateAndAdvisorQueries()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (advisorAccessToken, advisorId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (candidateAccessToken, candidateCvId) = await RegisterCandidateAndForceAdvisorAsync(advisorId);
        var companyId = Guid.NewGuid();
        var positionId = Guid.NewGuid();

        // Danışman işe yerleşmeyi kaydediyor.
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/career-advisor/employments")
        {
            Content = JsonContent.Create(new
            {
                candidateCvId,
                companyId,
                positionId,
                interviewId = (Guid?)null,
                startDateUtc = DateTime.UtcNow.AddDays(-7),
            }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreateEmploymentResponse>();

        // Danışman not ekliyor.
        var addNoteRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/employments/{createBody!.EmploymentId}/notes")
        {
            Content = JsonContent.Create(new { content = "İşe uyum süreci iyi gidiyor." }),
        };
        addNoteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.Created, (await _client.SendAsync(addNoteRequest)).StatusCode);

        // Danışman işten ayrılışı kaydediyor.
        var endRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/employments/{createBody.EmploymentId}/end")
        {
            Content = JsonContent.Create(new { departureReason = "Daha iyi bir fırsat buldu", endDateUtc = DateTime.UtcNow }),
        };
        endRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(endRequest)).StatusCode);

        // Aday kendi geçmişinde SonaErdi durumunu görüyor.
        var candidateViewRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/employments/candidates/{candidateCvId}");
        candidateViewRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", candidateAccessToken);
        var candidateViewResponse = await _client.SendAsync(candidateViewRequest);
        Assert.Equal(HttpStatusCode.OK, candidateViewResponse.StatusCode);
        var candidateEmployments = await candidateViewResponse.Content.ReadFromJsonAsync<List<EmploymentResponse>>();
        var candidateSideEmployment = Assert.Single(candidateEmployments!, e => e.Id == createBody.EmploymentId);
        Assert.Equal("SonaErdi", candidateSideEmployment.Status);
        Assert.Equal("Daha iyi bir fırsat buldu", candidateSideEmployment.DepartureReason);

        // Danışman not geçmişinde eklediği notu görüyor.
        var notesRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/career-advisor/employments/{createBody.EmploymentId}/notes");
        notesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        var notesResponse = await _client.SendAsync(notesRequest);
        Assert.Equal(HttpStatusCode.OK, notesResponse.StatusCode);
        var notes = await notesResponse.Content.ReadFromJsonAsync<GetEmploymentNotesResponse>();
        Assert.Contains(notes!.Items, n => n.Content == "İşe uyum süreci iyi gidiyor.");
    }

    [Fact]
    public async Task CreateEmployment_AsDifferentAdvisor_ReturnsForbidden()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (_, ownAdvisorId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (otherAdvisorAccessToken, _) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (_, candidateCvId) = await RegisterCandidateAndForceAdvisorAsync(ownAdvisorId);

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/career-advisor/employments")
        {
            Content = JsonContent.Create(new
            {
                candidateCvId,
                companyId = Guid.NewGuid(),
                positionId = Guid.NewGuid(),
                interviewId = (Guid?)null,
                startDateUtc = DateTime.UtcNow,
            }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherAdvisorAccessToken);

        var response = await _client.SendAsync(createRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
