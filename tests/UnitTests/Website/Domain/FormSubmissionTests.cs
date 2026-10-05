using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class FormSubmissionTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LegalDocumentKey PrivacyNoticeKey = LegalDocumentKey.Create("kvkk-contact").Value;
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    private static FileAttachment CreateFile() =>
        FileAttachment.Create("website-form-attachments/2026/10/05/abc.pdf", "cv.pdf", "application/pdf", 1024, Now, "FormSubmission", Guid.NewGuid());

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var submissionId = Guid.NewGuid();
        var formDefinitionId = Guid.NewGuid();
        var fileAttachment = FormSubmissionFileAttachment.Create("cv", CreateFile());
        var acceptedVersion = FormSubmissionAcceptedLegalVersion.Create(PrivacyNoticeKey, 1, isPrivacyNotice: true);

        var result = FormSubmission.Create(
            submissionId, formDefinitionId, 3, "[]", "GM-2026-000001", Tr, Now, null, null, "{}", [fileAttachment], [acceptedVersion]);

        Assert.True(result.IsSuccess);
        Assert.Equal(submissionId, result.Value.Id);
        Assert.Equal(formDefinitionId, result.Value.FormDefinitionId);
        Assert.Equal(3, result.Value.DefinitionVersion);
        Assert.Equal("GM-2026-000001", result.Value.ReferenceNumber);
        Assert.Equal(FormSubmissionStatus.New, result.Value.Status);
        Assert.Single(result.Value.FileAttachments);
        Assert.Single(result.Value.AcceptedLegalVersions);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutReferenceNumber_Fails(string? referenceNumber)
    {
        var result = FormSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(), 1, "[]", referenceNumber!, Tr, Now, null, null, "{}", [], []);

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmission.ReferenceNumberRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithOptionalFieldsOmitted_Succeeds()
    {
        var result = FormSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(), 1, "[]", "GM-2026-000002", Tr, Now, submittedByUserId: null, sourceContentItemId: null, "{}", [], []);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.SubmittedByUserId);
        Assert.Null(result.Value.SourceContentItemId);
    }
}
