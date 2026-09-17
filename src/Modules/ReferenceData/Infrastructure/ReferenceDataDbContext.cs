using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure;

public sealed class ReferenceDataDbContext(DbContextOptions<ReferenceDataDbContext> options)
    : DbContext(options), IUnitOfWork
{
    // SEED
    public DbSet<Country> Countries => Set<Country>();

    public DbSet<Province> Provinces => Set<Province>();

    public DbSet<District> Districts => Set<District>();

    public DbSet<Language> Languages => Set<Language>();

    // ADMIN-MANAGED
    public DbSet<Sector> Sectors => Set<Sector>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<WorkLocationType> WorkLocationTypes => Set<WorkLocationType>();

    public DbSet<EmploymentType> EmploymentTypes => Set<EmploymentType>();

    public DbSet<EducationLevel> EducationLevels => Set<EducationLevel>();

    public DbSet<Gender> Genders => Set<Gender>();

    public DbSet<MilitaryStatus> MilitaryStatuses => Set<MilitaryStatus>();

    public DbSet<DriversLicenseType> DriversLicenseTypes => Set<DriversLicenseType>();

    public DbSet<LanguageLevel> LanguageLevels => Set<LanguageLevel>();

    public DbSet<ExperienceLevel> ExperienceLevels => Set<ExperienceLevel>();

    public DbSet<Nationality> Nationalities => Set<Nationality>();

    public DbSet<DisabilityCategory> DisabilityCategories => Set<DisabilityCategory>();

    public DbSet<DiplomaGradingSystem> DiplomaGradingSystems => Set<DiplomaGradingSystem>();

    public DbSet<ReferenceType> ReferenceTypes => Set<ReferenceType>();

    public DbSet<Currency> Currencies => Set<Currency>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<SchoolCategory> SchoolCategories => Set<SchoolCategory>();

    public DbSet<TaxOffice> TaxOffices => Set<TaxOffice>();

    public DbSet<WorkField> WorkFields => Set<WorkField>();

    public DbSet<School> Schools => Set<School>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReferenceDataDbContext).Assembly);
    }
}
