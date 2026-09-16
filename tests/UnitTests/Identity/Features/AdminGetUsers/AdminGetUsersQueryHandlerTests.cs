using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.AdminGetUsers;

public class AdminGetUsersQueryHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();

    private AdminGetUsersQueryHandler CreateHandler() => new(_userRepository);

    private User AddUser(string email, UserRole role)
    {
        var user = User.Register(Email.Create(email).Value, PasswordHash.FromHashedValue("hash"), role);
        _userRepository.Add(user);
        return user;
    }

    [Fact]
    public async Task Handle_WithNoFilters_ReturnsAllUsersPaged()
    {
        AddUser("aday1@example.com", UserRole.Candidate);
        AddUser("aday2@example.com", UserRole.Candidate);
        AddUser("employer@example.com", UserRole.Employer);

        var result = await CreateHandler().Handle(
            new AdminGetUsersQuery(null, null, null, null, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Items.Count);
    }

    [Fact]
    public async Task Handle_FilteredByRole_ReturnsOnlyMatchingUsers()
    {
        AddUser("aday1@example.com", UserRole.Candidate);
        AddUser("employer@example.com", UserRole.Employer);

        var result = await CreateHandler().Handle(
            new AdminGetUsersQuery(null, "Employer", null, null, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("employer@example.com", result.Value.Items[0].Email);
    }

    [Fact]
    public async Task Handle_FilteredByEmailSubstring_ReturnsMatchingUsers()
    {
        AddUser("aday1@example.com", UserRole.Candidate);
        AddUser("employer@example.com", UserRole.Employer);

        var result = await CreateHandler().Handle(
            new AdminGetUsersQuery("aday", null, null, null, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("aday1@example.com", result.Value.Items[0].Email);
    }

    [Fact]
    public async Task Handle_FilteredByEmailConfirmed_ReturnsOnlyMatchingUsers()
    {
        var confirmed = AddUser("confirmed@example.com", UserRole.Candidate);
        confirmed.IssueEmailVerificationToken("hash", DateTime.UtcNow.AddHours(1));
        confirmed.ConfirmEmail("hash");
        AddUser("unconfirmed@example.com", UserRole.Candidate);

        var result = await CreateHandler().Handle(
            new AdminGetUsersQuery(null, null, null, null, true, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("confirmed@example.com", result.Value.Items[0].Email);
    }

    [Fact]
    public async Task Handle_FilteredByLockedOut_ReturnsOnlyLockedUsers()
    {
        var locked = AddUser("locked@example.com", UserRole.Candidate);
        for (var i = 0; i < 5; i++)
        {
            locked.RegisterFailedLoginAttempt();
        }

        AddUser("active@example.com", UserRole.Candidate);

        var result = await CreateHandler().Handle(
            new AdminGetUsersQuery(null, null, null, true, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("locked@example.com", result.Value.Items[0].Email);
        Assert.True(result.Value.Items[0].IsLockedOut);
    }

    [Fact]
    public async Task Handle_FilteredByStatus_ReturnsOnlyMatchingUsers()
    {
        var deactivated = AddUser("deactivated@example.com", UserRole.Candidate);
        deactivated.Deactivate();
        AddUser("active@example.com", UserRole.Candidate);

        var result = await CreateHandler().Handle(
            new AdminGetUsersQuery(null, null, "Disabled", null, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("deactivated@example.com", result.Value.Items[0].Email);
        Assert.Equal("Disabled", result.Value.Items[0].Status);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectSliceAndTotals()
    {
        for (var i = 0; i < 5; i++)
        {
            AddUser($"aday{i}@example.com", UserRole.Candidate);
        }

        var result = await CreateHandler().Handle(
            new AdminGetUsersQuery(null, null, null, null, null, 2, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(3, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Page);
    }
}
