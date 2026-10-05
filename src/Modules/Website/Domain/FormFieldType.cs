namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 3): the typed field kinds a FormField can take. Text/Textarea carry
// Min/MaxLength, Select/MultiSelect carry Options, Date carries optional Min/Max, File carries
// AllowedFileTypes/MaxSizeMb - which constraints apply to which Type is FormField.Create's job.
public enum FormFieldType
{
    Text,
    Email,
    Phone,
    Textarea,
    Select,
    MultiSelect,
    Checkbox,
    Date,
    File,
}
