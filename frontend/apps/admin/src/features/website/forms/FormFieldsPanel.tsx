import { getApiErrorMessage, getFormDefinitionById, setFormFields, type FormDefinitionDetailResponse } from '@gm/api-client';
import {
  Button,
  ConfirmDialog,
  EmptyState,
  ReorderList,
  StatusBadge,
  UnsavedChangesGuard,
  confirmDiscardChanges,
  useToast,
  type TranslationLanguage,
} from '@gm/ui';
import { useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useAuth } from '@gm/auth';
import { FormFieldDialog, fieldTypeLabel } from './FormFieldDialog';
import { draftFieldsFromResponse, draftFieldsToInput, newDraftField, type DraftFormField } from './formFieldTypes';

/**
 * `SetFormFields` tüm listeyi sırayla değiştirir (tekil alan ucu yok) - aynı "taslak diziyi düzenle,
 * tek PUT ile kaydet" deseni `SliderDetailPage`'in slide listesiyle aynı (bkz. slideTypes.ts).
 */
export function FormFieldsPanel({
  formId,
  detail,
  languages,
  onSaved,
}: {
  formId: string;
  detail: FormDefinitionDetailResponse;
  languages: readonly TranslationLanguage[];
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();

  const [fields, setFields] = useState<DraftFormField[]>(() => draftFieldsFromResponse(detail.fields));
  const [loadedRowVersion, setLoadedRowVersion] = useState(detail.rowVersion);
  const [editingTempKey, setEditingTempKey] = useState<string | null>(null);
  const [deletingTempKey, setDeletingTempKey] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  if (detail.rowVersion !== loadedRowVersion) {
    setLoadedRowVersion(detail.rowVersion);
    setFields(draftFieldsFromResponse(detail.fields));
  }

  const dirty = JSON.stringify(draftFieldsToInput(fields)) !== JSON.stringify(draftFieldsToInput(draftFieldsFromResponse(detail.fields)));
  const editingField = editingTempKey ? fields.find((f) => f.tempKey === editingTempKey) ?? null : null;
  const existingKeys = fields.map((f) => f.key).filter((k) => k.trim() !== '');

  async function handleSave() {
    setIsSaving(true);
    try {
      const fresh = await getFormDefinitionById(client, formId);
      if (fresh.rowVersion !== detail.rowVersion) {
        const proceed = window.confirm('Bu form siz düzenlerken sunucuda değişmiş. Listeyi yeniden yükleyip değişikliklerinizi kaybetmek ister misiniz?');
        if (proceed) {
          setLoadedRowVersion('');
          onSaved();
        }
        setIsSaving(false);
        return;
      }

      await setFormFields(client, formId, { rowVersion: detail.rowVersion, fields: draftFieldsToInput(fields) });
      showToast('Alan listesi kaydedildi.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-form', formId] });
      onSaved();
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
      if ((mutationError as { status?: number }).status === 409) {
        onSaved();
      }
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div>
      <UnsavedChangesGuard isDirty={dirty} />
      {fields.length === 0 ? (
        <EmptyState title="Henüz alan yok" description="Alan ekleyin." />
      ) : (
        <ReorderList
          items={fields}
          getKey={(f) => f.tempKey}
          getLabel={(f) => f.translations[0]?.label || f.key || '(adsız)'}
          onMove={(fromIndex, toIndex) =>
            setFields((current) => {
              const list = [...current];
              const [moved] = list.splice(fromIndex, 1);
              list.splice(toIndex, 0, moved!);
              return list;
            })
          }
          renderItem={(field) => (
            <div className="field-row" style={{ alignItems: 'center' }}>
              <strong>{field.translations[0]?.label || field.key || '(adsız)'}</strong>
              <span className="field-hint">{field.key}</span>
              <StatusBadge tone="neutral">{field.type ? fieldTypeLabel(field.type) : '—'}</StatusBadge>
              {field.isRequired && <StatusBadge tone="warning">Zorunlu</StatusBadge>}
              <Button type="button" tone="quiet" onClick={() => setEditingTempKey(field.tempKey)}>
                Düzenle
              </Button>
              <Button type="button" tone="danger" onClick={() => setDeletingTempKey(field.tempKey)}>
                Sil
              </Button>
            </div>
          )}
        />
      )}

      <div className="field-row">
        <Button
          type="button"
          tone="secondary"
          onClick={() => {
            const draft = newDraftField();
            setFields((current) => [...current, draft]);
            setEditingTempKey(draft.tempKey);
          }}
        >
          Alan ekle
        </Button>
        <Button type="button" onClick={() => void handleSave()} disabled={!dirty || isSaving}>
          {isSaving ? 'Kaydediliyor…' : 'Alan listesini kaydet'}
        </Button>
        <Button
          type="button"
          tone="quiet"
          onClick={() => {
            if (!confirmDiscardChanges(dirty)) return;
            setFields(draftFieldsFromResponse(detail.fields));
          }}
        >
          Değişiklikleri geri al
        </Button>
      </div>

      <FormFieldDialog
        key={editingTempKey ?? 'closed'}
        open={editingTempKey !== null}
        field={editingField}
        existingKeys={existingKeys}
        languages={languages}
        onSave={(next) => {
          setFields((current) => (current.some((f) => f.tempKey === next.tempKey) ? current.map((f) => (f.tempKey === next.tempKey ? next : f)) : [...current, next]));
          setEditingTempKey(null);
        }}
        onCancel={() => {
          // Yeni eklenen ama hiç kaydedilmemiş bir alan vazgeçilince taslaktan da kaldırılır.
          if (editingField && !detail.fields.some((f) => f.key === editingField.key)) {
            setFields((current) => current.filter((f) => f.tempKey !== editingTempKey));
          }
          setEditingTempKey(null);
        }}
      />
      <ConfirmDialog
        open={deletingTempKey !== null}
        title="Alanı kaldır"
        description="Bu alan taslaktan kaldırılacak. Kaydetmeden listeyi terk ederseniz bu değişiklik uygulanmaz."
        onConfirm={() => {
          setFields((current) => current.filter((f) => f.tempKey !== deletingTempKey));
          setDeletingTempKey(null);
        }}
        onCancel={() => setDeletingTempKey(null)}
      />
    </div>
  );
}
