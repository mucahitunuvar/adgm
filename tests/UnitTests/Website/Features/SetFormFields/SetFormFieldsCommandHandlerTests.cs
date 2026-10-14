using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.SetFormFields;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.SetFormFields;

public class SetFormFieldsCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeFormDefinitionRepository _formDefinitionRepository = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private SetFormFieldsCommandHandler CreateHandler() =>
        new(_formDefinitionRepository, new FakeCurrentUserContext(UserId), TimeProvider.System, _cacheService, new FakeUnitOfWork());

    private FormDefinition SeedForm()
    {
        var form = FormDefinition.Create(
            "iletisim-formu", 30, [], LegalDocumentKey.Create("gizlilik-bildirimi").Value, [], Tr, "İletişim Formu", null,
            "Teşekkürler, formunuz alındı.", "Gönder", UserId, Now).Value;
        _formDefinitionRepository.Seed(form);
        return form;
    }

    private static FormFieldInput CreateField(string? type = "Text", IReadOnlyList<string>? allowedFileTypes = null) =>
        new(
            "ad-soyad", type, true, 1, null, null, [], null, null, allowedFileTypes ?? [], null,
            [new FormFieldTranslationInput("tr", "Ad Soyad", null, null)]);

    [Theory]
    [InlineData("999")]
    [InlineData("Text,Email")] // 0|1 = 1 = Email - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedFieldType_Fails(string type)
    {
        var form = SeedForm();
        var command = new SetFormFieldsCommand(form.Id, form.RowVersion, [CreateField(type)]);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.InvalidType", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Pdf,Docx")] // 0|1 = 1 = Docx - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedAllowedFileType_Fails(string allowedFileType)
    {
        var form = SeedForm();
        var command = new SetFormFieldsCommand(form.Id, form.RowVersion, [CreateField("File", [allowedFileType])]);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("FormField.InvalidAllowedFileType", result.Error.Code);
    }
}
