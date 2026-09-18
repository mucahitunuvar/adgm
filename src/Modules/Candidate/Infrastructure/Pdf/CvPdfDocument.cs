using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Pdf;

// QuestPDF IDocument implementation (ADR-021) - the fixed CV template. Section order follows
// Candidate.md: Header -> Kişisel Bilgiler -> Özet -> Deneyim -> Eğitim -> Bilgisayar Bilgisi ->
// Diller -> Sertifikalar -> Referanslar -> Hobiler. Every section but the header is omitted
// entirely when it has nothing to show ("boş bölüm kuralı") - no empty headings. Net Maaş
// Beklentisi is deliberately absent from CandidateCvPdfModel itself (not just unrendered here) so
// this class has no way to leak it even by mistake.
public sealed class CvPdfDocument(CandidateCvPdfModel model) : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30, Unit.Point);
            page.Size(PageSizes.A4);
            page.DefaultTextStyle(style => style.FontSize(10));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            if (model.PhotoBytes is { Length: > 0 })
            {
                row.ConstantItem(70, Unit.Point).Height(90, Unit.Point).Image(model.PhotoBytes).FitArea();
            }

            row.RelativeItem().PaddingLeft(model.PhotoBytes is { Length: > 0 } ? 15 : 0, Unit.Point).Column(column =>
            {
                column.Item().Text(model.FullName).FontSize(20).Bold();

                if (!string.IsNullOrWhiteSpace(model.Title))
                {
                    column.Item().Text(model.Title).FontSize(12).FontColor(Colors.Grey.Darken2);
                }

                column.Item().PaddingTop(4, Unit.Point).Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontSize(9));
                    text.Span(model.Email);

                    if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
                    {
                        text.Span("   |   " + model.PhoneNumber);
                    }
                });

                var locationLine = string.Join(
                    ", ", new[] { model.Address, model.DistrictName, model.ProvinceName }.Where(p => !string.IsNullOrWhiteSpace(p)));

                if (!string.IsNullOrWhiteSpace(locationLine))
                {
                    column.Item().Text(locationLine).FontSize(9);
                }

                if (model.SocialMediaLinks.Count > 0)
                {
                    var socialLine = string.Join("   ", model.SocialMediaLinks.Select(link => $"{link.Platform}: {link.Url}"));
                    column.Item().Text(socialLine).FontSize(9);
                }
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(15, Unit.Point).Column(column =>
        {
            column.Spacing(12, Unit.Point);

            ComposePersonalInfo(column);

            if (!string.IsNullOrWhiteSpace(model.Summary))
            {
                ComposeSection(column, "Özet", body => body.Item().Text(model.Summary!));
            }

            if (model.Experiences.Count > 0)
            {
                ComposeSection(column, "Deneyim", body =>
                {
                    foreach (var experience in model.Experiences.OrderByDescending(e => e.StartDate))
                    {
                        ComposeExperience(body, experience);
                    }
                });
            }

            if (model.Educations.Count > 0)
            {
                ComposeSection(column, "Eğitim", body =>
                {
                    foreach (var education in model.Educations.OrderByDescending(e => e.StartDate))
                    {
                        ComposeEducation(body, education);
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(model.ComputerSkills))
            {
                ComposeSection(column, "Bilgisayar Bilgisi", body => body.Item().Text(model.ComputerSkills!));
            }

            if (model.Languages.Count > 0)
            {
                ComposeSection(column, "Diller", body =>
                {
                    foreach (var language in model.Languages)
                    {
                        var suffix = language.IsNativeLanguage ? " (Anadil)" : string.Empty;
                        body.Item().Text($"{language.LanguageName} - {language.LanguageLevelName}{suffix}");
                    }
                });
            }

            if (model.Certificates.Count > 0)
            {
                ComposeSection(column, "Sertifikalar", body =>
                {
                    foreach (var certificate in model.Certificates)
                    {
                        ComposeCertificate(body, certificate);
                    }
                });
            }

            if (model.References.Count > 0)
            {
                ComposeSection(column, "Referanslar", body =>
                {
                    foreach (var reference in model.References)
                    {
                        ComposeReference(body, reference);
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(model.Hobbies))
            {
                ComposeSection(column, "Hobiler", body => body.Item().Text(model.Hobbies!));
            }
        });
    }

    // Kişisel Bilgiler'in kendisi başlıksız bir bölüm değildir: dolu tek bir alan bile olsa listeye
    // eklenir; hepsi boşsa (nadiren - Doğum Tarihi hariç hepsi opsiyonel) bölüm hiç görünmez.
    private void ComposePersonalInfo(ColumnDescriptor column)
    {
        var lines = new List<string>();

        if (model.BirthDate is not null)
        {
            lines.Add($"Doğum Tarihi: {model.BirthDate:dd.MM.yyyy}");
        }

        if (!string.IsNullOrWhiteSpace(model.GenderName))
        {
            lines.Add($"Cinsiyet: {model.GenderName}");
        }

        if (!string.IsNullOrWhiteSpace(model.NationalityName))
        {
            lines.Add($"Uyruk: {model.NationalityName}");
        }

        if (!string.IsNullOrWhiteSpace(model.DriversLicenseTypeName))
        {
            lines.Add($"Sürücü Belgesi: {model.DriversLicenseTypeName}");
        }

        if (!string.IsNullOrWhiteSpace(model.MilitaryStatusName))
        {
            lines.Add($"Askerlik Durumu: {model.MilitaryStatusName}");
        }

        if (lines.Count == 0 && model.DisabilityInfo is null)
        {
            return;
        }

        ComposeSection(column, "Kişisel Bilgiler", body =>
        {
            foreach (var line in lines)
            {
                body.Item().Text(line);
            }

            if (model.DisabilityInfo is { } disability)
            {
                ComposeDisabilityInfo(body, disability);
            }
        });
    }

    private static void ComposeDisabilityInfo(ColumnDescriptor body, CandidateCvPdfDisabilityInfo disability)
    {
        body.Item().PaddingTop(4, Unit.Point).Text("Engellilik Bilgisi").Bold();
        body.Item().Text($"Kategori: {disability.CategoryName} ({disability.Percentage}%)");

        if (!string.IsNullOrWhiteSpace(disability.Description))
        {
            body.Item().Text(disability.Description);
        }

        var flags = new List<string>();
        if (disability.HasHealthReport)
        {
            flags.Add("Sağlık raporu var");
        }

        if (disability.UsesMedication)
        {
            flags.Add("İlaç kullanımı var");
        }

        if (disability.HasChronicCondition)
        {
            flags.Add("Kronik rahatsızlık var");
        }

        if (disability.HasContagiousDisease)
        {
            flags.Add("Bulaşıcı hastalık var");
        }

        if (disability.HasConsciousnessLossRisk)
        {
            flags.Add("Bilinç kaybı riski var");
        }

        if (flags.Count > 0)
        {
            body.Item().Text(string.Join(", ", flags)).FontSize(9);
        }
    }

    private static void ComposeExperience(ColumnDescriptor body, CandidateCvPdfExperience experience)
    {
        body.Item().Column(item =>
        {
            item.Item().Text(text =>
            {
                text.Span(experience.CompanyName).Bold();

                if (!string.IsNullOrWhiteSpace(experience.PositionName))
                {
                    text.Span($" - {experience.PositionName}");
                }
            });

            var endLabel = experience.IsCurrentJob ? "Halen" : experience.EndDate?.ToString("dd.MM.yyyy") ?? "-";
            item.Item().Text($"{experience.StartDate:dd.MM.yyyy} - {endLabel}").FontSize(9).FontColor(Colors.Grey.Darken2);

            var detailLine = string.Join(
                ", ",
                new[] { experience.SectorName, experience.WorkFieldName, experience.EmploymentTypeName, experience.ProvinceName, experience.CountryName }
                    .Where(p => !string.IsNullOrWhiteSpace(p)));

            if (!string.IsNullOrWhiteSpace(detailLine))
            {
                item.Item().Text(detailLine).FontSize(9);
            }

            if (!string.IsNullOrWhiteSpace(experience.JobDescription))
            {
                item.Item().Text(experience.JobDescription);
            }
        });
    }

    private static void ComposeEducation(ColumnDescriptor body, CandidateCvPdfEducation education)
    {
        body.Item().Column(item =>
        {
            item.Item().Text(education.EducationLevelName).Bold();

            var endLabel = education.CompletionStatus switch
            {
                "Continuing" => "Devam Ediyor",
                "Dropped" => "Terk",
                _ => education.EndDate?.ToString("dd.MM.yyyy") ?? "-",
            };
            item.Item().Text($"{education.StartDate:dd.MM.yyyy} - {endLabel}").FontSize(9).FontColor(Colors.Grey.Darken2);

            if (!string.IsNullOrWhiteSpace(education.SchoolName))
            {
                item.Item().Text(education.SchoolName!);
            }

            var detailLine = string.Join(
                ", ",
                new[] { education.ProvinceName, education.DiplomaGradingSystemName is null ? null : $"{education.DiplomaGradingSystemName}: {education.DiplomaGrade}" }
                    .Where(p => !string.IsNullOrWhiteSpace(p)));

            if (!string.IsNullOrWhiteSpace(detailLine))
            {
                item.Item().Text(detailLine).FontSize(9);
            }

            if (!string.IsNullOrWhiteSpace(education.Description))
            {
                item.Item().Text(education.Description!);
            }
        });
    }

    private static void ComposeCertificate(ColumnDescriptor body, CandidateCvPdfCertificate certificate)
    {
        body.Item().Column(item =>
        {
            item.Item().Text(text =>
            {
                text.Span(certificate.Name).Bold();
                text.Span($" - {certificate.IssuingInstitution}");
            });

            if (certificate.CertificateDate is not null)
            {
                item.Item().Text(certificate.CertificateDate.Value.ToString("dd.MM.yyyy")).FontSize(9).FontColor(Colors.Grey.Darken2);
            }

            if (!string.IsNullOrWhiteSpace(certificate.Description))
            {
                item.Item().Text(certificate.Description!);
            }
        });
    }

    private static void ComposeReference(ColumnDescriptor body, CandidateCvPdfReference reference)
    {
        body.Item().Column(item =>
        {
            item.Item().Text(text =>
            {
                text.Span($"{reference.FirstName} {reference.LastName}").Bold();

                if (!string.IsNullOrWhiteSpace(reference.Position) || !string.IsNullOrWhiteSpace(reference.Company))
                {
                    var role = string.Join(", ", new[] { reference.Position, reference.Company }.Where(p => !string.IsNullOrWhiteSpace(p)));
                    text.Span($" - {role}");
                }
            });

            item.Item().Text($"{reference.ReferenceTypeName} • {reference.ReferenceLanguageName}").FontSize(9).FontColor(Colors.Grey.Darken2);

            var contactLine = string.Join(", ", new[] { reference.Email, reference.PhoneNumber }.Where(p => !string.IsNullOrWhiteSpace(p)));

            if (!string.IsNullOrWhiteSpace(contactLine))
            {
                item.Item().Text(contactLine).FontSize(9);
            }
        });
    }

    private static void ComposeSection(ColumnDescriptor column, string title, Action<ColumnDescriptor> composeBody)
    {
        column.Item().Column(section =>
        {
            section.Spacing(4, Unit.Point);
            section.Item().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten1)
                .PaddingBottom(2, Unit.Point).Text(title).FontSize(13).Bold();
            section.Item().Column(composeBody);
        });
    }
}
