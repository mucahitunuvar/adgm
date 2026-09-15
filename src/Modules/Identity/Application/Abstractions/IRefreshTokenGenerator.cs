namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

public interface IRefreshTokenGenerator
{
    string GenerateToken();

    string Hash(string token);

    TimeSpan Lifetime { get; }
}
