using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class FormSubmissionSequenceTests
{
    [Fact]
    public void CreateForYear_StartsAtZero()
    {
        var sequence = FormSubmissionSequence.CreateForYear(2026);

        Assert.Equal(2026, sequence.Year);
        Assert.Equal(0, sequence.NextValue);
    }

    [Fact]
    public void Reserve_IncrementsAndReturnsNextValue()
    {
        var sequence = FormSubmissionSequence.CreateForYear(2026);

        var first = sequence.Reserve();
        var second = sequence.Reserve();

        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.Equal(2, sequence.NextValue);
    }

    [Fact]
    public void Reserve_RegeneratesRowVersion()
    {
        var sequence = FormSubmissionSequence.CreateForYear(2026);
        var before = sequence.RowVersion;

        sequence.Reserve();

        Assert.NotEqual(before, sequence.RowVersion);
    }
}
