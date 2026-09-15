using GenclikMerkezi.Modules.Identity.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakePasswordHasher : IPasswordHasher
{
    private const string Prefix = "hashed:";

    public string Hash(string password) => Prefix + password;

    public bool Verify(string password, string hashedPassword) => hashedPassword == Prefix + password;
}
