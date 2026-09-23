namespace GenclikMerkezi.Modules.Website;

public sealed class WebsiteModuleMarker
{
    // Same reasoning as every other module's marker (e.g. SupportModuleMarker): each module resolves
    // its own IUnitOfWork by this key so a single composition root can host several keyed
    // registrations without them colliding.
    public const string UnitOfWorkKey = "Website";
}
