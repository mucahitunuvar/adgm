/**
 * `FormSubmissionAnswerResponse.value` OpenAPI'de `unknown` (alan türüne göre EAV-benzeri: Text/Email/
 * Phone/Textarea -> string, Checkbox -> boolean, MultiSelect -> string dizisi, Select -> string, File
 * cevapları buraya değil `files`'a gider). FRONTEND-FAZ2B-MASTER-PROMPT.md 2b-2(c): `unknown: any`
 * KULLANILMADAN tür korumalarıyla güvenli ayrıştırılır; tanınmayan biçim JSON metni olarak gösterilir.
 * Başvuru verisi düşmanca kabul edilir (FRONTEND-FAZ2B-MASTER-PROMPT.md "ortak ilkeler") - her dal
 * düz METİN döner, React'in kendi kaçışlaması dışında hiçbir HTML yorumlama/`dangerouslySetInnerHTML`
 * yoktur.
 */
export function formatAnswerValue(value: unknown): string {
  if (value === null || value === undefined) {
    return '—';
  }

  if (typeof value === 'string') {
    return value === '' ? '—' : value;
  }

  if (typeof value === 'number') {
    return String(value);
  }

  if (typeof value === 'boolean') {
    return value ? 'Evet' : 'Hayır';
  }

  if (Array.isArray(value)) {
    return value.length === 0 ? '—' : value.map((item) => formatAnswerValue(item)).join(', ');
  }

  // Tanınmayan biçim (nesne vb.) - veri kaybetmemek için JSON metni olarak gösterilir, HTML'e
  // yorumlanmaz.
  try {
    return JSON.stringify(value);
  } catch {
    return String(value);
  }
}

export function FormSubmissionAnswerValue({ value }: { value: unknown }) {
  return <span>{formatAnswerValue(value)}</span>;
}
