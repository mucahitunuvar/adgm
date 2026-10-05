namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §1/§12.2 (Faz 3 Görev 3): the closed set of file types an admin may allow for a File field -
// deliberately not an open MIME-type string, so FileSignatureValidator (Görev 4) only ever needs to
// recognize these four signatures.
public enum FormFieldAllowedFileType
{
    Pdf,
    Docx,
    Jpg,
    Png,
}
