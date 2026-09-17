namespace GenclikMerkezi.Contracts.ReferenceData;

// Every lookup type ReferenceData owns - both SEED and ADMIN-MANAGED (ADR-016 Decision 1) are
// listed here, since a consuming module needs to name which one it's asking about regardless of
// which category it falls into.
public enum ReferenceDataLookupType
{
    Country,
    Province,
    District,
    Language,
    Sector,
    Position,
    Department,
    WorkLocationType,
    EmploymentType,
    EducationLevel,
    Gender,
    MilitaryStatus,
    DriversLicenseType,
    LanguageLevel,
    ExperienceLevel,
    Nationality,
    DisabilityCategory,
    DiplomaGradingSystem,
    ReferenceType,
    Currency,
    Skill,
    SchoolCategory,
    TaxOffice,
    WorkField,
    School,
}
