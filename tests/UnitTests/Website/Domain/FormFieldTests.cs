using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class FormFieldTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;

    private static IReadOnlyList<FormFieldTranslation> Translations(string label = "Ad") =>
        [FormFieldTranslation.Create(Tr, label, null, null).Value];

    private static IReadOnlyList<FormFieldOption> Options(int count)
    {
        var options = new List<FormFieldOption>();
        for (var i = 0; i < count; i++)
        {
            var optionTranslations = new List<FormFieldOptionTranslation> { FormFieldOptionTranslation.Create(Tr, $"Seçenek {i}").Value };
            options.Add(FormFieldOption.Create($"option_{i}", optionTranslations).Value);
        }

        return options;
    }

    [Fact]
    public void Create_TextFieldWithValidLengths_Succeeds()
    {
        var result = FormField.Create(
            "full_name", FormFieldType.Text, true, 0, 2, 100, [], null, null, [], null, Translations());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.MinLength);
        Assert.Equal(100, result.Value.MaxLength);
    }

    [Fact]
    public void Create_TextFieldWithMinGreaterThanMax_Fails()
    {
        var result = FormField.Create(
            "full_name", FormFieldType.Text, true, 0, 100, 10, [], null, null, [], null, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.MinLengthGreaterThanMaxLength", result.Error.Code);
    }

    [Fact]
    public void Create_TextFieldWithMaxLengthOverLimit_Fails()
    {
        var result = FormField.Create(
            "message", FormFieldType.Textarea, true, 0, null, FormField.MaxTextLength + 1, [], null, null, [], null, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.LengthInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_NonTextFieldWithLengthSet_Fails()
    {
        var result = FormField.Create(
            "accepts", FormFieldType.Checkbox, true, 0, 1, 10, [], null, null, [], null, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.LengthNotSupported", result.Error.Code);
    }

    [Fact]
    public void Create_SelectFieldWithTooFewOptions_Fails()
    {
        var result = FormField.Create(
            "city", FormFieldType.Select, true, 0, null, null, Options(1), null, null, [], null, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.OptionCountInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_SelectFieldWithTooManyOptions_Fails()
    {
        var result = FormField.Create(
            "city", FormFieldType.Select, true, 0, null, null, Options(FormField.MaxOptionCount + 1), null, null, [], null, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.OptionCountInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_SelectFieldWithDuplicateOptionKeys_Fails()
    {
        var duplicateOptions = new List<FormFieldOption>
        {
            FormFieldOption.Create("city_a", [FormFieldOptionTranslation.Create(Tr, "A").Value]).Value,
            FormFieldOption.Create("city_a", [FormFieldOptionTranslation.Create(Tr, "A2").Value]).Value,
        };

        var result = FormField.Create(
            "city", FormFieldType.Select, true, 0, null, null, duplicateOptions, null, null, [], null, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.DuplicateOptionKey", result.Error.Code);
    }

    [Fact]
    public void Create_NonSelectFieldWithOptions_Fails()
    {
        var result = FormField.Create(
            "full_name", FormFieldType.Text, true, 0, null, null, Options(2), null, null, [], null, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.OptionsNotSupported", result.Error.Code);
    }

    [Fact]
    public void Create_DateFieldWithValidRange_Succeeds()
    {
        var result = FormField.Create(
            "birth_date", FormFieldType.Date, true, 0, null, null, [], new DateTime(2000, 1, 1), new DateTime(2010, 1, 1), [], null,
            Translations());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_DateFieldWithMinAfterMax_Fails()
    {
        var result = FormField.Create(
            "birth_date", FormFieldType.Date, true, 0, null, null, [], new DateTime(2010, 1, 1), new DateTime(2000, 1, 1), [], null,
            Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.DateMinGreaterThanDateMax", result.Error.Code);
    }

    [Fact]
    public void Create_NonDateFieldWithDateRange_Fails()
    {
        var result = FormField.Create(
            "full_name", FormFieldType.Text, true, 0, null, null, [], new DateTime(2000, 1, 1), null, [], null, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.DateRangeNotSupported", result.Error.Code);
    }

    [Fact]
    public void Create_FileFieldWithValidConstraints_Succeeds()
    {
        var result = FormField.Create(
            "cv", FormFieldType.File, true, 0, null, null, [], null, null, [FormFieldAllowedFileType.Pdf], 5, Translations());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_FileFieldWithoutAllowedTypes_Fails()
    {
        var result = FormField.Create(
            "cv", FormFieldType.File, true, 0, null, null, [], null, null, [], 5, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.AllowedFileTypesRequired", result.Error.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Create_FileFieldWithInvalidMaxSizeMb_Fails(int maxSizeMb)
    {
        var result = FormField.Create(
            "cv", FormFieldType.File, true, 0, null, null, [], null, null, [FormFieldAllowedFileType.Pdf], maxSizeMb, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.MaxSizeMbInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_NonFileFieldWithFileConstraints_Fails()
    {
        var result = FormField.Create(
            "full_name", FormFieldType.Text, true, 0, null, null, [], null, null, [FormFieldAllowedFileType.Pdf], null, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.FileConstraintsNotSupported", result.Error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Full Name")]
    [InlineData("full-name")]
    public void Create_WithInvalidKey_Fails(string? key)
    {
        var result = FormField.Create(key, FormFieldType.Text, true, 0, null, null, [], null, null, [], null, Translations());

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.KeyInvalid", result.Error.Code);
    }
}
