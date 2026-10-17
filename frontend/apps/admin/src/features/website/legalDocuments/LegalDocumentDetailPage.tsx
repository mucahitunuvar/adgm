import {
  LegalDocumentVersionStatus,
  createLegalDocumentDraft,
  deleteLegalDocument,
  deleteLegalDocumentDraft,
  getApiErrorMessage,
  getLegalDocumentById,
  getSiteLanguages,
  publishLegalDocumentDraft,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  ConfirmDialog,
  ErrorState,
  FormSection,
  LoadingState,
  PageHeader,
  StatusBadge,
  TextField,
  TranslationTabs,
  useToast,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getEnumLabel } from '../../../i18n/getEnumLabel';
import { ROUTES } from '../../../routes';
import { LegalDocumentDraftBodyPanel } from './LegalDocumentDraftBodyPanel';
import { LegalDocumentTitlePanel } from './LegalDocumentTitlePanel';
import { LegalDocumentVersionHistory } from './LegalDocumentVersionHistory';
import { PublishLegalDocumentDraftDialog } from './PublishLegalDocumentDraftDialog';

export function LegalDocumentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { client } = useAuth();
  const { data } = useQuery({
    queryKey: ['website-legal-document', id],
    queryFn: () => getLegalDocumentById(client, id!),
    enabled: Boolean(id),
  });

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title={data ? `Yasal metin: ${data.key}` : 'Yasal metin'}
        description={data ? getEnumLabel('LegalDocumentKind', data.kind) : undefined}
      />
      <LegalDocumentDetailEditor />
    </div>
  );
}

// Üstteki bileşenin kendi ayrı `useQuery`si yalnız başlık için - aynı `queryKey` sayesinde ek bir ağ
// isteği değil (bkz. SliderDetailPage.tsx'teki aynı not); `PageHeader` böylece yükleme/hata durumunda
// bile koşulsuz render edilir (axe `page-has-heading-one`).
function LegalDocumentDetailEditor() {
  const { id } = useParams<{ id: string }>();
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['website-legal-document', id],
    queryFn: () => getLegalDocumentById(client, id!),
    enabled: Boolean(id),
  });

  const [activeTitleLanguage, setActiveTitleLanguage] = useState('');
  const [changeSummary, setChangeSummary] = useState('');
  const [activeDraftLanguage, setActiveDraftLanguage] = useState('');
  const [isPublishOpen, setIsPublishOpen] = useState(false);
  const [isDiscardDraftOpen, setIsDiscardDraftOpen] = useState(false);
  const [isDeleteDocumentOpen, setIsDeleteDocumentOpen] = useState(false);

  const languages = languagesData?.items ?? [];

  function onChanged() {
    void queryClient.invalidateQueries({ queryKey: ['website-legal-documents'] });
    void refetch();
  }

  const createDraftMutation = useMutation({
    mutationFn: () => {
      if (!data || !id) throw new Error('Henüz yüklenmedi.');
      return createLegalDocumentDraft(client, id, { rowVersion: data.rowVersion, changeSummary: changeSummary || null });
    },
    onSuccess: () => {
      showToast('Taslak oluşturuldu.', 'success');
      setChangeSummary('');
      onChanged();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const discardDraftMutation = useMutation({
    mutationFn: () => {
      if (!data || !id) throw new Error('Henüz yüklenmedi.');
      return deleteLegalDocumentDraft(client, id, { rowVersion: data.rowVersion });
    },
    onSuccess: () => {
      showToast('Taslak silindi.', 'success');
      setIsDiscardDraftOpen(false);
      onChanged();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const publishMutation = useMutation({
    mutationFn: (effectiveAtUtc: string | null) => {
      if (!data || !id) throw new Error('Henüz yüklenmedi.');
      return publishLegalDocumentDraft(client, id, { rowVersion: data.rowVersion, effectiveAtUtc });
    },
    onSuccess: () => {
      showToast('Yeni sürüm yürürlüğe girdi.', 'success');
      setIsPublishOpen(false);
      onChanged();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const deleteDocumentMutation = useMutation({
    mutationFn: () => {
      if (!id) throw new Error('Henüz yüklenmedi.');
      return deleteLegalDocument(client, id);
    },
    onSuccess: () => {
      showToast('Yasal metin silindi.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-legal-documents'] });
      navigate(ROUTES.websiteLegalDocuments);
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  if (isLoading || !id) return <LoadingState />;
  if (error || !data) return <ErrorState description={error ? getApiErrorMessage(error) : 'Yasal metin bulunamadı.'} />;

  const documentId = id;
  const draftVersion = data.versions.find((version) => version.status === LegalDocumentVersionStatus.Draft);
  const effectiveVersion = data.versions.find((version) => version.versionNumber === data.effectiveVersionNumber);

  if (!activeTitleLanguage && languages[0]) setActiveTitleLanguage(languages[0].code);
  if (!activeDraftLanguage && languages[0]) setActiveDraftLanguage(languages[0].code);

  return (
    <div>
      <section>
        <h2>Durum</h2>
        <p>
          {effectiveVersion ? (
            <StatusBadge tone="success">{`Yürürlükte: v${effectiveVersion.versionNumber}`}</StatusBadge>
          ) : (
            <StatusBadge tone="warning">Yayınlı sürüm yok</StatusBadge>
          )}{' '}
          {draftVersion && <StatusBadge tone="info">{`Taslak: v${draftVersion.versionNumber}`}</StatusBadge>}
        </p>
      </section>

      <FormSection title="Başlık (dil bazlı)" description="Her dilin kendi kaydet düğmesi vardır. Bu başlık sürümden bağımsızdır.">
        <TranslationTabs
          languages={languages}
          activeCode={activeTitleLanguage}
          onChange={setActiveTitleLanguage}
          renderPanel={(code) => (
            <LegalDocumentTitlePanel
              key={code}
              documentId={documentId}
              languageCode={code}
              rowVersion={data.rowVersion}
              initialTitle={data.translations.find((t) => t.languageCode === code)?.title ?? ''}
              onSaved={onChanged}
            />
          )}
        />
      </FormSection>

      <FormSection
        title="Taslak"
        description="Aynı anda en fazla bir taslak sürüm olabilir; yeni taslak, yürürlükteki sürümün metinleri kopyalanarak başlar."
      >
        {!draftVersion ? (
          <div>
            <TextField
              id="legal-document-change-summary"
              label="Değişiklik özeti"
              hint="İsteğe bağlı; sürüm geçmişinde görünür."
              value={changeSummary}
              onChange={(event) => setChangeSummary(event.target.value)}
            />
            <Button type="button" onClick={() => createDraftMutation.mutate()} disabled={createDraftMutation.isPending}>
              {createDraftMutation.isPending ? 'Oluşturuluyor…' : 'Yeni taslak oluştur'}
            </Button>
          </div>
        ) : (
          <div>
            <TranslationTabs
              languages={languages}
              activeCode={activeDraftLanguage}
              onChange={setActiveDraftLanguage}
              renderPanel={(code) => (
                <LegalDocumentDraftBodyPanel
                  key={code}
                  documentId={documentId}
                  languageCode={code}
                  rowVersion={data.rowVersion}
                  savedBody={draftVersion.translations.find((t) => t.languageCode === code)?.body ?? ''}
                  onSaved={onChanged}
                />
              )}
            />
            <div className="field-row">
              <Button type="button" onClick={() => setIsPublishOpen(true)}>
                Yayınla
              </Button>
              <Button type="button" tone="danger" onClick={() => setIsDiscardDraftOpen(true)}>
                Taslağı sil
              </Button>
            </div>
          </div>
        )}
      </FormSection>

      <FormSection title="Sürüm geçmişi">
        <LegalDocumentVersionHistory versions={data.versions} />
      </FormSection>

      <section>
        <Button type="button" tone="danger" onClick={() => setIsDeleteDocumentOpen(true)}>
          Yasal metni sil
        </Button>
      </section>

      <PublishLegalDocumentDraftDialog
        open={isPublishOpen}
        versionNumber={draftVersion?.versionNumber ?? null}
        isSubmitting={publishMutation.isPending}
        onConfirm={(effectiveAtUtc) => publishMutation.mutate(effectiveAtUtc)}
        onCancel={() => setIsPublishOpen(false)}
      />
      <ConfirmDialog
        open={isDiscardDraftOpen}
        title="Taslağı sil"
        description="Bu taslak kalıcı olarak silinecek; yürürlükteki sürüm değişmeden kalır."
        isConfirming={discardDraftMutation.isPending}
        onConfirm={() => discardDraftMutation.mutate()}
        onCancel={() => setIsDiscardDraftOpen(false)}
      />
      <ConfirmDialog
        open={isDeleteDocumentOpen}
        title="Yasal metni sil"
        description="Bu yasal metin ve tüm sürümleri kalıcı olarak silinecek. Bu belge bir ayarda (bülten/etkinlik/çerez) seçiliyse o ayar geçersiz kalır. Backend, yayınlanmış bir sürümü olan belgelerin silinmesini reddedebilir."
        isConfirming={deleteDocumentMutation.isPending}
        onConfirm={() => deleteDocumentMutation.mutate()}
        onCancel={() => setIsDeleteDocumentOpen(false)}
      />
    </div>
  );
}

export default LegalDocumentDetailPage;
