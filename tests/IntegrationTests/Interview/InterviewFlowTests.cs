using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Employer;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;
using GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsCandidate;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Interview;

// Uçtan uca akış (PROJECT.md §8.4-8.5): aday talep açar -> danışman planlar -> danışman sonucu
// kaydeder -> hem aday hem firma kendi query'lerinde görüşmeyi doğru durumda görür. Görev 6/7'deki
// en-az-yüklü-danışman "mayını"ndan kaçınmak için: gerçek kayıt akışlarının (register) otomatik
// atadığı danışman ne olursa olsun, adayın CareerAdvisorId'si DI'dan çözülen repository ile
// deterministik bir danışmana zorlanır (AssignCareerAdvisor) - yalnızca Request/Schedule/
// RecordResult/query adımları gerçek HTTP ile test edilir.
public class InterviewFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public InterviewFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<(string AccessToken, Guid CompanyId)> RegisterAndApproveEmployerAsync(string adminAccessToken)
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
        return (login!.AccessToken, registerBody.CompanyId);
    }

    [Fact]
    public async Task RequestScheduleAndRecordResult_ReflectsInBothPartiesQueries()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (advisorAccessToken, advisorId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (candidateAccessToken, _) = await RegisterCandidateAndForceAdvisorAsync(advisorId);
        var (employerAccessToken, companyId) = await RegisterAndApproveEmployerAsync(adminAccessToken);

        // Aday, firmayla görüşme talep ediyor.
        var requestRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/candidates/interviews")
        {
            Content = JsonContent.Create(new { companyId }),
        };
        requestRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", candidateAccessToken);
        var requestResponse = await _client.SendAsync(requestRequest);
        Assert.Equal(HttpStatusCode.Created, requestResponse.StatusCode);
        var requestBody = await requestResponse.Content.ReadFromJsonAsync<RequestInterviewAsCandidateResponse>();

        // Danışman görüşmeyi planlıyor.
        var scheduleRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/interviews/{requestBody!.InterviewId}/schedule")
        {
            Content = JsonContent.Create(new { scheduledAtUtc = DateTime.UtcNow.AddDays(3) }),
        };
        scheduleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(scheduleRequest)).StatusCode);

        // Danışman sonucu kaydediyor.
        var resultRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/interviews/{requestBody.InterviewId}/result")
        {
            Content = JsonContent.Create(new { outcome = "Olumlu", resultNotes = "İyi geçti" }),
        };
        resultRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(resultRequest)).StatusCode);

        // Aday kendi query'sinde görüşmeyi Tamamlandi/Olumlu olarak görüyor.
        var candidateInterviewsRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/candidates/interviews");
        candidateInterviewsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", candidateAccessToken);
        var candidateInterviewsResponse = await _client.SendAsync(candidateInterviewsRequest);
        Assert.Equal(HttpStatusCode.OK, candidateInterviewsResponse.StatusCode);
        var candidateInterviews = await candidateInterviewsResponse.Content.ReadFromJsonAsync<List<InterviewResponse>>();
        var candidateSideInterview = Assert.Single(candidateInterviews!, i => i.Id == requestBody.InterviewId);
        Assert.Equal("Tamamlandi", candidateSideInterview.Status);
        Assert.Equal("Olumlu", candidateSideInterview.Outcome);
        Assert.Equal("İyi geçti", candidateSideInterview.ResultNotes);

        // Firma da kendi query'sinde aynı görüşmeyi görüyor.
        var employerInterviewsRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/employer/interviews");
        employerInterviewsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        var employerInterviewsResponse = await _client.SendAsync(employerInterviewsRequest);
        Assert.Equal(HttpStatusCode.OK, employerInterviewsResponse.StatusCode);
        var employerInterviews = await employerInterviewsResponse.Content.ReadFromJsonAsync<List<InterviewResponse>>();
        Assert.Contains(employerInterviews!, i => i.Id == requestBody.InterviewId && i.Status == "Tamamlandi");
    }

    [Fact]
    public async Task Schedule_AsDifferentAdvisor_ReturnsForbidden()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (_, organizingAdvisorId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (otherAdvisorAccessToken, _) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (candidateAccessToken, _) = await RegisterCandidateAndForceAdvisorAsync(organizingAdvisorId);
        var (_, companyId) = await RegisterAndApproveEmployerAsync(adminAccessToken);

        var requestRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/candidates/interviews")
        {
            Content = JsonContent.Create(new { companyId }),
        };
        requestRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", candidateAccessToken);
        var requestResponse = await _client.SendAsync(requestRequest);
        var requestBody = await requestResponse.Content.ReadFromJsonAsync<RequestInterviewAsCandidateResponse>();

        var scheduleRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/interviews/{requestBody!.InterviewId}/schedule")
        {
            Content = JsonContent.Create(new { scheduledAtUtc = DateTime.UtcNow.AddDays(3) }),
        };
        scheduleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherAdvisorAccessToken);

        var response = await _client.SendAsync(scheduleRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
