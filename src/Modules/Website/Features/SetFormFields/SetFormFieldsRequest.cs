namespace GenclikMerkezi.Modules.Website.Features.SetFormFields;

public sealed record SetFormFieldsRequest(byte[] RowVersion, IReadOnlyList<FormFieldInput> Fields);
