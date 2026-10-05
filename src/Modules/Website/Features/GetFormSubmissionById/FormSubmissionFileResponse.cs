namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissionById;

public sealed record FormSubmissionFileResponse(Guid FileId, string FieldKey, string OriginalFileName, string ContentType, long SizeInBytes);
