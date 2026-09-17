using System.Net;
using System.Net.Http.Json;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Candidate;

// Exercises RegisterCandidateCommandHandler's compensating action against a real, unmocked
// infrastructure failure (BrokenCandidateDatabaseWebApplicationFactory), per the Candidate module
// master prompt's explicit instruction for this scenario.
public class RegisterCandidateCompensationTests : IClassFixture<BrokenCandidateDatabaseWebApplicationFactory>
{
    private readonly BrokenCandidateDatabaseWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RegisterCandidateCompensationTests(BrokenCandidateDatabaseWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WhenCandidateCvPersistenceFails_DeactivatesTheJustCreatedUser()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password = "Sifre123", firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null });

        // CandidateDbContext.SaveChangesAsync throws for real (unreachable LocalDB instance);
        // RegisterCandidateCommandHandler compensates then rethrows, so GlobalExceptionHandler maps
        // it to 500 - this is an unexpected infrastructure failure, not an expected Result failure.
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = await userRepository.GetByEmailAsync(Email.Create(email).Value);

        Assert.NotNull(user);
        Assert.Equal(UserStatus.Disabled, user!.Status);
    }
}
