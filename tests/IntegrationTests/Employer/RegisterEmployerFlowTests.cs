using System.Net;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;

namespace GenclikMerkezi.IntegrationTests.Employer;

public class RegisterEmployerFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RegisterEmployerFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object ValidPayload(string email, string taxNumber) => new
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
        taxNumber,
        marketingConsent = true,
    };

    // TaxNumber şekil kuralı tam 10 haneli rakam olduğu için (RegisterEmployerCommandValidator),
    // Guid tabanlı bir string (hex karakterler içerebilir) burada kullanılamaz.
    private static string RandomTaxNumber() => Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString();

    [Fact]
    public async Task Register_WithValidInput_CreatesUserAndPendingApprovalCompany()
    {
        var email = $"firma-{Guid.NewGuid():N}@example.com";
        var taxNumber = RandomTaxNumber();

        var response = await _client.PostAsJsonAsync("/api/v1/employer/register", ValidPayload(email, taxNumber));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RegisterEmployerResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.UserId);
        Assert.NotEqual(Guid.Empty, body.CompanyId);
    }

    [Fact]
    public async Task Register_WithAlreadyRegisteredEmail_ReturnsConflict()
    {
        var email = $"firma-{Guid.NewGuid():N}@example.com";
        var payload = ValidPayload(email, RandomTaxNumber());

        var first = await _client.PostAsJsonAsync("/api/v1/employer/register", payload);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync(
            "/api/v1/employer/register", ValidPayload(email, RandomTaxNumber()));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_WithAlreadyRegisteredTaxNumber_ReturnsConflict()
    {
        var taxNumber = RandomTaxNumber();
        var first = await _client.PostAsJsonAsync(
            "/api/v1/employer/register", ValidPayload($"firma-{Guid.NewGuid():N}@example.com", taxNumber));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync(
            "/api/v1/employer/register", ValidPayload($"firma-{Guid.NewGuid():N}@example.com", taxNumber));

        // TaxNumber'daki unique index ihlali EF Core tarafından bir DbUpdateException olarak
        // yükselir - RegisterEmployerCommandHandler bunu bir Result.Failure olarak değil, beklenmeyen
        // bir altyapı hatası olarak ele alır (compensation + rethrow), GlobalExceptionHandler 500'e eşler.
        Assert.Equal(HttpStatusCode.InternalServerError, second.StatusCode);
    }

    [Fact]
    public async Task Register_WithEmptyName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/employer/register",
            new
            {
                email = $"firma-{Guid.NewGuid():N}@example.com",
                password = "Sifre123",
                name = "",
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
                taxNumber = RandomTaxNumber(),
                marketingConsent = false,
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidTaxNumberFormat_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/employer/register", ValidPayload($"firma-{Guid.NewGuid():N}@example.com", "123"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
