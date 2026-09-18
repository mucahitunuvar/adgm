using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.Login;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.CareerAdvisor;

// Exercises CreateCareerAdvisorCommandHandler's compensating action against a real, unmocked
// infrastructure failure (BrokenCareerAdvisorDatabaseWebApplicationFactory), mirroring
// Candidate's RegisterCandidateCompensationTests.
public class CreateCareerAdvisorCompensationTests : IClassFixture<BrokenCareerAdvisorDatabaseWebApplicationFactory>
{
    private readonly BrokenCareerAdvisorDatabaseWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateCareerAdvisorCompensationTests(BrokenCareerAdvisorDatabaseWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WhenCareerAdvisorPersistenceFails_DeactivatesTheJustCreatedUser()
    {
        var adminEmail = $"admin-{Guid.NewGuid():N}@example.com";
        const string adminPassword = "AdminSifre123";
        await _factory.SeedAdminUserAsync(adminEmail, adminPassword);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = adminEmail, password = adminPassword });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var email = $"danisman-{Guid.NewGuid():N}@example.com";
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/career-advisors")
        {
            Content = JsonContent.Create(new { email, password = "Sifre123", firstName = "Ayşe", lastName = "Kaya", phoneNumber = (string?)null }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        // CareerAdvisorDbContext.SaveChangesAsync throws for real (unreachable LocalDB instance);
        // CreateCareerAdvisorCommandHandler compensates then rethrows, so GlobalExceptionHandler
        // maps it to 500 - this is an unexpected infrastructure failure, not an expected Result failure.
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = await userRepository.GetByEmailAsync(Email.Create(email).Value);

        Assert.NotNull(user);
        Assert.Equal(UserStatus.Disabled, user!.Status);
    }
}
