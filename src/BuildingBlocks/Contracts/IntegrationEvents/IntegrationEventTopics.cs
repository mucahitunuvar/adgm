namespace GenclikMerkezi.Contracts.IntegrationEvents;

// [CapSubscribe] requires a compile-time constant, so topic names live here where both the
// publishing and subscribing module already have a reference (GenclikMerkezi.Contracts),
// rather than as string literals duplicated (and easy to typo out of sync) in each module.
public static class IntegrationEventTopics
{
    public const string UserRegistered = "identity.user-registered";
    public const string PasswordResetRequested = "identity.password-reset-requested";
}
