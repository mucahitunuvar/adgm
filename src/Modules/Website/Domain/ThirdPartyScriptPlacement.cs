namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13 (Faz 3 Görev 7): where the frontend injects the script tag.
public enum ThirdPartyScriptPlacement
{
    Head,
    BodyEnd,
}
