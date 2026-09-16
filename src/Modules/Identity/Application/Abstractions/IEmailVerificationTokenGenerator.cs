namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

public interface IEmailVerificationTokenGenerator
{
    string GenerateToken();

    string Hash(string token);

    TimeSpan Lifetime { get; }
}
