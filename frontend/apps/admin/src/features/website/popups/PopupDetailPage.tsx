import {
  activatePopup,
  deactivatePopup,
  deletePopup,
  deletePopupTranslation,
  getApiErrorMessage,
  getPopupById,
  getSiteLanguages,
  updatePopup,
  updatePopupTranslation,
  type PopupDetailResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  CheckboxField,
  ConfirmDialog,
  DateTimeField,
  ErrorState,
  LoadingState,
  PageHeader,
  SelectField,
  StatusBadge,
  TextAreaField,
  TextField,
  TranslationTabs,
  UnsavedChangesGuard,
  useToast,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useMediaPicker } from '../../media/useMediaPicker';
import { LinkTargetField, type LinkTargetValue } from '../shared/LinkTargetField';
import { ROUTES } from '../../../routes';

interface SettingsDraft {
  displayMode: string;
  deviceTarget: string;
  frequency: string;
  frequencyDays: string;
  delaySeconds: number;
  dismissible: boolean;
  priority: number;
  publishAtUtc: string | null;
  unpublishAtUtc: string | null;
  imageMediaId: string | null;
  link: LinkTargetValue | null;
  targetingKind: string;
  targetingContentItemIds: string;
  targetingPaths: string;
}

function draftFromDetail(detail: PopupDetailResponse): SettingsDraft {
  return {
    displayMode: detail.displayMode,
    deviceTarget: detail.deviceTarget,
    frequency: detail.frequency,
    frequencyDays: detail.frequencyDays ? String(detail.frequencyDays) : '',
    delaySeconds: Number(detail.delaySeconds),
    dismissible: detail.dismissible,
    priority: Number(detail.priority),
    publishAtUtc: detail.publishAtUtc,
    unpublishAtUtc: detail.unpublishAtUtc,
    imageMediaId: detail.imageMediaId,
    link: detail.link,
    targetingKind: detail.targeting.kind,
    targetingContentItemIds: detail.targeting.contentItemIds.join('\n'),
    targetingPaths: detail.targeting.paths.join('\n'),
  };
}

export function PopupDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const { pickMedia, pickerElement } = useMediaPicker();

  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['website-popup', id],
    queryFn: () => getPopupById(client, id!),
    enabled: Boolean(id),
  });

  const [draft, setDraft] = useState<SettingsDraft | null>(null);
  const [loadedRowVersion, setLoadedRowVersion] = useState<string | null>(null);
  const [activeLanguage, setActiveLanguage] = useState('');
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const languages = languagesData?.items ?? [];

  if (data && data.rowVersion !== loadedRowVersion) {
    setLoadedRowVersion(data.rowVersion);
    setDraft(draftFromDetail(data));
    if (!activeLanguage && data.translations[0]) setActiveLanguage(data.translations[0].languageCode);
  }

  if (isLoading || !draft || !id) return <LoadingState />;
  if (error || !data) return <ErrorState description={error ? getApiErrorMessage(error) : 'Pop-up bulunamadı.'} />;

  const popupId = id;
  const popup = data;
  // Sabit kopya: TS'in null-daraltması aşağıdaki iç içe fonksiyon bildirimlerine taşınmıyor.
  const settingsDraft = draft;
  const isDirty = JSON.stringify(settingsDraft) !== JSON.stringify(draftFromDetail(popup));

  async function handlePickImage() {
    const asset = await pickMedia();
    if (asset) setDraft({ ...settingsDraft, imageMediaId: asset.id });
  }

  async function handleSaveSettings() {
    setIsSaving(true);
    try {
      await updatePopup(client, popupId, {
        rowVersion: popup.rowVersion,
        displayMode: settingsDraft.displayMode,
        imageMediaId: settingsDraft.imageMediaId,
        link: settingsDraft.link,
        targeting: {
          kind: settingsDraft.targetingKind,
          contentItemIds:
            settingsDraft.targetingKind === 'Contents'
              ? settingsDraft.targetingContentItemIds.split(/[\n,]+/).map((s) => s.trim()).filter(Boolean)
              : null,
          paths:
            settingsDraft.targetingKind === 'Paths'
              ? settingsDraft.targetingPaths.split(/[\n,]+/).map((s) => s.trim()).filter(Boolean)
              : null,
        },
        deviceTarget: settingsDraft.deviceTarget,
        publishAtUtc: settingsDraft.publishAtUtc,
        unpublishAtUtc: settingsDraft.unpublishAtUtc,
        delaySeconds: settingsDraft.delaySeconds,
        frequency: settingsDraft.frequency,
        frequencyDays: settingsDraft.frequency === 'EveryNDays' ? Number(settingsDraft.frequencyDays) : null,
        dismissible: settingsDraft.displayMode === 'Modal' ? true : settingsDraft.dismissible,
        priority: settingsDraft.priority,
      });
      showToast('Ayarlar kaydedildi.', 'success');
      setLoadedRowVersion(null);
      void queryClient.invalidateQueries({ queryKey: ['website-popups'] });
      await refetch();
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
      if ((mutationError as { status?: number }).status === 409) {
        setLoadedRowVersion(null);
        await refetch();
      }
    } finally {
      setIsSaving(false);
    }
  }

  async function handleToggleActive() {
    try {
      if (popup.isActive) await deactivatePopup(client, popupId, popup.rowVersion);
      else await activatePopup(client, popupId, popup.rowVersion);
      showToast(popup.isActive ? 'Pop-up pasifleştirildi.' : 'Pop-up etkinleştirildi.', 'success');
      setLoadedRowVersion(null);
      void queryClient.invalidateQueries({ queryKey: ['website-popups'] });
      await refetch();
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
    }
  }

  async function handleDelete() {
    try {
      await deletePopup(client, popupId);
      showToast('Pop-up silindi.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-popups'] });
      navigate(ROUTES.websitePopups);
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
    }
  }

  return (
    <div className="container section">
      <PageHeader eyebrow="Site yönetimi" title={`Pop-up: ${popup.translations[0]?.title || popup.translations[0]?.body.slice(0, 40) || '(başlıksız)'}`} />
      <UnsavedChangesGuard isDirty={isDirty} />

      <section>
        <h2>Ayarlar</h2>
        <StatusBadge tone={popup.isActive ? 'success' : 'neutral'}>{popup.isActive ? 'Etkin' : 'Pasif'}</StatusBadge>
        <div className="field-row">
          <SelectField id="popup-display-mode" label="Tür" value={draft.displayMode} onChange={(e) => setDraft({ ...draft, displayMode: e.target.value })}>
            <option value="Modal">Modal</option>
            <option value="Banner">Banner</option>
          </SelectField>
          <SelectField id="popup-device" label="Cihaz" value={draft.deviceTarget} onChange={(e) => setDraft({ ...draft, deviceTarget: e.target.value })}>
            <option value="All">Tümü</option>
            <option value="Desktop">Masaüstü</option>
            <option value="Mobile">Mobil</option>
          </SelectField>
          <TextField id="popup-priority" label="Öncelik (0-100)" type="number" value={draft.priority} onChange={(e) => setDraft({ ...draft, priority: Number(e.target.value) })} />
        </div>

        {draft.displayMode === 'Modal' && (
          <div className="field-row">
            <Button type="button" tone="quiet" onClick={() => void handlePickImage()}>
              {draft.imageMediaId ? 'Görseli değiştir' : 'Görsel seç'}
            </Button>
            <TextField id="popup-delay" label="Gecikme (sn, 0-60)" type="number" value={draft.delaySeconds} onChange={(e) => setDraft({ ...draft, delaySeconds: Number(e.target.value) })} />
          </div>
        )}
        {pickerElement}

        <div className="field-row">
          <SelectField id="popup-frequency" label="Gösterim sıklığı" value={draft.frequency} onChange={(e) => setDraft({ ...draft, frequency: e.target.value })}>
            <option value="EveryVisit">Her ziyarette</option>
            <option value="OncePerSession">Oturumda bir kez</option>
            <option value="EveryNDays">N günde bir</option>
          </SelectField>
          {draft.frequency === 'EveryNDays' && (
            <TextField id="popup-frequency-days" label="Gün sayısı (1-365)" type="number" value={draft.frequencyDays} onChange={(e) => setDraft({ ...draft, frequencyDays: e.target.value })} />
          )}
          {draft.displayMode === 'Banner' && (
            <CheckboxField id="popup-dismissible" label="Kapatılabilir" checked={draft.dismissible} onChange={(e) => setDraft({ ...draft, dismissible: e.target.checked })} />
          )}
        </div>

        <div className="field-row">
          <DateTimeField id="popup-publish-at" label="Yayın başlangıcı" value={draft.publishAtUtc} onChange={(v) => setDraft({ ...draft, publishAtUtc: v })} />
          <DateTimeField id="popup-unpublish-at" label="Yayın bitişi" value={draft.unpublishAtUtc} onChange={(v) => setDraft({ ...draft, unpublishAtUtc: v })} />
        </div>

        <h3>Hedefleme</h3>
        <SelectField id="popup-targeting-kind" label="Kapsam" value={draft.targetingKind} onChange={(e) => setDraft({ ...draft, targetingKind: e.target.value })}>
          <option value="AllPages">Tüm sayfalar</option>
          <option value="HomeOnly">Yalnız ana sayfa</option>
          <option value="Contents">Belirli içerikler</option>
          <option value="Paths">Belirli yollar</option>
        </SelectField>
        {draft.targetingKind === 'Contents' && (
          <TextAreaField
            id="popup-targeting-contents"
            label="İçerik ID'leri"
            hint="Her satıra veya virgülle ayırarak bir içerik kimliği (GUID) girin."
            value={draft.targetingContentItemIds}
            onChange={(e) => setDraft({ ...draft, targetingContentItemIds: e.target.value })}
          />
        )}
        {draft.targetingKind === 'Paths' && (
          <TextAreaField
            id="popup-targeting-paths"
            label="Yollar"
            hint="Her satıra veya virgülle ayırarak bir yol girin (ör. /iletisim)."
            value={draft.targetingPaths}
            onChange={(e) => setDraft({ ...draft, targetingPaths: e.target.value })}
          />
        )}

        <LinkTargetField idPrefix="popup" value={draft.link} onChange={(link) => setDraft({ ...draft, link })} />

        <div className="field-row">
          <Button type="button" onClick={() => void handleSaveSettings()} disabled={!isDirty || isSaving}>
            {isSaving ? 'Kaydediliyor…' : 'Ayarları kaydet'}
          </Button>
          <Button type="button" tone="quiet" onClick={() => void handleToggleActive()}>
            {popup.isActive ? 'Pasifleştir' : 'Etkinleştir'}
          </Button>
          <Button type="button" tone="danger" onClick={() => setIsDeleteOpen(true)}>
            Sil
          </Button>
        </div>
      </section>

      <section>
        <h2>Metinler (dil bazlı)</h2>
        <TranslationTabs
          languages={languages}
          activeCode={activeLanguage}
          onChange={setActiveLanguage}
          renderPanel={(languageCode) => (
            <PopupTranslationPanel
              key={languageCode}
              languageCode={languageCode}
              popupId={popupId}
              rowVersion={popup.rowVersion}
              initial={popup.translations.find((t) => t.languageCode === languageCode) ?? { languageCode, title: '', body: '', buttonLabel: null }}
              isDefaultLanguage={languages[0]?.code === languageCode}
              onSaved={() => {
                setLoadedRowVersion(null);
                void refetch();
              }}
            />
          )}
        />
      </section>

      <ConfirmDialog
        open={isDeleteOpen}
        title="Pop-up'ı sil"
        description="Bu pop-up kalıcı olarak silinecek."
        onConfirm={() => {
          setIsDeleteOpen(false);
          void handleDelete();
        }}
        onCancel={() => setIsDeleteOpen(false)}
      />
    </div>
  );
}

function PopupTranslationPanel({
  languageCode,
  popupId,
  rowVersion,
  initial,
  isDefaultLanguage,
  onSaved,
}: {
  languageCode: string;
  popupId: string;
  rowVersion: string;
  initial: { languageCode: string; title: string | null; body: string; buttonLabel: string | null };
  isDefaultLanguage: boolean;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [title, setTitle] = useState(initial.title ?? '');
  const [body, setBody] = useState(initial.body);
  const [buttonLabel, setButtonLabel] = useState(initial.buttonLabel ?? '');
  const isDirty = title !== (initial.title ?? '') || body !== initial.body || buttonLabel !== (initial.buttonLabel ?? '');

  const saveMutation = useMutation({
    mutationFn: () =>
      updatePopupTranslation(client, popupId, languageCode, { rowVersion, title: title || null, body, buttonLabel: buttonLabel || null }),
    onSuccess: () => {
      showToast('Metin kaydedildi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deletePopupTranslation(client, popupId, languageCode, rowVersion),
    onSuccess: () => {
      showToast('Çeviri silindi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  return (
    <div>
      <TextField id={`popup-title-${languageCode}`} label="Başlık" value={title} onChange={(e) => setTitle(e.target.value)} />
      <TextAreaField id={`popup-body-${languageCode}`} label="Metin" required value={body} onChange={(e) => setBody(e.target.value)} />
      <TextField id={`popup-button-label-${languageCode}`} label="Buton etiketi" value={buttonLabel} onChange={(e) => setButtonLabel(e.target.value)} />
      <div className="field-row">
        <Button type="button" onClick={() => saveMutation.mutate()} disabled={!isDirty || !body.trim() || saveMutation.isPending}>
          Bu dili kaydet
        </Button>
        {!isDefaultLanguage && (
          <Button type="button" tone="danger" onClick={() => deleteMutation.mutate()} disabled={deleteMutation.isPending}>
            Bu dili sil
          </Button>
        )}
      </div>
    </div>
  );
}
