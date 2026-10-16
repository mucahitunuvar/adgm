import { EventFormat, EventRegistrationState } from '@gm/api-client';

const FORMAT_LABELS: Record<string, string> = {
  [EventFormat.InPerson]: 'Yüz yüze',
  [EventFormat.Online]: 'Çevrimiçi',
  [EventFormat.Hybrid]: 'Hibrit',
};

const REGISTRATION_STATE_LABELS: Record<string, string> = {
  [EventRegistrationState.NotOpen]: 'Kayıt henüz açılmadı',
  [EventRegistrationState.Open]: 'Kayıt açık',
  [EventRegistrationState.Full]: 'Kontenjan doldu',
  [EventRegistrationState.WaitlistOpen]: 'Bekleme listesi açık',
  [EventRegistrationState.Closed]: 'Kayıt kapandı',
  [EventRegistrationState.Cancelled]: 'Etkinlik iptal edildi',
};

export function eventFormatLabel(format: string): string {
  return FORMAT_LABELS[format] ?? format;
}

export function registrationStateLabel(state: string): string {
  return REGISTRATION_STATE_LABELS[state] ?? state;
}

/** Yalnızca `Open`/`WaitlistOpen` durumunda kayıt formu gösterilir; diğerleri bilgilendirmedir. */
export function canRegister(state: string): boolean {
  return state === EventRegistrationState.Open || state === EventRegistrationState.WaitlistOpen;
}
