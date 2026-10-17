import {
  addFormSubmissionNote,
  archiveFormSubmission,
  changeFormSubmissionStatus,
  downloadFormSubmissionFile,
  FormSubmissionStatus,
  getApiErrorMessage,
  getFormSubmissionById,
  unarchiveFormSubmission,
  type FormSubmissionDetailResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  buttonClassName,
  DataTable,
  downloadBlob,
  ErrorState,
  formatDateTime,
  LoadingState,
  PageHeader,
  StatusBadge,
  TextAreaField,
  useToast,
  type DataTableColumn,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getEnumLabel } from '../../../i18n/getEnumLabel';
import { ROUTES } from '../../../routes';
import { AssignFormSubmissionDialog } from './AssignFormSubmissionDialog';
import { FormSubmissionAnswerValue } from './FormSubmissionAnswerValue';

// Backend invariant'ının (FormSubmission.AllowedTransitions, src/Modules/Website/Domain/
// FormSubmission.cs) birebir aynısı - asıl doğrulama her zaman backend'dedir, bu yalnız hangi
// düğmelerin gösterileceğini belirler.
const ALLOWED_NEXT_STATUSES: Record<string, string[]> = {
  [FormSubmissionStatus.New]: [FormSubmissionStatus.InReview],
  [FormSubmissionStatus.InReview]: [FormSubmissionStatus.AwaitingInfo, FormSubmissionStatus.Approved, FormSubmissionStatus.Rejected],
  [FormSubmissionStatus.AwaitingInfo]: [FormSubmissionStatus.InReview],
  [FormSubmissionStatus.Approved]: [FormSubmissionStatus.Completed],
  [FormSubmissionStatus.Rejected]: [FormSubmissionStatus.InReview],
  [FormSubmissionStatus.Completed]: [FormSubmissionStatus.InReview],
};

const CLOSING_STATUSES: readonly string[] = [FormSubmissionStatus.Rejected, FormSubmissionStatus.Completed];

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function FormSubmissionDetailPage() {
  const { submissionId } = useParams<{ submissionId: string }>();
  const { client } = useAuth();
  const { data } = useQuery({
    queryKey: ['website-form-submission', submissionId],
    queryFn: () => getFormSubmissionById(client, submissionId!),
    enabled: Boolean(submissionId),
    staleTime: 0,
    gcTime: 30_000,
  });

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Form başvuruları"
        title={data ? `Başvuru ${data.referenceNumber}` : 'Başvuru'}
        description={data ? `Form: ${data.formKey} · Gönderim: ${formatDateTime(data.submittedAtUtc)}` : undefined}
        actions={
          <Link to={ROUTES.websiteFormSubmissions} className={buttonClassName('quiet')}>
            Listeye dön
          </Link>
        }
      />
      <FormSubmissionDetailEditor />
    </div>
  );
}

// `PageHeader`'ın (sayfanın tek h1'i) üstteki sarmalayıcıda KOŞULSUZ render edilmesi gerekir
// (bkz. `FormDefinitionDetailPage`'in aynı notu / `SliderDetailPage`'in kaynağı).
function FormSubmissionDetailEditor() {
  const { submissionId } = useParams<{ submissionId: string }>();
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [isAssignOpen, setIsAssignOpen] = useState(false);
  const [noteText, setNoteText] = useState('');
  const [downloadingFileId, setDownloadingFileId] = useState<string | null>(null);

  const queryKey = ['website-form-submission', submissionId] as const;
  const { data: detail, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => getFormSubmissionById(client, submissionId!),
    enabled: Boolean(submissionId),
    staleTime: 0,
    gcTime: 30_000,
  });

  function onChanged() {
    void queryClient.invalidateQueries({ queryKey });
    void queryClient.invalidateQueries({ queryKey: ['website-form-submissions'] });
  }

  const statusMutation = useMutation({
    mutationFn: (status: string) => changeFormSubmissionStatus(client, submissionId!, { rowVersion: detail!.rowVersion, status }),
    onSuccess: () => {
      showToast('Durum güncellendi.', 'success');
      onChanged();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const noteMutation = useMutation({
    mutationFn: () => addFormSubmissionNote(client, submissionId!, { rowVersion: detail!.rowVersion, text: noteText }),
    onSuccess: () => {
      showToast('Not eklendi.', 'success');
      setNoteText('');
      onChanged();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const archiveMutation = useMutation({
    mutationFn: () => archiveFormSubmission(client, submissionId!, { rowVersion: detail!.rowVersion }),
    onSuccess: () => {
      showToast('Başvuru arşivlendi.', 'success');
      onChanged();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const unarchiveMutation = useMutation({
    mutationFn: () => unarchiveFormSubmission(client, submissionId!, { rowVersion: detail!.rowVersion }),
    onSuccess: () => {
      showToast('Başvuru arşivden çıkarıldı.', 'success');
      onChanged();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  async function handleDownload(fileId: string, originalFileName: string) {
    setDownloadingFileId(fileId);
    try {
      const { blob } = await downloadFormSubmissionFile(client, submissionId!, fileId);
      downloadBlob(blob, originalFileName);
    } catch (downloadError) {
      showToast(getApiErrorMessage(downloadError), 'danger');
    } finally {
      setDownloadingFileId(null);
    }
  }

  if (!submissionId) return <ErrorState description="Başvuru kimliği eksik." />;
  if (isLoading) return <LoadingState />;
  if (error || !detail) return <ErrorState description={error ? getApiErrorMessage(error) : 'Başvuru bulunamadı.'} />;

  const isAnonymized = Boolean(detail.anonymizedAtUtc);
  const isArchived = Boolean(detail.archivedAtUtc);
  const isMutable = !isAnonymized && !isArchived;
  const canArchive = isMutable && CLOSING_STATUSES.includes(detail.status);
  const nextStatuses = isMutable ? ALLOWED_NEXT_STATUSES[detail.status] ?? [] : [];

  const fileColumns: DataTableColumn<FormSubmissionDetailResponse['files'][number]>[] = [
    { key: 'fieldKey', header: 'Alan', render: (row) => row.fieldKey },
    { key: 'originalFileName', header: 'Dosya adı', render: (row) => row.originalFileName },
    { key: 'contentType', header: 'Tür', render: (row) => row.contentType },
    { key: 'sizeInBytes', header: 'Boyut', render: (row) => formatFileSize(Number(row.sizeInBytes)) },
  ];

  return (
    <div>
      {isAnonymized && (
        <p className="field-hint">Bu başvuru kişisel verisi anonimleştirilmiştir (saklama süresi doldu); artık yönetilemez.</p>
      )}
      {isArchived && !isAnonymized && <p className="field-hint">Bu başvuru arşivlenmiş; arşivden çıkarılmadan düzenlenemez.</p>}

      <section>
        <h2>Durum</h2>
        <div className="field-row" style={{ alignItems: 'center' }}>
          <StatusBadge tone="neutral">{getEnumLabel('FormSubmissionStatus', detail.status)}</StatusBadge>
          {nextStatuses.map((status) => (
            <Button key={status} type="button" onClick={() => statusMutation.mutate(status)} disabled={statusMutation.isPending}>
              {getEnumLabel('FormSubmissionStatus', status)} yap
            </Button>
          ))}
        </div>
        <div className="field-row" style={{ alignItems: 'center' }}>
          <span>Atanan: {detail.assignedToUserId ?? 'Atanmamış'}</span>
          <Button type="button" tone="quiet" onClick={() => setIsAssignOpen(true)} disabled={isAnonymized}>
            Ata
          </Button>
          {canArchive && (
            <Button type="button" tone="secondary" onClick={() => archiveMutation.mutate()} disabled={archiveMutation.isPending}>
              Arşivle
            </Button>
          )}
          {isArchived && !isAnonymized && (
            <Button type="button" onClick={() => unarchiveMutation.mutate()} disabled={unarchiveMutation.isPending}>
              Arşivden çıkar
            </Button>
          )}
        </div>
      </section>

      <section>
        <h2>Alan yanıtları</h2>
        <table className="data-table">
          <caption className="sr-only">Alan yanıtları</caption>
          <thead>
            <tr>
              <th scope="col">Alan</th>
              <th scope="col">Yanıt</th>
            </tr>
          </thead>
          <tbody>
            {detail.answers.map((answer) => (
              <tr key={answer.fieldKey}>
                <th scope="row">{answer.label}</th>
                <td>
                  <FormSubmissionAnswerValue value={answer.value} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      {detail.files.length > 0 && (
        <section>
          <h2>Dosyalar</h2>
          <DataTable
            caption="Dosyalar"
            columns={fileColumns}
            rows={detail.files}
            getRowKey={(row) => row.fileId}
            rowActions={(row) => (
              <Button type="button" tone="quiet" onClick={() => void handleDownload(row.fileId, row.originalFileName)} disabled={downloadingFileId === row.fileId}>
                {downloadingFileId === row.fileId ? 'İndiriliyor…' : 'İndir'}
              </Button>
            )}
          />
        </section>
      )}

      <section>
        <h2>Kabul edilen yasal metinler</h2>
        {detail.acceptedLegalVersions.length === 0 ? (
          <p className="field-hint">Yok.</p>
        ) : (
          <ul>
            {detail.acceptedLegalVersions.map((version) => (
              <li key={version.legalDocumentKey}>
                {version.legalDocumentKey} - v{version.versionNumber} {version.isPrivacyNotice ? '(aydınlatma metni)' : '(açık rıza)'}
              </li>
            ))}
          </ul>
        )}
      </section>

      <section>
        <h2>Durum geçmişi</h2>
        <table className="data-table">
          <caption className="sr-only">Durum geçmişi</caption>
          <thead>
            <tr>
              <th scope="col">Tarih</th>
              <th scope="col">Değişiklik</th>
              <th scope="col">Değiştiren</th>
            </tr>
          </thead>
          <tbody>
            {detail.statusHistory.map((entry, index) => (
              <tr key={index}>
                <td>{formatDateTime(entry.changedAtUtc)}</td>
                <td>
                  {getEnumLabel('FormSubmissionStatus', entry.fromStatus)} → {getEnumLabel('FormSubmissionStatus', entry.toStatus)}
                </td>
                <td>{entry.changedByUserId}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section>
        <h2>İç notlar</h2>
        {detail.internalNotes.length === 0 ? (
          <p className="field-hint">Henüz not yok.</p>
        ) : (
          <ul>
            {detail.internalNotes.map((note) => (
              <li key={note.id}>
                <p>{note.text}</p>
                <span className="field-hint">
                  {formatDateTime(note.createdAtUtc)} · {note.authorUserId}
                </span>
              </li>
            ))}
          </ul>
        )}
        <TextAreaField id="form-submission-note" label="Yeni not" value={noteText} onChange={(event) => setNoteText(event.target.value)} disabled={!isMutable} />
        <Button type="button" onClick={() => noteMutation.mutate()} disabled={!isMutable || !noteText.trim() || noteMutation.isPending}>
          {noteMutation.isPending ? 'Ekleniyor…' : 'Not ekle'}
        </Button>
      </section>

      <AssignFormSubmissionDialog
        open={isAssignOpen}
        submissionId={submissionId}
        rowVersion={detail.rowVersion}
        currentAssignedToUserId={detail.assignedToUserId}
        onClose={() => setIsAssignOpen(false)}
      />
    </div>
  );
}

export default FormSubmissionDetailPage;
