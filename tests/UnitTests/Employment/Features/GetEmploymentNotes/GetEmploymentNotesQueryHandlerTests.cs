using GenclikMerkezi.Modules.Employment.Domain;
using GenclikMerkezi.Modules.Employment.Features.GetEmploymentNotes;
using GenclikMerkezi.UnitTests.Employment.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employment.Features.GetEmploymentNotes;

public class GetEmploymentNotesQueryHandlerTests
{
    private readonly FakeEmploymentNoteRepository _employmentNoteRepository = new();

    private GetEmploymentNotesQueryHandler CreateHandler() => new(_employmentNoteRepository);

    [Fact]
    public async Task Handle_ReturnsNotesForEmployment_NewestFirst()
    {
        var employmentId = Guid.NewGuid();
        var older = EmploymentNote.Create(employmentId, Guid.NewGuid(), "İlk not", DateTime.UtcNow.AddDays(-1));
        var newer = EmploymentNote.Create(employmentId, Guid.NewGuid(), "İkinci not", DateTime.UtcNow);
        _employmentNoteRepository.Add(older);
        _employmentNoteRepository.Add(newer);
        _employmentNoteRepository.Add(EmploymentNote.Create(Guid.NewGuid(), Guid.NewGuid(), "Başka kayıt", DateTime.UtcNow));

        var result = await CreateHandler().Handle(new GetEmploymentNotesQuery(employmentId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(newer.Id, result.Value.Items[0].Id);
        Assert.Equal(older.Id, result.Value.Items[1].Id);
    }

    [Fact]
    public async Task Handle_WithNoNotes_ReturnsEmptyPage()
    {
        var result = await CreateHandler().Handle(new GetEmploymentNotesQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }
}
