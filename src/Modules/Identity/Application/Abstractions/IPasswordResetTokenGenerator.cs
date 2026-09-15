namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

public interface IPasswordResetTokenGenerator
{
    string GenerateToken();

    string Hash(string token);

    TimeSpan Lifetime { get; }
}
