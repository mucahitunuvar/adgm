namespace GenclikMerkezi.Modules.ReferenceData;

public sealed class ReferenceDataModuleMarker
{
    // See IdentityModuleMarker/NotificationModuleMarker for why this is keyed rather than a plain
    // AddScoped<IUnitOfWork> registration.
    public const string UnitOfWorkKey = "ReferenceData";
}
