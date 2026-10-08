using System.Security.Cryptography;
using System.Text;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4 (Faz 5 Görev 7): pure hashing of an already-serialized snapshot string - the serializer
// itself (System.Text.Json) stays in the Application layer (ContentItemRevisionSnapshot), so Domain
// only ever sees and hashes plain text, never a serialization framework type.
public static class ContentItemRevisionHasher
{
    public static string ComputeHash(string snapshotJson)
    {
        var bytes = Encoding.UTF8.GetBytes(snapshotJson);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }
}
