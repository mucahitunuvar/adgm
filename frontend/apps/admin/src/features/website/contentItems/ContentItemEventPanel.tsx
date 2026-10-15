import {
  ApiError,
  EventFormat,
  cancelEventSchedule,
  deleteEventSchedule,
  getApiErrorMessage,
  getEventScheduleByContentItemId,
  reactivateEventSchedule,
  upsertEventSchedule,
  type EventScheduleDetailResponse,
  type EventScheduleTranslationResponse,
  type UpsertEventScheduleRequest,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  CheckboxField,
  ErrorState,
  FormSection,
  LoadingState,
  SelectField,
  TextField,
  TranslationTabs,
  useToast,
  type TranslationLanguage,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { enumOptions } from '../../../i18n/enumOptions';

type EventForm = Omit<UpsertEventScheduleRequest, 'rowVersion'>;

function utcToLocalInputValue(utc: string): string {
  const date = new Date(utc);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function localInputValueToUtc(local: string): string {
  return new Date(local).toISOString();
}

function toForm(schedule: EventScheduleDetailResponse | null, languages: readonly TranslationLanguage[]): EventForm {
  if (!schedule) {
    return {
      startsAtUtc: '',
      endsAtUtc: '',
      format: EventFormat.InPerson,
      onlineLink: null,
      capacity: null,
      registrationEnabled: false,
      registrationOpensAtUtc: null,
      registrationClosesAtUtc: null,
      minAge: null,
      maxAge: null,
      autoConfirm: false,
      waitlistEnabled: false,
      translations: languages.map((l) => ({
        languageCode: l.code,
        venueName: null,
        venueAddress: null,
        feeInfo: null,
        instructors: null,
        programFlow: null,
        accessibilityNote: null,
      })),
    };
  }
  return {
    startsAtUtc: schedule.startsAtUtc,
    endsAtUtc: schedule.endsAtUtc,
    format: schedule.format,
    onlineLink: schedule.onlineLink,
    capacity: schedule.capacity,
    registrationEnabled: schedule.registrationEnabled,
    registrationOpensAtUtc: schedule.registrationOpensAtUtc,
    registrationClosesAtUtc: schedule.registrationClosesAtUtc,
    minAge: schedule.minAge,
    maxAge: schedule.maxAge,
    autoConfirm: schedule.autoConfirm,
    waitlistEnabled: schedule.waitlistEnabled,
    translations: languages.map((l) => {
      const existing = schedule.translations.find((t) => t.languageCode === l.code);
      return existing ?? { languageCode: l.code, venueName: null, venueAddress: null, feeInfo: null, instructors: null, programFlow: null, accessibilityNote: null };
    }),
  };
}

const formatOptions = enumOptions(EventFormat, 'EventFormat');

export function ContentItemEventPanel({ contentItemId, languages }: { contentItemId: string; languages: readonly TranslationLanguage[] }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const queryKey = ['website-event-schedule', contentItemId] as const;
  const [activeLanguage, setActiveLanguage] = useState(languages[0]?.code ?? '');

  const { data: schedule, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => getEventScheduleByContentItemId(client, contentItemId),
    retry: false,
  });

  const notFound = error instanceof ApiError && error.status === 404;
  const [form, setForm] = useState<EventForm | null>(null);
  const activeForm = form ?? toForm(notFound ? null : (schedule ?? null), languages);

  function onSaved(message: string) {
    showToast(message, 'success');
    setForm(null);
    void queryClient.invalidateQueries({ queryKey });
  }

  const saveMutation = useMutation({
    mutationFn: () => upsertEventSchedule(client, contentItemId, { rowVersion: schedule?.rowVersion ?? null, ...activeForm }),
    onSuccess: () => onSaved(schedule ? 'Etkinlik güncellendi.' : 'Etkinlik oluşturuldu.'),
    onError: (saveError) => showToast(getApiErrorMessage(saveError), 'danger'),
  });
  const cancelMutation = useMutation({
    mutationFn: (reason: string) => cancelEventSchedule(client, contentItemId, { rowVersion: schedule!.rowVersion, reason: reason || null }),
    onSuccess: () => onSaved('Etkinlik iptal edildi.'),
    onError: (cancelError) => showToast(getApiErrorMessage(cancelError), 'danger'),
  });
  const reactivateMutation = useMutation({
    mutationFn: () => reactivateEventSchedule(client, contentItemId, { rowVersion: schedule!.rowVersion }),
    onSuccess: () => onSaved('Etkinlik yeniden etkinleştirildi.'),
    onError: (reactivateError) => showToast(getApiErrorMessage(reactivateError), 'danger'),
  });
  const deleteMutation = useMutation({
    mutationFn: () => deleteEventSchedule(client, contentItemId),
    onSuccess: () => onSaved('Etkinlik tanımı silindi.'),
    onError: (deleteError) => showToast(getApiErrorMessage(deleteError), 'danger'),
  });

  if (isLoading) return <LoadingState />;
  if (error && !notFound) return <ErrorState description={getApiErrorMessage(error)} />;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    saveMutation.mutate();
  }

  return (
    <div>
      {notFound && <p className="field-hint">Bu içerik için henüz bir etkinlik tanımı yok. Aşağıdaki formu doldurup oluşturun.</p>}
      {schedule && (
        <p className="field-hint">
          Onaylanan: {schedule.confirmedCount} · Bekleme listesinde: {schedule.waitlistedCount}
          {schedule.isCancelled && ' · İPTAL EDİLDİ'}
        </p>
      )}
      <form onSubmit={handleSubmit}>
        <FormSection title="Etkinlik">
          <TextField
            id="event-starts-at"
            label="Başlangıç"
            type="datetime-local"
            required
            value={activeForm.startsAtUtc ? utcToLocalInputValue(activeForm.startsAtUtc) : ''}
            onChange={(e) => setForm({ ...activeForm, startsAtUtc: localInputValueToUtc(e.target.value) })}
          />
          <TextField
            id="event-ends-at"
            label="Bitiş"
            type="datetime-local"
            required
            value={activeForm.endsAtUtc ? utcToLocalInputValue(activeForm.endsAtUtc) : ''}
            onChange={(e) => setForm({ ...activeForm, endsAtUtc: localInputValueToUtc(e.target.value) })}
          />
          <SelectField id="event-format" label="Format" value={activeForm.format} onChange={(e) => setForm({ ...activeForm, format: e.target.value })}>
            {formatOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </SelectField>
          <TextField
            id="event-online-link"
            label="Online bağlantı"
            hint="Yalnız onaylı kayıtlara onay e-postasıyla gider; hiçbir public yanıtta görünmez."
            value={activeForm.onlineLink ?? ''}
            onChange={(e) => setForm({ ...activeForm, onlineLink: e.target.value || null })}
          />
          <TextField
            id="event-capacity"
            label="Kontenjan"
            type="number"
            value={activeForm.capacity ?? ''}
            onChange={(e) => setForm({ ...activeForm, capacity: e.target.value ? Number(e.target.value) : null })}
          />
          <CheckboxField
            id="event-registration-enabled"
            label="Başvuru etkin"
            checked={activeForm.registrationEnabled}
            onChange={(e) => setForm({ ...activeForm, registrationEnabled: e.target.checked })}
          />
          <TextField
            id="event-registration-opens-at"
            label="Başvuru açılış"
            type="datetime-local"
            value={activeForm.registrationOpensAtUtc ? utcToLocalInputValue(activeForm.registrationOpensAtUtc) : ''}
            onChange={(e) => setForm({ ...activeForm, registrationOpensAtUtc: e.target.value ? localInputValueToUtc(e.target.value) : null })}
          />
          <TextField
            id="event-registration-closes-at"
            label="Başvuru kapanış"
            type="datetime-local"
            value={activeForm.registrationClosesAtUtc ? utcToLocalInputValue(activeForm.registrationClosesAtUtc) : ''}
            onChange={(e) => setForm({ ...activeForm, registrationClosesAtUtc: e.target.value ? localInputValueToUtc(e.target.value) : null })}
          />
          <TextField
            id="event-min-age"
            label="Minimum yaş"
            type="number"
            value={activeForm.minAge ?? ''}
            onChange={(e) => setForm({ ...activeForm, minAge: e.target.value ? Number(e.target.value) : null })}
          />
          <TextField
            id="event-max-age"
            label="Maksimum yaş"
            type="number"
            value={activeForm.maxAge ?? ''}
            onChange={(e) => setForm({ ...activeForm, maxAge: e.target.value ? Number(e.target.value) : null })}
          />
          <CheckboxField
            id="event-auto-confirm"
            label="Otomatik onay"
            checked={activeForm.autoConfirm}
            onChange={(e) => setForm({ ...activeForm, autoConfirm: e.target.checked })}
          />
          <CheckboxField
            id="event-waitlist-enabled"
            label="Bekleme listesi etkin"
            checked={activeForm.waitlistEnabled}
            onChange={(e) => setForm({ ...activeForm, waitlistEnabled: e.target.checked })}
          />
        </FormSection>
        <FormSection title="Dile göre etkinlik bilgileri">
          <TranslationTabs
            languages={languages}
            activeCode={activeLanguage}
            onChange={setActiveLanguage}
            renderPanel={(code) => {
              const translation = activeForm.translations.find((t) => t.languageCode === code);
              function setField(field: Exclude<keyof EventScheduleTranslationResponse, 'languageCode'>, value: string) {
                setForm({
                  ...activeForm,
                  translations: activeForm.translations.map((t) => (t.languageCode === code ? { ...t, [field]: value || null } : t)),
                });
              }
              return (
                <div>
                  <TextField id={`event-venue-name-${code}`} label="Mekan adı" value={translation?.venueName ?? ''} onChange={(e) => setField('venueName', e.target.value)} />
                  <TextField id={`event-venue-address-${code}`} label="Mekan adresi" value={translation?.venueAddress ?? ''} onChange={(e) => setField('venueAddress', e.target.value)} />
                  <TextField id={`event-fee-info-${code}`} label="Ücret bilgisi" value={translation?.feeInfo ?? ''} onChange={(e) => setField('feeInfo', e.target.value)} />
                  <TextField id={`event-instructors-${code}`} label="Eğitmenler" value={translation?.instructors ?? ''} onChange={(e) => setField('instructors', e.target.value)} />
                  <TextField id={`event-program-flow-${code}`} label="Program akışı" value={translation?.programFlow ?? ''} onChange={(e) => setField('programFlow', e.target.value)} />
                  <TextField id={`event-accessibility-${code}`} label="Erişilebilirlik notu" value={translation?.accessibilityNote ?? ''} onChange={(e) => setField('accessibilityNote', e.target.value)} />
                </div>
              );
            }}
          />
        </FormSection>
        <Button type="button" tone="quiet" onClick={() => setForm(null)} disabled={saveMutation.isPending}>
          Değişiklikleri geri al
        </Button>{' '}
        <Button type="submit" disabled={saveMutation.isPending}>
          {saveMutation.isPending ? 'Kaydediliyor…' : schedule ? 'Kaydet' : 'Oluştur'}
        </Button>
      </form>
      {schedule && !notFound && (
        <div className="field-row">
          {schedule.isCancelled ? (
            <Button type="button" tone="secondary" onClick={() => reactivateMutation.mutate()} disabled={reactivateMutation.isPending}>
              Yeniden etkinleştir
            </Button>
          ) : (
            <Button type="button" tone="secondary" onClick={() => cancelMutation.mutate('')} disabled={cancelMutation.isPending}>
              İptal et
            </Button>
          )}
          <Button type="button" tone="danger" onClick={() => deleteMutation.mutate()} disabled={deleteMutation.isPending}>
            Etkinlik tanımını sil
          </Button>
        </div>
      )}
    </div>
  );
}
