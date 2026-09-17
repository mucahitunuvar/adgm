using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;
using GenclikMerkezi.Modules.ReferenceData.Features.GetDistricts;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.ReferenceData;

public static class ReferenceDataModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapReferenceDataModuleEndpoints(this IEndpointRouteBuilder app)
    {
        // SEED (ADR-016 Decision 1): read-only, no admin CRUD endpoint of any kind.
        LookupEndpoints.MapReadOnly(app, "countries", ReferenceDataLookupType.Country);
        LookupEndpoints.MapReadOnly(app, "provinces", ReferenceDataLookupType.Province);
        // Bespoke (not LookupEndpoints.MapReadOnly): District needs a ProvinceId filter, same
        // reason TaxOffice below is bespoke rather than generic.
        GetDistrictsEndpoint.Map(app);
        LookupEndpoints.MapReadOnly(app, "languages", ReferenceDataLookupType.Language);

        // ADMIN-MANAGED, plain shape (generic CRUD via ILookupItemFactory<TLookup>).
        LookupEndpoints.MapWithCrud<Sector>(app, "sectors", ReferenceDataLookupType.Sector);
        LookupEndpoints.MapWithCrud<Position>(app, "positions", ReferenceDataLookupType.Position);
        LookupEndpoints.MapWithCrud<Department>(app, "departments", ReferenceDataLookupType.Department);
        LookupEndpoints.MapWithCrud<WorkLocationType>(app, "work-location-types", ReferenceDataLookupType.WorkLocationType);
        LookupEndpoints.MapWithCrud<EmploymentType>(app, "employment-types", ReferenceDataLookupType.EmploymentType);
        LookupEndpoints.MapWithCrud<EducationLevel>(app, "education-levels", ReferenceDataLookupType.EducationLevel);
        LookupEndpoints.MapWithCrud<Gender>(app, "genders", ReferenceDataLookupType.Gender);
        LookupEndpoints.MapWithCrud<MilitaryStatus>(app, "military-statuses", ReferenceDataLookupType.MilitaryStatus);
        LookupEndpoints.MapWithCrud<DriversLicenseType>(app, "drivers-license-types", ReferenceDataLookupType.DriversLicenseType);
        LookupEndpoints.MapWithCrud<LanguageLevel>(app, "language-levels", ReferenceDataLookupType.LanguageLevel);
        LookupEndpoints.MapWithCrud<ExperienceLevel>(app, "experience-levels", ReferenceDataLookupType.ExperienceLevel);
        LookupEndpoints.MapWithCrud<Nationality>(app, "nationalities", ReferenceDataLookupType.Nationality);
        LookupEndpoints.MapWithCrud<DisabilityCategory>(app, "disability-categories", ReferenceDataLookupType.DisabilityCategory);
        LookupEndpoints.MapWithCrud<DiplomaGradingSystem>(app, "diploma-grading-systems", ReferenceDataLookupType.DiplomaGradingSystem);
        LookupEndpoints.MapWithCrud<ReferenceType>(app, "reference-types", ReferenceDataLookupType.ReferenceType);
        LookupEndpoints.MapWithCrud<Currency>(app, "currencies", ReferenceDataLookupType.Currency);
        LookupEndpoints.MapWithCrud<Skill>(app, "skills", ReferenceDataLookupType.Skill);
        LookupEndpoints.MapWithCrud<SchoolCategory>(app, "school-categories", ReferenceDataLookupType.SchoolCategory);

        // ADMIN-MANAGED, bespoke shape (ProvinceId - does not implement ILookupItemFactory<TaxOffice>).
        TaxOfficeEndpoints.Map(app);

        return app;
    }
}
