import { getContentCategoriesByType, getContentTypes, getImpactMetrics, getSliders, type BlockTypeFieldDescriptor } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, CheckboxField, DateTimeField, ReorderList, SelectField, TextField } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { RichTextEditor } from '@gm/rich-text';
import type { ReactNode } from 'react';
import { useContentItemPicker } from '../contentItems/useContentItemPicker';
import { useVideoPicker } from '../contentItems/useVideoPicker';
import { useMediaPicker } from '../../media/useMediaPicker';
import { LinkTargetField, type LinkTargetValue } from '../shared/LinkTargetField';
import { classifyField, emptyValueForField } from './blockFields';

export interface BlockFieldRendererProps {
  idPrefix: string;
  /** Yalnız `body` alanının zengin metin mi düz metin mi olduğunu ayırt etmek için (`rich-text`
      bloğunun kendi `body`'si - başka bloklardaki aynı isimli alan değil). */
  blockTypeKey: string;
  field: BlockTypeFieldDescriptor;
  value: unknown;
  onChange: (value: unknown) => void;
  /** Aynı nesnenin KARDEŞ alanlarının güncel değerleri - ör. `categoryId`'nin hangi içerik tipine
      ait kategorileri göstereceğini komşu `contentTypeKey` alanından okumak için. */
  siblingValues?: Record<string, unknown>;
}

/**
 * Tek bir blok alanını, `classifyField`'ın belirlediği türe göre render eder - gerekirse (object/
 * array<object>) kendi içine özyinelemeli olarak kendini çağırır. Master prompt'un "bilinmeyen alan
 * tipinde formu bozma, 'desteklenmeyen alan' göster" kuralı: `unsupported` dalı asla sessiz bir ham
 * metin/GUID kutusu çizmez.
 */
export function BlockFieldRenderer({ idPrefix, blockTypeKey, field, value, onChange, siblingValues }: BlockFieldRendererProps) {
  const kind = classifyField(field, blockTypeKey);
  const { client } = useAuth();
  const { pickContentItem, pickerElement: contentPickerElement } = useContentItemPicker();
  const { pickVideo, pickerElement: videoPickerElement } = useVideoPicker();
  const { pickMedia, pickerElement: mediaPickerElement } = useMediaPicker();

  const { data: contentTypes } = useQuery({
    queryKey: ['website-content-types'],
    queryFn: () => getContentTypes(client),
    enabled: kind === 'contentType' || kind === 'category',
  });
  const { data: sliders } = useQuery({ queryKey: ['website-sliders'], queryFn: () => getSliders(client), enabled: kind === 'slider' });
  const { data: metrics } = useQuery({
    queryKey: ['website-impact-metrics-all'],
    queryFn: () => getImpactMetrics(client, { pageSize: 100 }),
    enabled: kind === 'metricList',
  });

  const siblingContentTypeKey = typeof siblingValues?.contentTypeKey === 'string' ? siblingValues.contentTypeKey : null;
  const categoryContentTypeId = contentTypes?.find((t) => t.key === siblingContentTypeKey)?.id ?? null;
  const { data: categories } = useQuery({
    queryKey: ['website-content-categories', categoryContentTypeId],
    queryFn: () => getContentCategoriesByType(client, categoryContentTypeId!),
    enabled: kind === 'category' && Boolean(categoryContentTypeId),
  });

  switch (kind) {
    case 'text':
      return <TextField id={idPrefix} label={field.name} required={field.isRequired} value={(value as string) ?? ''} onChange={(e) => onChange(e.target.value)} />;

    case 'richText':
      return (
        <div className="form-field">
          <label htmlFor={idPrefix}>{field.name}</label>
          <RichTextEditor
            value={(value as string) ?? ''}
            onChange={onChange}
            ariaLabel={field.name}
            onRequestImage={async () => {
              const asset = await pickMedia();
              return asset ? { src: asset.originalUrl, alt: asset.originalFileName } : null;
            }}
          />
          {mediaPickerElement}
        </div>
      );

    case 'int':
    case 'decimal':
      return (
        <TextField
          id={idPrefix}
          label={field.name}
          type="number"
          required={field.isRequired}
          value={typeof value === 'number' ? value : ''}
          onChange={(e) => onChange(e.target.value === '' ? null : Number(e.target.value))}
        />
      );

    case 'boolean':
      return <CheckboxField id={idPrefix} label={field.name} checked={Boolean(value)} onChange={(e) => onChange(e.target.checked)} />;

    case 'dateTime':
      return <DateTimeField id={idPrefix} label={field.name} value={(value as string | null) ?? null} onChange={onChange} />;

    case 'media':
      return (
        <div className="field-row">
          <Button
            type="button"
            tone="quiet"
            onClick={() => void pickMedia().then((asset) => asset && onChange(asset.id))}
          >
            {value ? `${field.name}: değiştir` : `${field.name}: görsel seç`}
          </Button>
          {mediaPickerElement}
        </div>
      );

    case 'content':
      return (
        <div className="field-row">
          <Button type="button" tone="quiet" onClick={() => void pickContentItem().then((item) => item && onChange(item.id))}>
            {value ? `${field.name}: değiştir` : `${field.name}: içerik seç`}
          </Button>
          {contentPickerElement}
        </div>
      );

    case 'video':
      return (
        <div className="field-row">
          <Button type="button" tone="quiet" onClick={() => void pickVideo().then((video) => video && onChange(video.id))}>
            {value ? `${field.name}: değiştir` : `${field.name}: video seç`}
          </Button>
          {videoPickerElement}
        </div>
      );

    case 'contentType':
      return (
        <SelectField id={idPrefix} label={field.name} value={(value as string) ?? ''} onChange={(e) => onChange(e.target.value || null)}>
          <option value="">Seçin</option>
          {(contentTypes ?? []).map((type) => (
            <option key={type.id} value={type.id}>
              {type.defaultLanguageName}
            </option>
          ))}
        </SelectField>
      );

    case 'slider':
      return (
        <SelectField id={idPrefix} label={field.name} required={field.isRequired} value={(value as string) ?? ''} onChange={(e) => onChange(e.target.value || null)}>
          <option value="">Seçin</option>
          {(sliders ?? []).map((slider) => (
            <option key={slider.id} value={slider.id}>
              {slider.name}
            </option>
          ))}
        </SelectField>
      );

    case 'category':
      return (
        <SelectField
          id={idPrefix}
          label={field.name}
          hint={siblingContentTypeKey ? undefined : 'Önce içerik tipi anahtarını girin.'}
          disabled={!categoryContentTypeId}
          value={(value as string) ?? ''}
          onChange={(e) => onChange(e.target.value || null)}
        >
          <option value="">Tümü</option>
          {(categories ?? []).flatMap((category) => [category, ...category.children]).map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </SelectField>
      );

    case 'linkTarget':
      return (
        <LinkTargetField idPrefix={idPrefix} label={field.name} value={value as LinkTargetValue | null} onChange={onChange} />
      );

    case 'mediaList':
    case 'contentList':
      return (
        <GuidListField
          idPrefix={idPrefix}
          label={field.name}
          values={Array.isArray(value) ? (value as string[]) : []}
          onChange={onChange}
          onPick={kind === 'mediaList' ? () => pickMedia().then((a) => a?.id ?? null) : () => pickContentItem().then((i) => i?.id ?? null)}
          pickerElement={kind === 'mediaList' ? mediaPickerElement : contentPickerElement}
          addLabel={kind === 'mediaList' ? 'Görsel ekle' : 'İçerik ekle'}
        />
      );

    case 'metricList':
      return (
        <div>
          <p className="field-hint">{field.name}</p>
          {(metrics?.items ?? []).map((metric) => {
            const selected = Array.isArray(value) ? (value as string[]) : [];
            const isChecked = selected.includes(metric.id);
            return (
              <CheckboxField
                key={metric.id}
                id={`${idPrefix}-${metric.id}`}
                label={metric.label}
                checked={isChecked}
                onChange={(e) => onChange(e.target.checked ? [...selected, metric.id] : selected.filter((id) => id !== metric.id))}
              />
            );
          })}
        </div>
      );

    case 'arrayOfString':
      return (
        <TextField
          id={idPrefix}
          label={field.name}
          hint="Virgülle ayırın."
          value={Array.isArray(value) ? (value as string[]).join(', ') : ''}
          onChange={(e) => onChange(e.target.value.split(',').map((s) => s.trim()).filter(Boolean))}
        />
      );

    case 'object': {
      const objectValue = (value as Record<string, unknown>) ?? {};
      return (
        <fieldset className="form-section">
          <legend>{field.name}</legend>
          {(field.fields ?? []).map((nested) => (
            <BlockFieldRenderer
              key={nested.name}
              idPrefix={`${idPrefix}-${nested.name}`}
              blockTypeKey={blockTypeKey}
              field={nested}
              value={objectValue[nested.name]}
              siblingValues={objectValue}
              onChange={(nextValue) => onChange({ ...objectValue, [nested.name]: nextValue })}
            />
          ))}
        </fieldset>
      );
    }

    case 'arrayOfObject': {
      const items = Array.isArray(value) ? (value as Record<string, unknown>[]) : [];
      const nestedFields = field.fields ?? [];
      return (
        <div>
          <p className="field-hint">{field.name}</p>
          <ReorderList
            items={items}
            getKey={(item) => JSON.stringify(item)}
            getLabel={(item) => `${field.name}: ${JSON.stringify(item).slice(0, 30)}`}
            onMove={(fromIndex, toIndex) => {
              const next = [...items];
              const [moved] = next.splice(fromIndex, 1);
              next.splice(toIndex, 0, moved!);
              onChange(next);
            }}
            renderItem={(item, index) => (
              <fieldset className="form-section">
                <legend>{`${field.name} #${index + 1}`}</legend>
                {nestedFields.map((nested) => (
                  <BlockFieldRenderer
                    key={nested.name}
                    idPrefix={`${idPrefix}-${index}-${nested.name}`}
                    blockTypeKey={blockTypeKey}
                    field={nested}
                    value={item[nested.name]}
                    siblingValues={item}
                    onChange={(nextValue) => {
                      const next = [...items];
                      next[index] = { ...item, [nested.name]: nextValue };
                      onChange(next);
                    }}
                  />
                ))}
                <Button type="button" tone="danger" onClick={() => onChange(items.filter((_, i) => i !== index))}>
                  Kaldır
                </Button>
              </fieldset>
            )}
          />
          <Button
            type="button"
            tone="secondary"
            onClick={() => {
              const draft: Record<string, unknown> = {};
              for (const nested of nestedFields) draft[nested.name] = emptyValueForField(nested);
              onChange([...items, draft]);
            }}
          >
            {`${field.name} ekle`}
          </Button>
        </div>
      );
    }

    case 'unsupported':
      return (
        <p className="field-hint" role="note">
          {`"${field.name}" (${field.type}) - desteklenmeyen alan türü. Bu alan bu sürümde düzenlenemiyor.`}
        </p>
      );
  }
}

function GuidListField({
  idPrefix,
  label,
  values,
  onChange,
  onPick,
  pickerElement,
  addLabel,
}: {
  idPrefix: string;
  label: string;
  values: readonly string[];
  onChange: (next: string[]) => void;
  onPick: () => Promise<string | null>;
  pickerElement: ReactNode;
  addLabel: string;
}) {
  return (
    <div>
      <p className="field-hint">{label}</p>
      <ReorderList
        items={values}
        getKey={(id) => id}
        getLabel={(id) => id}
        onMove={(fromIndex, toIndex) => {
          const next = [...values];
          const [moved] = next.splice(fromIndex, 1);
          next.splice(toIndex, 0, moved!);
          onChange(next);
        }}
        renderItem={(id, index) => (
          <div className="field-row" style={{ alignItems: 'center' }} id={`${idPrefix}-${index}`}>
            <code>{id}</code>
            <Button type="button" tone="danger" onClick={() => onChange(values.filter((_, i) => i !== index))}>
              Kaldır
            </Button>
          </div>
        )}
      />
      <Button type="button" tone="secondary" onClick={() => void onPick().then((id) => id && onChange([...values, id]))}>
        {addLabel}
      </Button>
      {pickerElement}
    </div>
  );
}
