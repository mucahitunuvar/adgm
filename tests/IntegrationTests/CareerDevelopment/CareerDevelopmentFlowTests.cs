using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateDevelopmentPlan;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateSkillGap;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateTrainingRecommendation;
using GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.CareerDevelopment;

// Uçtan uca akış (ADR-011): danışman aday için beceri eksikliği tanımlar -> ona bağlı bir gelişim
// planı oluşturur -> plana bağlı bir eğitim önerisi ekler -> planı tamamlar -> aday kendi özet
// görünümünde tüm kayıtları doğru durumda görür. InterviewFlowTests/EmploymentFlowTests'teki
// en-az-yüklü-danışman "mayını" kaçınma deseniyle aynı: adayın CareerAdvisorId'si HTTP register
// sonrası DI'dan çözülen bir repository ile deterministik olarak zorlanır.
public class CareerDevelopmentFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CareerDevelopmentFlowTests(CustomWebApplicationFactory factory)
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
    public async Task CreatePlanWithSkillGapAndTrainingRecommendation_ThenComplete_ReflectsInCandidateSummary()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (advisorAccessToken, advisorId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (candidateAccessToken, candidateCvId) = await RegisterCandidateAndForceAdvisorAsync(advisorId);

        // Danışman beceri eksikliğini tanımlıyor.
        var skillGapRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/candidates/{candidateCvId}/skill-gaps")
        {
            Content = JsonContent.Create(new { skillId = Guid.NewGuid(), notes = "İngilizce yetersiz" }),
        };
        skillGapRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        var skillGapResponse = await _client.SendAsync(skillGapRequest);
        Assert.Equal(HttpStatusCode.Created, skillGapResponse.StatusCode);
        var skillGapBody = await skillGapResponse.Content.ReadFromJsonAsync<CreateSkillGapResponse>();

        // Danışman bu beceri eksikliğine bağlı bir gelişim planı oluşturuyor.
        var planRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/candidates/{candidateCvId}/development-plans")
        {
            Content = JsonContent.Create(new { skillGapId = skillGapBody!.SkillGapId, careerGoalId = (Guid?)null, description = "İngilizce kursu tamamla" }),
        };
        planRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        var planResponse = await _client.SendAsync(planRequest);
        Assert.Equal(HttpStatusCode.Created, planResponse.StatusCode);
        var planBody = await planResponse.Content.ReadFromJsonAsync<CreateDevelopmentPlanResponse>();

        // Danışman bu plana bağlı bir eğitim öneriyor.
        var trainingRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/candidates/{candidateCvId}/training-recommendations")
        {
            Content = JsonContent.Create(new { developmentPlanId = planBody!.DevelopmentPlanId, trainingId = Guid.NewGuid(), notes = "Uygun eğitim" }),
        };
        trainingRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.Created, (await _client.SendAsync(trainingRequest)).StatusCode);

        // Danışman planı tamamlıyor.
        var completeRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/development-plans/{planBody.DevelopmentPlanId}/complete");
        completeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(completeRequest)).StatusCode);

        // Aday kendi özet görünümünde tüm kayıtları doğru durumda görüyor.
        var summaryRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/career-development/candidates/{candidateCvId}");
        summaryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", candidateAccessToken);
        var summaryResponse = await _client.SendAsync(summaryRequest);
        Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);
        var summary = await summaryResponse.Content.ReadFromJsonAsync<GetCandidateDevelopmentSummaryResponse>();

        Assert.Single(summary!.SkillGaps);
        var plan = Assert.Single(summary.DevelopmentPlans);
        Assert.Equal("Tamamlandi", plan.Status);
        Assert.Equal(skillGapBody.SkillGapId, plan.SkillGapId);
        var recommendation = Assert.Single(summary.TrainingRecommendations);
        Assert.Equal(planBody.DevelopmentPlanId, recommendation.DevelopmentPlanId);
    }

    [Fact]
    public async Task CreateDevelopmentPlan_WithSkillGapFromDifferentCandidate_ReturnsConflict()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (advisorAccessToken, advisorId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (_, firstCandidateCvId) = await RegisterCandidateAndForceAdvisorAsync(advisorId);
        var (_, secondCandidateCvId) = await RegisterCandidateAndForceAdvisorAsync(advisorId);

        var skillGapRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/candidates/{firstCandidateCvId}/skill-gaps")
        {
            Content = JsonContent.Create(new { skillId = Guid.NewGuid(), notes = (string?)null }),
        };
        skillGapRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        var skillGapBody = await (await _client.SendAsync(skillGapRequest)).Content.ReadFromJsonAsync<CreateSkillGapResponse>();

        // İkinci adaya, birinci adayın SkillGapId'siyle bir gelişim planı açmaya çalışıyor.
        var planRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/candidates/{secondCandidateCvId}/development-plans")
        {
            Content = JsonContent.Create(new { skillGapId = skillGapBody!.SkillGapId, careerGoalId = (Guid?)null, description = "Açıklama" }),
        };
        planRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);

        var response = await _client.SendAsync(planRequest);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
