namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §7 / Faz 2 Görev 1 master prompt §1.2: one Menu aggregate per location, seeded empty by
// migration. "Mobile" from ADR-024 §7's original location list is dropped per the Faz 2 master
// prompt's user decision (§1): the mobile menu is not tracked separately, the frontend builds it from
// Header.
public enum MenuLocation
{
    Header = 0,
    Utility = 1,
    Footer = 2,
}
