using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class NewsletterSubscriberTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LegalDocumentKey PrivacyKey = LegalDocumentKey.Create("newsletter-privacy").Value;
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("missing-at-sign.example.com")]
    public void Create_WithInvalidEmail_Fails(string? email)
    {
        var result = NewsletterSubscriber.Create(email, Tr, PrivacyKey, 1, "unsubscribe-token", Now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_NormalizesEmailToTrimmedLowercase()
    {
        var result = NewsletterSubscriber.Create("  Test@Example.COM  ", Tr, PrivacyKey, 1, "unsubscribe-token", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("test@example.com", result.Value.Email);
        Assert.Equal(NewsletterSubscriberStatus.PendingConfirmation, result.Value.Status);
    }

    [Fact]
    public void Create_WithoutUnsubscribeToken_Fails()
    {
        var result = NewsletterSubscriber.Create("test@example.com", Tr, PrivacyKey, 1, "", Now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Confirm_FromPendingConfirmation_ActivatesAndSetsConfirmedAt()
    {
        var subscriber = NewsletterSubscriber.Create("test@example.com", Tr, PrivacyKey, 1, "token", Now).Value;

        var result = subscriber.Confirm(Now.AddMinutes(5));

        Assert.True(result.IsSuccess);
        Assert.Equal(NewsletterSubscriberStatus.Active, subscriber.Status);
        Assert.Equal(Now.AddMinutes(5), subscriber.ConfirmedAtUtc);
    }

    [Fact]
    public void Confirm_WhenAlreadyActive_Fails()
    {
        var subscriber = NewsletterSubscriber.Create("test@example.com", Tr, PrivacyKey, 1, "token", Now).Value;
        subscriber.Confirm(Now);

        var result = subscriber.Confirm(Now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Unsubscribe_FromActive_SetsUnsubscribedStatusAndTimestamp()
    {
        var subscriber = NewsletterSubscriber.Create("test@example.com", Tr, PrivacyKey, 1, "token", Now).Value;
        subscriber.Confirm(Now);

        var result = subscriber.Unsubscribe(Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(NewsletterSubscriberStatus.Unsubscribed, subscriber.Status);
        Assert.Equal(Now.AddDays(1), subscriber.UnsubscribedAtUtc);
    }

    [Fact]
    public void Unsubscribe_WhenAlreadyUnsubscribed_IsIdempotent()
    {
        var subscriber = NewsletterSubscriber.Create("test@example.com", Tr, PrivacyKey, 1, "token", Now).Value;
        subscriber.Confirm(Now);
        subscriber.Unsubscribe(Now.AddDays(1));

        var result = subscriber.Unsubscribe(Now.AddDays(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(Now.AddDays(1), subscriber.UnsubscribedAtUtc);
    }

    [Fact]
    public void Resubscribe_FromUnsubscribed_ReturnsToPendingConfirmationAndClearsPriorTimestamps()
    {
        var subscriber = NewsletterSubscriber.Create("test@example.com", Tr, PrivacyKey, 1, "token", Now).Value;
        subscriber.Confirm(Now);
        subscriber.Unsubscribe(Now.AddDays(1));

        var newKey = LegalDocumentKey.Create("newsletter-privacy-v2").Value;
        subscriber.Resubscribe(newKey, 2, Tr, Now.AddDays(2));

        Assert.Equal(NewsletterSubscriberStatus.PendingConfirmation, subscriber.Status);
        Assert.Null(subscriber.ConfirmedAtUtc);
        Assert.Null(subscriber.UnsubscribedAtUtc);
        Assert.Equal(Now.AddDays(2), subscriber.SubscribedAtUtc);
        Assert.Equal(newKey, subscriber.AcceptedPrivacyNoticeKey);
        Assert.Equal(2, subscriber.AcceptedPrivacyNoticeVersion);
    }

    [Fact]
    public void CanSendConfirmationEmail_BeforeAnyEmailSent_IsTrue()
    {
        var subscriber = NewsletterSubscriber.Create("test@example.com", Tr, PrivacyKey, 1, "token", Now).Value;

        Assert.True(subscriber.CanSendConfirmationEmail(Now));
    }

    [Fact]
    public void CanSendConfirmationEmail_WithinCooldownOfLastSend_IsFalse()
    {
        var subscriber = NewsletterSubscriber.Create("test@example.com", Tr, PrivacyKey, 1, "token", Now).Value;
        subscriber.RecordConfirmationEmailSent(Now);

        Assert.False(subscriber.CanSendConfirmationEmail(Now.AddMinutes(59)));
    }

    [Fact]
    public void CanSendConfirmationEmail_AfterCooldownElapsed_IsTrue()
    {
        var subscriber = NewsletterSubscriber.Create("test@example.com", Tr, PrivacyKey, 1, "token", Now).Value;
        subscriber.RecordConfirmationEmailSent(Now);

        Assert.True(subscriber.CanSendConfirmationEmail(Now.AddHours(1)));
    }
}
