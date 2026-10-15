import { listTags, type TagResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, SelectField } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useId, useState, type FormEvent } from 'react';

/** Çağıran taraf `key={sourceTag?.id}` vermelidir: değişen kaynağa göre yeniden kurulur. */
export function MergeTagDialog({
  sourceTag,
  isSubmitting,
  onConfirm,
  onCancel,
}: {
  sourceTag: TagResponse | null;
  isSubmitting: boolean;
  onConfirm: (targetId: string) => void;
  onCancel: () => void;
}) {
  const { client } = useAuth();
  const [selectedTargetId, setSelectedTargetId] = useState<string | null>(null);
  const selectId = useId();

  /** Hedef seçimi geçerli sayfadaki değil, kaynağın diliyle aynı TÜM etiketlerden yapılabilmeli. */
  const { data } = useQuery({
    queryKey: ['website-tags-merge-candidates', sourceTag?.languageCode],
    queryFn: () => listTags(client, { languageCode: sourceTag!.languageCode, pageSize: 200 }),
    enabled: Boolean(sourceTag),
  });
  const options = (data?.items ?? []).filter((tag) => tag.id !== sourceTag?.id);
  const targetId = selectedTargetId ?? options[0]?.id ?? '';

  if (!sourceTag) return null;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (targetId) onConfirm(targetId);
  }

  return (
    <AccessibleDialog open={Boolean(sourceTag)} onClose={onCancel} title="Etiketi birleştir">
      <h2>&quot;{sourceTag.name}&quot; etiketini birleştir</h2>
      <p>Bu etiketin tüm içerik atamaları seçtiğiniz etikete taşınır, &quot;{sourceTag.name}&quot; silinir. Bu işlem geri alınamaz.</p>
      <form onSubmit={handleSubmit}>
        <SelectField id={selectId} label="Hedef etiket" required value={targetId} onChange={(event) => setSelectedTargetId(event.target.value)}>
          {options.length === 0 && <option value="">Aynı dilde başka etiket yok</option>}
          {options.map((tag) => (
            <option key={tag.id} value={tag.id}>
              {tag.name}
            </option>
          ))}
        </SelectField>
        <div className="confirm-dialog__actions">
          <Button type="button" tone="quiet" onClick={onCancel} disabled={isSubmitting}>
            Vazgeç
          </Button>
          <Button type="submit" tone="danger" disabled={isSubmitting || !targetId}>
            {isSubmitting ? 'Birleştiriliyor…' : 'Birleştir'}
          </Button>
        </div>
      </form>
    </AccessibleDialog>
  );
}
