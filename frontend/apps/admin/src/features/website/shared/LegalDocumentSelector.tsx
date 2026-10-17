import { getLegalDocumentById, getLegalDocuments, type LegalDocumentKind } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { formatDateTime, SelectField } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { getEnumLabel } from '../../../i18n/getEnumLabel';

/**
 * Ayar ekranlarındaki (Bülten/Etkinlik/Çerez) aydınlatma/politika anahtarı alanları serbest metin
 * DEĞİLDİR - backend bu anahtarı `LegalDocument.Key`'e çözüp `Kind`'ini doğrular (ör.
 * `UpdateSiteSettingsNewsletterCommandHandler`, yalnız `PrivacyNotice` kabul eder); yanlış türde bir
 * anahtar 400 ile reddedilir. Bu seçici o yüzden listeyi `kind`e göre önceden daraltır - kullanıcı
 * geçersiz bir anahtarı hiç göremez.
 */
export function LegalDocumentSelector({
  idPrefix,
  label,
  kind,
  value,
  onChange,
  allowEmpty = true,
}: {
  idPrefix: string;
  label: string;
  kind: LegalDocumentKind;
  value: string | null;
  onChange: (value: string | null) => void;
  allowEmpty?: boolean;
}) {
  const { client } = useAuth();
  const { data: documents } = useQuery({ queryKey: ['website-legal-documents'], queryFn: () => getLegalDocuments(client) });

  const candidates = (documents ?? []).filter((document) => document.kind === kind);
  const selected = (documents ?? []).find((document) => document.key === value);

  const { data: selectedDetail } = useQuery({
    queryKey: ['website-legal-document', selected?.id],
    queryFn: () => getLegalDocumentById(client, selected!.id),
    enabled: Boolean(selected?.id),
  });
  const effectiveVersion = selectedDetail?.versions.find((version) => version.versionNumber === selectedDetail.effectiveVersionNumber);

  return (
    <div>
      <SelectField
        id={`${idPrefix}-legal-document-key`}
        label={label}
        hint={`Yalnız "${getEnumLabel('LegalDocumentKind', kind)}" türündeki yasal metinler listelenir.`}
        value={value ?? ''}
        onChange={(event) => onChange(event.target.value || null)}
      >
        {allowEmpty && <option value="">Yok</option>}
        {candidates.map((document) => (
          <option key={document.id} value={document.key}>
            {document.title} ({document.key})
          </option>
        ))}
      </SelectField>
      {value && !selected && (
        <p className="field-hint status status--warning">
          "{value}" anahtarlı bu türde bir yasal metin bulunamadı - kaydedilirse backend reddedecektir.
        </p>
      )}
      {selected && selected.effectiveVersionNumber == null && (
        <p className="field-hint status status--warning">
          "{selected.title}" belgesinin henüz yayınlı bir sürümü yok - bu anahtar yayınlı bir sürüm olmadan kaydedilemez.
        </p>
      )}
      {selected && effectiveVersion && (
        <p className="field-hint">
          Yürürlükteki sürüm: v{effectiveVersion.versionNumber}
          {effectiveVersion.effectiveAtUtc ? ` · yürürlük tarihi: ${formatDateTime(effectiveVersion.effectiveAtUtc)}` : ''}
          {effectiveVersion.publishedAtUtc ? ` · yayın tarihi: ${formatDateTime(effectiveVersion.publishedAtUtc)}` : ''}
        </p>
      )}
    </div>
  );
}
