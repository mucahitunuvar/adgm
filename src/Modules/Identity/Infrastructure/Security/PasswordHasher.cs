using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace GenclikMerkezi.Modules.Identity.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<User> _innerHasher = new PasswordHasher<User>();

    public string Hash(string password) => _innerHasher.HashPassword(null!, password);

    public bool Verify(string password, string hashedPassword)
    {
        var result = _innerHasher.VerifyHashedPassword(null!, hashedPassword, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
