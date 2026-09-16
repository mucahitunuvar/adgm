using System.Security.Cryptography;
using System.Text;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;

// EF Core's HasData() needs a fixed key per seeded row so regenerating a migration later (e.g.
// adding one more seed row) diffs against the existing rows instead of re-keying all of them.
// Not cryptographically meaningful - only needs to be stable for the same input string.
internal static class DeterministicGuid
{
    public static Guid Create(string input) => new(MD5.HashData(Encoding.UTF8.GetBytes(input)));
}
