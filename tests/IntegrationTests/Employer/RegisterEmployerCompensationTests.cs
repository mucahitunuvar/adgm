using System.Net;
using System.Net.Http.Json;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Employer;

// Exercises RegisterEmployerCommandHandler's compensating action against a real, unmocked
// infrastructure failure (BrokenEmployerDatabaseWebApplicationFactory), mirroring Candidate's
// RegisterCandidateCompensationTests / CareerAdvisor's CreateCareerAdvisorCompensationTests.
public class RegisterEmployerCompensationTests : IClassFixture<BrokenEmployerDatabaseWebApplicationFactory>
{
    private readonly BrokenEmployerDatabaseWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RegisterEmployerCompensationTests(BrokenEmployerDatabaseWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WhenCompanyPersistenceFails_DeactivatesTheJustCreatedUser()
    {
        var email = $"firma-{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsJsonAsync(
            "/api/v1/employer/register",
            new
            {
                email,
                password = "Sifre123",
                name = "Acme A.Ş.",
                sectorId = Guid.NewGuid(),
                foundedYear = 2010,
                employeeCount = 50,
                websiteUrl = "https://acme.example.com",
                countryId = Guid.NewGuid(),
                provinceId = Guid.NewGuid(),
                districtId = Guid.NewGuid(),
                address = "Örnek Mah. No:1",
                aboutHtml = "<p>Hakkımızda</p>",
                contactFirstName = "Ayşe",
                contactLastName = "Kaya",
                contactPhone = "05551234567",
                taxOfficeId = Guid.NewGuid(),
                taxNumber = Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(),
                marketingConsent = true,
            });

        // EmployerDbContext.SaveChangesAsync throws for real (unreachable LocalDB instance);
        // RegisterEmployerCommandHandler compensates then rethrows, so GlobalExceptionHandler maps
        // it to 500 - this is an unexpected infrastructure failure, not an expected Result failure.
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = await userRepository.GetByEmailAsync(Email.Create(email).Value);

        Assert.NotNull(user);
        Assert.Equal(UserStatus.Disabled, user!.Status);
    }
}
