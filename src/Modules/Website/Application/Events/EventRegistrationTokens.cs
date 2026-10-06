using System.Security.Cryptography;
using System.Text;

namespace GenclikMerkezi.Modules.Website.Application.Events;

// ADR-024 §11.2 (Faz 4 Görev 3). CancelToken mirrors NewsletterSubscriber's own
// GenerateUnsubscribeToken exactly (32 random bytes, hex-encoded, stored and looked up as-is -
// EventRegistration.CancelToken's own remarks explain why). VerificationTokenHash is deliberately
// different: the raw token is emailed but never persisted, only its SHA-256 hash is (so a database
// read alone - a backup, a leaked connection string - cannot be replayed as a valid verification
// link); Hash is not a secret-keyed HMAC because the raw token itself already has 256 bits of
// entropy, the same amount a keyed hash would add no meaningful protection against guessing.
internal static class EventRegistrationTokens
{
    public static string GenerateRawToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    public static string Hash(string rawToken) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
