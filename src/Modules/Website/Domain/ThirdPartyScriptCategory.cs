namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13 (Faz 3 Görev 7): the cookie-consent category a script belongs to. Necessary scripts run
// without visitor consent; Analytics/Marketing scripts only load once the visitor has accepted that
// category (frontend's job - this module only ever reports which category a script belongs to).
public enum ThirdPartyScriptCategory
{
    Necessary,
    Analytics,
    Marketing,
}
