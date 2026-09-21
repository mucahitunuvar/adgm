namespace GenclikMerkezi.Contracts.CareerAdvisor;

// UserId/Email, Görev 3/ADR-022 §5 madde 2'nin broadcast bildirimi için eklendi (havuza atama
// duyurusu tüm aktif danışmanlara INotificationModuleContract.SendBulkAsync ile gidiyor, o da
// NotificationRecipient(Guid UserId, string Email) bekliyor). CareerAdvisor domain entity'si zaten
// ikisini de tutuyor.
public sealed record ActiveCareerAdvisorSummary(Guid CareerAdvisorId, Guid UserId, string Email);
