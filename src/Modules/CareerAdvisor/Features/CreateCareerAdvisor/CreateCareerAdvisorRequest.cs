namespace GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;

public sealed record CreateCareerAdvisorRequest(string Email, string Password, string FirstName, string LastName, string? PhoneNumber);
