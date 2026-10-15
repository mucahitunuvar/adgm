import { LinkTargetKind, getContentTypes } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, SelectField, TextField } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useContentItemPicker } from '../contentItems/useContentItemPicker';

/**
 * Backend'in `MenuItemLinkInput`/`SlideLinkInput`/`LinkTargetDto`'su - üçü de birebir aynı alan
 * kümesini taşır (`kind, contentItemId, contentTypeId, internalPath, externalUrl`); bu bileşen
 * hepsiyle yapısal olarak uyumlu, tek ortak şekil.
 */
export interface LinkTargetValue {
  kind: string;
  contentItemId: string | null;
  contentTypeId: string | null;
  internalPath: string | null;
  externalUrl: string | null;
}

const EMPTY_LINK: LinkTargetValue = {
  kind: LinkTargetKind.ExternalUrl,
  contentItemId: null,
  contentTypeId: null,
  internalPath: null,
  externalUrl: null,
};

/**
 * `LinkTarget` seçici - Menü öğeleri, slider slide'ları, pop-up/CTA/quick-links gibi blok alanları
 * ortak kullanır (FRONTEND-FAZ2-MASTER-PROMPT.md "ortak ilkeler": "tüm link alanları LinkTarget'tır").
 * `LinkTargetKind.None` kasıtlı olarak seçenek listesinde YOK - "bağlantı yok" `value: null` ile
 * temsil edilir (`allowEmpty` true iken "Yok" seçeneği bunu seçer), `kind: 'None'` özel bir UI durumu
 * olarak ayrıca modellenmez.
 */
export function LinkTargetField({
  idPrefix,
  label = 'Bağlantı',
  value,
  onChange,
  allowEmpty = true,
}: {
  idPrefix: string;
  label?: string;
  value: LinkTargetValue | null;
  onChange: (value: LinkTargetValue | null) => void;
  allowEmpty?: boolean;
}) {
  const { client } = useAuth();
  const { data: contentTypes } = useQuery({ queryKey: ['website-content-types'], queryFn: () => getContentTypes(client) });
  const { pickContentItem, pickerElement } = useContentItemPicker();

  async function handlePickContentItem() {
    const item = await pickContentItem();
    if (item) onChange({ ...EMPTY_LINK, kind: LinkTargetKind.Content, contentItemId: item.id });
  }

  return (
    <div className="field-row">
      <SelectField
        id={`${idPrefix}-link-kind`}
        label={`${label} türü`}
        value={value?.kind ?? ''}
        onChange={(event) => {
          const nextKind = event.target.value;
          onChange(nextKind ? { ...EMPTY_LINK, kind: nextKind } : null);
        }}
      >
        {allowEmpty && <option value="">Yok</option>}
        <option value={LinkTargetKind.Content}>İçerik</option>
        <option value={LinkTargetKind.ContentTypeListing}>İçerik tipi liste sayfası</option>
        <option value={LinkTargetKind.InternalPath}>Sistem rotası (göreli yol)</option>
        <option value={LinkTargetKind.ExternalUrl}>Dış bağlantı</option>
      </SelectField>

      {value?.kind === LinkTargetKind.Content && (
        <>
          <Button type="button" tone="quiet" onClick={() => void handlePickContentItem()}>
            {value.contentItemId ? 'İçeriği değiştir' : 'İçerik seç'}
          </Button>
          {pickerElement}
        </>
      )}

      {value?.kind === LinkTargetKind.ContentTypeListing && (
        <SelectField
          id={`${idPrefix}-link-content-type`}
          label="İçerik tipi"
          value={value.contentTypeId ?? ''}
          onChange={(event) => onChange({ ...value, contentTypeId: event.target.value || null })}
        >
          <option value="">Seçin</option>
          {(contentTypes ?? []).map((type) => (
            <option key={type.id} value={type.id}>
              {type.defaultLanguageName}
            </option>
          ))}
        </SelectField>
      )}

      {value?.kind === LinkTargetKind.InternalPath && (
        <TextField
          id={`${idPrefix}-link-path`}
          label="Yol"
          hint="Örn. /iletisim"
          value={value.internalPath ?? ''}
          onChange={(event) => onChange({ ...value, internalPath: event.target.value })}
        />
      )}

      {value?.kind === LinkTargetKind.ExternalUrl && (
        <TextField
          id={`${idPrefix}-link-url`}
          label="URL"
          type="url"
          value={value.externalUrl ?? ''}
          onChange={(event) => onChange({ ...value, externalUrl: event.target.value })}
        />
      )}
    </div>
  );
}
