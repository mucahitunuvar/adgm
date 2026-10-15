import { AccessibleDialog, Button, TextField } from '@gm/ui';
import { useId, useState, type FormEvent } from 'react';

export interface CreateCategoryDialogInput {
  defaultLanguageName: string;
  defaultLanguageSlug: string;
  sortOrder: number;
}

export interface CreateCategoryDialogProps {
  /** `undefined`: kapalı. `null`: kök kategori. `string`: bu kategorinin altına. */
  parentId: string | null | undefined;
  isSubmitting: boolean;
  onConfirm: (input: CreateCategoryDialogInput) => void;
  onCancel: () => void;
}

/** Çağıran taraf `key={parentId ?? 'root'}` vermelidir: her açılışta boş durumla yeniden kurulur. */
export function CreateCategoryDialog({ parentId, isSubmitting, onConfirm, onCancel }: CreateCategoryDialogProps) {
  const [name, setName] = useState('');
  const [slug, setSlug] = useState('');
  const [sortOrder, setSortOrder] = useState(0);
  const nameId = useId();
  const slugId = useId();
  const sortId = useId();

  if (parentId === undefined) return null;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onConfirm({ defaultLanguageName: name, defaultLanguageSlug: slug, sortOrder });
  }

  return (
    <AccessibleDialog open={parentId !== undefined} onClose={onCancel} title={parentId ? 'Alt kategori ekle' : 'Kök kategori ekle'}>
      <h2>{parentId ? 'Alt kategori ekle' : 'Kök kategori ekle'}</h2>
      <form onSubmit={handleSubmit}>
        <TextField id={nameId} label="Ad (varsayılan dil)" required value={name} onChange={(e) => setName(e.target.value)} />
        <TextField id={slugId} label="Slug" hint="Yol parçası, ör. haberler-kategorisi." required value={slug} onChange={(e) => setSlug(e.target.value)} />
        <TextField id={sortId} label="Sıra" type="number" required value={sortOrder} onChange={(e) => setSortOrder(Number(e.target.value))} />
        <div className="confirm-dialog__actions">
          <Button type="button" tone="quiet" onClick={onCancel} disabled={isSubmitting}>
            Vazgeç
          </Button>
          <Button type="submit" disabled={isSubmitting || !name || !slug}>
            {isSubmitting ? 'Oluşturuluyor…' : 'Oluştur'}
          </Button>
        </div>
      </form>
    </AccessibleDialog>
  );
}
