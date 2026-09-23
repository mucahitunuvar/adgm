using System.Security.Cryptography;
using System.Text;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Seed;

// EF Core's HasData() needs a fixed key per seeded row so regenerating a migration later (e.g.
// adding one more seed row) diffs against the existing rows instead of re-keying all of them.
internal static class DeterministicGuid
{
    public static Guid Create(string input) => new(MD5.HashData(Encoding.UTF8.GetBytes(input)));
}
