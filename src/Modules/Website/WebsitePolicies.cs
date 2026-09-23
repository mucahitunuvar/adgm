namespace GenclikMerkezi.Modules.Website;

// ADR-024 §2: Website endpoints authorize via named policies rather than a direct RequireRole call
// (unlike every other module's per-endpoint literal-role pattern) so that a future real permission
// system only needs the policy *definitions* (see WebsiteModuleServiceCollectionExtensions) changed
// - endpoints referencing these constants never need to be touched.
public static class WebsitePolicies
{
    public const string ContentManage = "Website.Content.Manage";
    public const string ContentPublish = "Website.Content.Publish";
    public const string StructureManage = "Website.Structure.Manage";
    public const string DesignManage = "Website.Design.Manage";
    public const string SettingsManage = "Website.Settings.Manage";
    public const string SubmissionsView = "Website.Submissions.View";
    public const string SubmissionsManage = "Website.Submissions.Manage";
}
