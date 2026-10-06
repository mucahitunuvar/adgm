namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2/§14 (Faz 3 Görev 5). The closed set of entities PersonalDataAccessLog can refer to -
// NewsletterSubscriber is listed ahead of Görev 6's own feature existing, the same
// declared-ahead-of-use pattern FileCategory's Website members already follow.
public enum PersonalDataEntityType
{
    FormSubmission,
    NewsletterSubscriber,

    // Faz 4 Görev 4: the admin-side registration list/detail endpoints.
    EventRegistration,
}
