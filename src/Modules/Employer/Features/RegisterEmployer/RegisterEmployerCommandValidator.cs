using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;

// Şekil kontrolü (FluentValidation) — ReferenceData FK'ları (Sector/Country/Province/District/
// TaxOffice) mevcut Candidate emsaliyle tutarlı olarak var olma kontrolüne tabi tutulmuyor
// (AddExperienceCommandValidator/UpdateCandidateCvContactInfoCommandValidator da yapmıyor).
// Email/Password'ün benzersizlik ve karmaşıklık kuralları Identity'nin kendi RegisterUserCommand
// validator'ı tarafından zaten uygulanıyor (IIdentityService.CreateUserAsync üzerinden).
public sealed class RegisterEmployerCommandValidator : AbstractValidator<RegisterEmployerCommand>
{
    public RegisterEmployerCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);

        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.SectorId).NotEmpty();
        RuleFor(c => c.FoundedYear).InclusiveBetween(1900, DateTime.UtcNow.Year).When(c => c.FoundedYear is not null);
        RuleFor(c => c.EmployeeCount).GreaterThanOrEqualTo(0).When(c => c.EmployeeCount is not null);
        RuleFor(c => c.WebsiteUrl).MaximumLength(500).When(c => c.WebsiteUrl is not null);
        RuleFor(c => c.CountryId).NotEmpty();
        RuleFor(c => c.ProvinceId).NotEmpty();
        RuleFor(c => c.DistrictId).NotEmpty();
        RuleFor(c => c.Address).NotEmpty().MaximumLength(500);
        RuleFor(c => c.AboutHtml).MaximumLength(4000).When(c => c.AboutHtml is not null);

        RuleFor(c => c.ContactFirstName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.ContactLastName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.ContactPhone).NotEmpty().MaximumLength(20);
        RuleFor(c => c.TaxOfficeId).NotEmpty();

        // Türkiye Vergi Kimlik Numarası: 10 haneli, yalnızca rakam.
        RuleFor(c => c.TaxNumber).NotEmpty().Matches(@"^\d{10}$")
            .WithMessage("TaxNumber must be exactly 10 digits.");
    }
}
