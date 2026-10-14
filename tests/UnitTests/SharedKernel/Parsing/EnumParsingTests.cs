using GenclikMerkezi.SharedKernel.Parsing;

namespace GenclikMerkezi.UnitTests.SharedKernel.Parsing;

public class EnumParsingTests
{
    private enum Sample
    {
        Draft,
        Published,
        Unpublished,
        Archived,
    }

    [Fact]
    public void Returns_true_for_an_exact_member_name()
    {
        var success = EnumParsing.TryParseDefined<Sample>("Published", out var result);

        Assert.True(success);
        Assert.Equal(Sample.Published, result);
    }

    [Fact]
    public void Is_case_insensitive_by_default()
    {
        var success = EnumParsing.TryParseDefined<Sample>("published", out var result);

        Assert.True(success);
        Assert.Equal(Sample.Published, result);
    }

    [Fact]
    public void Can_be_made_case_sensitive()
    {
        var success = EnumParsing.TryParseDefined<Sample>("published", out _, ignoreCase: false);

        Assert.False(success);
    }

    [Fact]
    public void Trims_surrounding_whitespace()
    {
        var success = EnumParsing.TryParseDefined<Sample>("  Published  ", out var result);

        Assert.True(success);
        Assert.Equal(Sample.Published, result);
    }

    [Fact]
    public void Rejects_a_numeric_string_even_when_it_matches_an_underlying_value()
    {
        // Enum.TryParse would happily return Sample.Published here (the bug this guards against).
        var success = EnumParsing.TryParseDefined<Sample>("1", out _);

        Assert.False(success);
    }

    [Fact]
    public void Rejects_a_comma_combination_even_when_it_coincides_with_a_defined_member()
    {
        // Published (1) | Unpublished (2) = 3 = Archived. Enum.TryParse AND Enum.IsDefined would
        // both accept this as "Archived", even though the caller asked for neither name.
        var success = EnumParsing.TryParseDefined<Sample>("Published,Unpublished", out _);

        Assert.False(success);
    }

    [Fact]
    public void Rejects_an_undefined_number()
    {
        var success = EnumParsing.TryParseDefined<Sample>("999", out _);

        Assert.False(success);
    }

    [Fact]
    public void Rejects_null_and_empty_input()
    {
        Assert.False(EnumParsing.TryParseDefined<Sample>(null, out _));
        Assert.False(EnumParsing.TryParseDefined<Sample>(string.Empty, out _));
    }

    [Fact]
    public void Rejects_garbage_input()
    {
        Assert.False(EnumParsing.TryParseDefined<Sample>("not-a-member", out _));
    }
}
