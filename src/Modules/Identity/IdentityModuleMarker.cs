namespace GenclikMerkezi.Modules.Identity;

public sealed class IdentityModuleMarker
{
    // With multiple modules registered in one composition root, a plain (unkeyed)
    // IUnitOfWork registration per module collides: DI's "last registration wins"
    // resolution would silently hand every module's handlers the LAST module's
    // DbContext instead of their own. Each module resolves its own IUnitOfWork by
    // this key instead (see IdentityModuleServiceCollectionExtensions).
    public const string UnitOfWorkKey = "Identity";
}
