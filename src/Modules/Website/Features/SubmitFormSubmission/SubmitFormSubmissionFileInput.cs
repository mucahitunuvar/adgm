namespace GenclikMerkezi.Modules.Website.Features.SubmitFormSubmission;

public sealed record SubmitFormSubmissionFileInput(string FieldKey, Stream Content, string FileName, string ContentType, long Length);
