using GenclikMerkezi.Modules.Website.Infrastructure.PublicSubmissions;
using Microsoft.AspNetCore.DataProtection;

namespace GenclikMerkezi.UnitTests.Website.Infrastructure.PublicSubmissions;

public class DataProtectionSubmissionTokenGeneratorTests
{
    private static DataProtectionSubmissionTokenGenerator CreateGenerator() =>
        new(DataProtectionProvider.Create("GenclikMerkezi.Tests"));

    [Fact]
    public void ValidateToken_ForFreshlyGeneratedToken_ReturnsIssuedAtUtc()
    {
        var generator = CreateGenerator();
        var issuedAtUtc = new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

        var token = generator.GenerateToken(issuedAtUtc, TimeSpan.FromHours(2));
        var result = generator.ValidateToken(token);

        Assert.True(result.IsSuccess);
        Assert.Equal(issuedAtUtc, result.Value);
    }

    [Fact]
    public async Task ValidateToken_AfterExpiration_Fails()
    {
        var generator = CreateGenerator();
        var token = generator.GenerateToken(DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(1));
        await Task.Delay(50);

        var result = generator.ValidateToken(token);

        Assert.True(result.IsFailure);
        Assert.Equal("PublicSubmission.InvalidToken", result.Error.Code);
    }

    [Fact]
    public void ValidateToken_WithTamperedPayload_Fails()
    {
        var generator = CreateGenerator();
        var token = generator.GenerateToken(DateTimeOffset.UtcNow, TimeSpan.FromHours(2));
        var tampered = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

        var result = generator.ValidateToken(tampered);

        Assert.True(result.IsFailure);
        Assert.Equal("PublicSubmission.InvalidToken", result.Error.Code);
    }

    [Fact]
    public void ValidateToken_WithTokenFromDifferentGeneratorInstance_ButSamePurposeAndApplication_StillValidates()
    {
        var provider = DataProtectionProvider.Create("GenclikMerkezi.Tests");
        var first = new DataProtectionSubmissionTokenGenerator(provider);
        var second = new DataProtectionSubmissionTokenGenerator(provider);
        var issuedAtUtc = new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

        var token = first.GenerateToken(issuedAtUtc, TimeSpan.FromHours(2));
        var result = second.ValidateToken(token);

        Assert.True(result.IsSuccess);
        Assert.Equal(issuedAtUtc, result.Value);
    }

    [Fact]
    public void ValidateToken_ForGarbageInput_Fails()
    {
        var generator = CreateGenerator();

        var result = generator.ValidateToken("not-a-real-token");

        Assert.True(result.IsFailure);
        Assert.Equal("PublicSubmission.InvalidToken", result.Error.Code);
    }

    [Fact]
    public void ValidateToken_ForNullOrWhitespaceInput_Fails()
    {
        var generator = CreateGenerator();

        Assert.True(generator.ValidateToken(null).IsFailure);
        Assert.True(generator.ValidateToken("   ").IsFailure);
    }
}
