import {
  archiveContentItem,
  createContentPreviewLink,
  deleteContentItem,
  duplicateContentItem,
  getApiErrorMessage,
  publishContentItem,
  scheduleContentItem,
  unarchiveContentItem,
  unpublishContentItem,
  type ContentItemDetailResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, ConfirmDialog, TextField, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../../routes';

/** `datetime-local` girişi (yerel saat) <-> backend'in beklediği UTC ISO dizgisi. */
function utcToLocalInputValue(utc: string | null): string {
  if (!utc) return '';
  const date = new Date(utc);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function localInputValueToUtc(local: string): string | null {
  if (!local) return null;
  return new Date(local).toISOString();
}

export function ContentItemLifecycleActions({
  contentItemId,
  detail,
  onChanged,
}: {
  contentItemId: string;
  detail: ContentItemDetailResponse;
  onChanged: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();
  const [isScheduleDialogOpen, setIsScheduleDialogOpen] = useState(false);
  const [isTrashConfirmOpen, setIsTrashConfirmOpen] = useState(false);
  const [previewLink, setPreviewLink] = useState<{ previewPath: string; expiresAtUtc: string } | null>(null);

  function onMutated(message: string) {
    showToast(message, 'success');
    onChanged();
  }

  function onMutationError(error: unknown) {
    showToast(getApiErrorMessage(error), 'danger');
  }

  const publishMutation = useMutation({
    mutationFn: (body: { publishAtUtc: string | null; unpublishAtUtc: string | null }) =>
      publishContentItem(client, contentItemId, { rowVersion: detail.rowVersion, ...body }),
    onSuccess: () => {
      onMutated('İçerik yayınlandı.');
      setIsScheduleDialogOpen(false);
    },
    onError: onMutationError,
  });
  const scheduleMutation = useMutation({
    mutationFn: (body: { publishAtUtc: string | null; unpublishAtUtc: string | null }) =>
      scheduleContentItem(client, contentItemId, { rowVersion: detail.rowVersion, ...body }),
    onSuccess: () => {
      onMutated('Zamanlama güncellendi.');
      setIsScheduleDialogOpen(false);
    },
    onError: onMutationError,
  });
  const unpublishMutation = useMutation({
    mutationFn: () => unpublishContentItem(client, contentItemId, { rowVersion: detail.rowVersion }),
    onSuccess: () => onMutated('İçerik yayından kaldırıldı.'),
    onError: onMutationError,
  });
  const archiveMutation = useMutation({
    mutationFn: () => archiveContentItem(client, contentItemId, { rowVersion: detail.rowVersion }),
    onSuccess: () => onMutated('İçerik arşivlendi.'),
    onError: onMutationError,
  });
  const unarchiveMutation = useMutation({
    mutationFn: () => unarchiveContentItem(client, contentItemId, { rowVersion: detail.rowVersion }),
    onSuccess: () => onMutated('İçerik arşivden çıkarıldı (taslak/yayından kaldırılmış durumda).'),
    onError: onMutationError,
  });
  const duplicateMutation = useMutation({
    mutationFn: () => duplicateContentItem(client, contentItemId),
    onSuccess: (response) => {
      showToast('İçerik kopyalandı.', 'success');
      navigate(ROUTES.websiteContentItemDetail(response.id));
    },
    onError: onMutationError,
  });
  const trashMutation = useMutation({
    mutationFn: () => deleteContentItem(client, contentItemId, { rowVersion: detail.rowVersion }),
    onSuccess: () => {
      showToast('İçerik çöp kutusuna taşındı.', 'success');
      navigate(ROUTES.websiteContentItems);
    },
    onError: onMutationError,
  });
  const previewLinkMutation = useMutation({
    mutationFn: () => createContentPreviewLink(client, contentItemId, { languageCode: null, durationHours: null }),
    onSuccess: (response) => setPreviewLink(response),
    onError: onMutationError,
  });

  const [scheduleForm, setScheduleForm] = useState({
    publishAtUtc: utcToLocalInputValue(detail.publishAtUtc),
    unpublishAtUtc: utcToLocalInputValue(detail.unpublishAtUtc),
  });

  function openScheduleDialog() {
    setScheduleForm({
      publishAtUtc: utcToLocalInputValue(detail.publishAtUtc),
      unpublishAtUtc: utcToLocalInputValue(detail.unpublishAtUtc),
    });
    setIsScheduleDialogOpen(true);
  }

  function submitSchedule() {
    const body = {
      publishAtUtc: localInputValueToUtc(scheduleForm.publishAtUtc),
      unpublishAtUtc: localInputValueToUtc(scheduleForm.unpublishAtUtc),
    };
    if (detail.status === 'Published') {
      scheduleMutation.mutate(body);
    } else {
      publishMutation.mutate(body);
    }
  }

  const isPending =
    publishMutation.isPending ||
    scheduleMutation.isPending ||
    unpublishMutation.isPending ||
    archiveMutation.isPending ||
    unarchiveMutation.isPending ||
    duplicateMutation.isPending ||
    trashMutation.isPending;

  return (
    <div className="field-row">
      {(detail.status === 'Draft' || detail.status === 'Unpublished') && (
        <Button type="button" onClick={() => publishMutation.mutate({ publishAtUtc: null, unpublishAtUtc: null })} disabled={isPending}>
          Yayınla
        </Button>
      )}
      {(detail.status === 'Draft' || detail.status === 'Unpublished' || detail.status === 'Published') && (
        <Button type="button" tone="secondary" onClick={openScheduleDialog} disabled={isPending}>
          Zamanla
        </Button>
      )}
      {detail.status === 'Published' && (
        <Button type="button" tone="secondary" onClick={() => unpublishMutation.mutate()} disabled={isPending}>
          Yayından kaldır
        </Button>
      )}
      {detail.status !== 'Archived' && (
        <Button type="button" tone="secondary" onClick={() => archiveMutation.mutate()} disabled={isPending}>
          Arşivle
        </Button>
      )}
      {detail.status === 'Archived' && (
        <Button type="button" tone="secondary" onClick={() => unarchiveMutation.mutate()} disabled={isPending}>
          Arşivden çıkar
        </Button>
      )}
      <Button type="button" tone="quiet" onClick={() => duplicateMutation.mutate()} disabled={isPending}>
        Kopyala
      </Button>
      <Button type="button" tone="quiet" onClick={() => previewLinkMutation.mutate()} disabled={previewLinkMutation.isPending}>
        Önizleme bağlantısı oluştur
      </Button>
      <Button type="button" tone="danger" onClick={() => setIsTrashConfirmOpen(true)} disabled={isPending}>
        Çöp kutusuna taşı
      </Button>

      <AccessibleDialog open={isScheduleDialogOpen} onClose={() => setIsScheduleDialogOpen(false)} title="Yayın zamanlaması">
        <TextField
          id="ci-schedule-publish-at"
          label="Yayın tarihi"
          type="datetime-local"
          value={scheduleForm.publishAtUtc}
          onChange={(event) => setScheduleForm({ ...scheduleForm, publishAtUtc: event.target.value })}
          hint="Boş bırakılırsa (yayınlama için) anında yayınlanır."
        />
        <TextField
          id="ci-schedule-unpublish-at"
          label="Yayından kaldırma tarihi"
          type="datetime-local"
          value={scheduleForm.unpublishAtUtc}
          onChange={(event) => setScheduleForm({ ...scheduleForm, unpublishAtUtc: event.target.value })}
          hint="Boş bırakılırsa süresiz yayında kalır."
        />
        <Button type="button" tone="quiet" onClick={() => setIsScheduleDialogOpen(false)}>
          Vazgeç
        </Button>{' '}
        <Button type="button" onClick={submitSchedule} disabled={publishMutation.isPending || scheduleMutation.isPending}>
          {detail.status === 'Published' ? 'Zamanlamayı güncelle' : 'Yayınla'}
        </Button>
      </AccessibleDialog>

      <ConfirmDialog
        open={isTrashConfirmOpen}
        title="İçeriği çöp kutusuna taşı"
        description="İçerik 30 gün çöp kutusunda tutulur, bu süre içinde geri alınabilir. Yayındaysa önce yayından kaldırılmalıdır."
        confirmLabel="Çöp kutusuna taşı"
        isConfirming={trashMutation.isPending}
        onConfirm={() => trashMutation.mutate()}
        onCancel={() => setIsTrashConfirmOpen(false)}
      />

      {previewLink && (
        <AccessibleDialog open={Boolean(previewLink)} onClose={() => setPreviewLink(null)} title="Önizleme bağlantısı">
          <p>
            <code>{previewLink.previewPath}</code>
          </p>
          <p className="field-hint">Son kullanma: {previewLink.expiresAtUtc}</p>
          <Button type="button" onClick={() => setPreviewLink(null)}>
            Kapat
          </Button>
        </AccessibleDialog>
      )}
    </div>
  );
}
