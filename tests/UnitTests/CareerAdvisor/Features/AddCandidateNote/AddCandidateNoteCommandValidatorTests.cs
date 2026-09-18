using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using GenclikMerkezi.Modules.CareerAdvisor.Features.AddCandidateNote;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Features.AddCandidateNote;

public class AddCandidateNoteCommandValidatorTests
{
    private readonly AddCandidateNoteCommandValidator _validator = new();

    private static AddCandidateNoteCommand ValidCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid(), nameof(NoteType.Genel), "İçerik");

    [Theory]
    [InlineData(nameof(NoteType.Genel))]
    [InlineData(nameof(NoteType.IsGorusmesi))]
    [InlineData("genel")]
    public void Validate_WithValidNoteType_Passes(string noteType)
    {
        var result = _validator.Validate(ValidCommand() with { NoteType = noteType });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Gecersiz")]
    [InlineData("")]
    public void Validate_WithInvalidNoteType_Fails(string noteType)
    {
        var result = _validator.Validate(ValidCommand() with { NoteType = noteType });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddCandidateNoteCommand.NoteType));
    }

    [Fact]
    public void Validate_WithContentExceedingMaxLength_Fails()
    {
        var command = ValidCommand() with { Content = new string('a', 4001) };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddCandidateNoteCommand.Content));
    }
}
