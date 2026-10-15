import type { ImpactMetricDetailResponse } from '@gm/api-client';
import { AccessibleDialog, Button, TextField, TranslationTabs, type TranslationLanguage } from '@gm/ui';
import { useState } from 'react';

export interface ImpactMetricCoreDraft {
  value: string;
  iconKey: string;
  sortOrder: number;
  /** Yalnız OLUŞTURMADA kullanılır (varsayılan dilin etiketi) - backend `CreateImpactMetricRequest.
      DefaultLanguageLabel`'i nullable tanımlasa da `CreateImpactMetricCommandValidator` `NotEmpty()`
      ister (OpenAPI'nin nullable işaretiyle çelişiyor, bkz. rapordaki backend/OpenAPI bulgusu). */
  defaultLanguageLabel: string;
}

export interface ImpactMetricTranslationDraft {
  languageCode: string;
  label: string;
  unit: string;
  period: string;
  source: string;
}

/** Hem "yeni ekle" (detail: null) hem "mevcut düzenle" biçiminde kullanılır. */
export function ImpactMetricEditDialog({
  open,
  detail,
  languages,
  onSaveCore,
  onSaveTranslation,
  onClose,
}: {
  open: boolean;
  detail: ImpactMetricDetailResponse | null;
  languages: readonly TranslationLanguage[];
  onSaveCore: (draft: ImpactMetricCoreDraft) => void;
  onSaveTranslation: (languageCode: string, draft: ImpactMetricTranslationDraft) => void;
  onClose: () => void;
}) {
  const [activeLanguage, setActiveLanguage] = useState(languages[0]?.code ?? '');
  const [core, setCore] = useState<ImpactMetricCoreDraft>({
    value: detail ? String(detail.value) : '',
    iconKey: detail?.iconKey ?? '',
    sortOrder: detail ? Number(detail.sortOrder) : 0,
    defaultLanguageLabel: '',
  });

  if (!open) return null;

  function translationFor(languageCode: string): ImpactMetricTranslationDraft {
    const existing = detail?.translations.find((t) => t.languageCode === languageCode);
    return {
      languageCode,
      label: existing?.label ?? '',
      unit: existing?.unit ?? '',
      period: existing?.period ?? '',
      source: existing?.source ?? '',
    };
  }

  return (
    <AccessibleDialog open={open} onClose={onClose} title={detail ? 'Rakam düzenle' : 'Yeni rakam'}>
      <h2>{detail ? 'Rakam düzenle' : 'Yeni rakam'}</h2>
      <TextField id="metric-value" label="Değer" type="number" value={core.value} onChange={(e) => setCore({ ...core, value: e.target.value })} />
      <TextField id="metric-icon" label="İkon anahtarı" value={core.iconKey} onChange={(e) => setCore({ ...core, iconKey: e.target.value })} />
      <TextField
        id="metric-sort"
        label="Sıra"
        type="number"
        value={core.sortOrder}
        onChange={(e) => setCore({ ...core, sortOrder: Number(e.target.value) })}
      />
      {!detail && (
        <TextField
          id="metric-default-label"
          label="Etiket (varsayılan dil)"
          required
          value={core.defaultLanguageLabel}
          onChange={(e) => setCore({ ...core, defaultLanguageLabel: e.target.value })}
        />
      )}
      <Button type="button" onClick={() => onSaveCore(core)} disabled={!detail && !core.defaultLanguageLabel.trim()}>
        {detail ? 'Temel bilgileri kaydet' : 'Oluştur'}
      </Button>

      {detail && (
        <TranslationTabs
          languages={languages}
          activeCode={activeLanguage}
          onChange={setActiveLanguage}
          renderPanel={(languageCode) => (
            <TranslationPanel
              key={languageCode}
              initial={translationFor(languageCode)}
              onSave={(next) => onSaveTranslation(languageCode, next)}
            />
          )}
        />
      )}

      <div className="confirm-dialog__actions">
        <Button type="button" tone="quiet" onClick={onClose}>
          Kapat
        </Button>
      </div>
    </AccessibleDialog>
  );
}

function TranslationPanel({
  initial,
  onSave,
}: {
  initial: ImpactMetricTranslationDraft;
  onSave: (draft: ImpactMetricTranslationDraft) => void;
}) {
  const [draft, setDraft] = useState(initial);
  const isDirty = JSON.stringify(draft) !== JSON.stringify(initial);

  return (
    <div>
      <TextField id={`metric-label-${draft.languageCode}`} label="Etiket" required value={draft.label} onChange={(e) => setDraft({ ...draft, label: e.target.value })} />
      <TextField id={`metric-unit-${draft.languageCode}`} label="Birim" value={draft.unit} onChange={(e) => setDraft({ ...draft, unit: e.target.value })} />
      <TextField id={`metric-period-${draft.languageCode}`} label="Dönem" value={draft.period} onChange={(e) => setDraft({ ...draft, period: e.target.value })} />
      <TextField id={`metric-source-${draft.languageCode}`} label="Kaynak" value={draft.source} onChange={(e) => setDraft({ ...draft, source: e.target.value })} />
      <Button type="button" tone="secondary" onClick={() => onSave(draft)} disabled={!isDirty || !draft.label.trim()}>
        Bu dili kaydet
      </Button>
    </div>
  );
}
