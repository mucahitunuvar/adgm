namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 5): "hangi işlem (görüntüleme, dosya indirme, dışa aktarma)".
public enum PersonalDataAccessAction
{
    View,
    DownloadFile,
    Export,
}
