namespace GenclikMerkezi.Modules.Website.Features.DownloadFormSubmissionFile;

public sealed record DownloadFormSubmissionFileResult(byte[] Content, string ContentType, string FileName);
