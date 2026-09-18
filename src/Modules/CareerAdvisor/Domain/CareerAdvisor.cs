using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.CareerAdvisor.Domain;

public sealed class CareerAdvisor : AggregateRoot
{
    public Guid UserId { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Email { get; private set; }

    public string? PhoneNumber { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? DeactivatedAtUtc { get; private set; }

    private CareerAdvisor(
        Guid id, Guid userId, string firstName, string lastName, string email, string? phoneNumber, DateTime createdAtUtc)
        : base(id)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    // Kayıt anında Identity.User'dan bir kerelik seed edilir (CandidateCv.Create ile aynı desen);
    // sonrasında FirstName/LastName/Email/PhoneNumber User'la senkron kalmaz, bağımsız düzenlenebilir.
    public static CareerAdvisor Create(
        Guid userId, string firstName, string lastName, string email, string? phoneNumber, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), userId, firstName, lastName, email, phoneNumber, createdAtUtc);

    // Idempotent: zaten pasif bir danışmanı tekrar deaktive etmek zararsız bir admin aksiyonudur,
    // hata değildir (Identity.User.Deactivate() ile aynı desen) - DeactivatedAtUtc ilk deaktivasyon
    // anını korur, tekrar çağrıda üzerine yazılmaz.
    public void Deactivate(DateTime deactivatedAtUtc)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        DeactivatedAtUtc = deactivatedAtUtc;
    }
}
