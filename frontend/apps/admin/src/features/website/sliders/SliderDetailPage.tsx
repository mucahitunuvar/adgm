import {
  deleteSlider,
  getApiErrorMessage,
  getSiteLanguages,
  getSliderById,
  replaceSlides,
  updateSlider,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  ConfirmDialog,
  EmptyState,
  ErrorState,
  LoadingState,
  PageHeader,
  ReorderList,
  TextField,
  TranslationTabs,
  UnsavedChangesGuard,
  confirmDiscardChanges,
  useToast,
} from '@gm/ui';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { useState } from 'react';
import { ROUTES } from '../../../routes';
import { SlideEditDialog } from './SlideEditDialog';
import { draftSlidesFromResponse, draftSlidesToInput, newDraftSlide, type DraftSlide } from './slideTypes';

export function SliderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();

  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['website-slider', id],
    queryFn: () => getSliderById(client, id!),
    enabled: Boolean(id),
  });

  const [names, setNames] = useState<Record<string, string>>({});
  const [slides, setSlides] = useState<DraftSlide[] | null>(null);
  const [loadedRowVersion, setLoadedRowVersion] = useState<string | null>(null);
  const [activeLanguage, setActiveLanguage] = useState('');
  const [editingTempKey, setEditingTempKey] = useState<string | null>(null);
  const [deletingTempKey, setDeletingTempKey] = useState<string | null>(null);
  const [isDeleteSliderOpen, setIsDeleteSliderOpen] = useState(false);
  const [isSavingSlides, setIsSavingSlides] = useState(false);
  const [isSavingNames, setIsSavingNames] = useState(false);

  const languages = languagesData?.items ?? [];

  if (data && data.rowVersion !== loadedRowVersion) {
    setLoadedRowVersion(data.rowVersion);
    setSlides(draftSlidesFromResponse(data.slides));
    setNames(Object.fromEntries(data.translations.map((t) => [t.languageCode, t.name])));
    if (!activeLanguage && data.translations[0]) setActiveLanguage(data.translations[0].languageCode);
  }

  if (isLoading || !slides || !id) return <LoadingState />;
  if (error || !data) return <ErrorState description={error ? getApiErrorMessage(error) : 'Slider bulunamadı.'} />;

  // Sabit kopyalar: TS'in null-daraltması aşağıdaki iç içe fonksiyon bildirimlerinin gövdesine
  // taşınmıyor (bkz. MenusPage.tsx'teki aynı not).
  const sliderId = id;
  const slider = data;
  const currentSlides = slides;
  const slidesDirty =
    JSON.stringify(draftSlidesToInput(currentSlides)) !== JSON.stringify(draftSlidesToInput(draftSlidesFromResponse(slider.slides)));
  const namesDirty = JSON.stringify(names) !== JSON.stringify(Object.fromEntries(slider.translations.map((t) => [t.languageCode, t.name])));

  function updateSlide(next: DraftSlide) {
    setSlides((current) => (current ?? []).map((slide) => (slide.tempKey === next.tempKey ? next : slide)));
  }

  async function handleSaveNames() {
    setIsSavingNames(true);
    try {
      await updateSlider(client, sliderId, {
        rowVersion: slider.rowVersion,
        translations: languages.map((language) => ({ languageCode: language.code, name: names[language.code] ?? '' })),
      });
      showToast('Slider adı kaydedildi.', 'success');
      setLoadedRowVersion(null);
      void queryClient.invalidateQueries({ queryKey: ['website-sliders'] });
      await refetch();
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
    } finally {
      setIsSavingNames(false);
    }
  }

  async function handleSaveSlides() {
    setIsSavingSlides(true);
    try {
      const fresh = await getSliderById(client, sliderId);
      if (fresh.rowVersion !== slider.rowVersion) {
        const proceed = window.confirm(
          'Bu slider siz düzenlerken sunucuda değişmiş. Listeyi yeniden yükleyip değişikliklerinizi kaybetmek ister misiniz?',
        );
        if (proceed) {
          setLoadedRowVersion(null);
          await refetch();
          setIsSavingSlides(false);
          return;
        }
      }

      await replaceSlides(client, sliderId, { rowVersion: slider.rowVersion, slides: draftSlidesToInput(currentSlides) });
      showToast('Slide listesi kaydedildi.', 'success');
      setLoadedRowVersion(null);
      void queryClient.invalidateQueries({ queryKey: ['website-sliders'] });
      await refetch();
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
      if ((mutationError as { status?: number }).status === 409) {
        setLoadedRowVersion(null);
        await refetch();
      }
    } finally {
      setIsSavingSlides(false);
    }
  }

  async function handleDeleteSlider() {
    try {
      await deleteSlider(client, sliderId);
      showToast('Slider silindi.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-sliders'] });
      navigate(ROUTES.websiteSliders);
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
    }
  }

  const editingSlide = editingTempKey ? currentSlides.find((s) => s.tempKey === editingTempKey) ?? null : null;

  return (
    <div className="container section">
      <PageHeader eyebrow="Site yönetimi" title={`Slider: ${data.key}`} />
      <UnsavedChangesGuard isDirty={slidesDirty || namesDirty} />

      <section>
        <h2>Ad (dil bazlı)</h2>
        <TranslationTabs
          languages={languages}
          activeCode={activeLanguage}
          onChange={setActiveLanguage}
          renderPanel={(languageCode) => (
            <TextField
              id={`slider-name-${languageCode}`}
              label="Ad"
              required
              value={names[languageCode] ?? ''}
              onChange={(event) => setNames({ ...names, [languageCode]: event.target.value })}
            />
          )}
        />
        <Button type="button" onClick={() => void handleSaveNames()} disabled={!namesDirty || isSavingNames}>
          {isSavingNames ? 'Kaydediliyor…' : 'Adı kaydet'}
        </Button>
      </section>

      <section>
        <h2>Slide'lar</h2>
        {currentSlides.length === 0 ? (
          <EmptyState title="Henüz slide yok" description="Slide ekleyin." />
        ) : (
          <ReorderList
            items={currentSlides}
            getKey={(slide) => slide.tempKey}
            getLabel={(slide) => slide.translations[0]?.title || '(başlıksız)'}
            onMove={(fromIndex, toIndex) =>
              setSlides((current) => {
                const list = [...(current ?? [])];
                const [moved] = list.splice(fromIndex, 1);
                list.splice(toIndex, 0, moved!);
                return list;
              })
            }
            renderItem={(slide) => (
              <div className="field-row" style={{ alignItems: 'center' }}>
                <strong>{slide.translations[0]?.title || '(başlıksız)'}</strong>
                <span className="field-hint">{slide.isActive ? 'Etkin' : 'Pasif'}</span>
                <Button type="button" tone="quiet" onClick={() => setEditingTempKey(slide.tempKey)}>
                  Düzenle
                </Button>
                <Button type="button" tone="danger" onClick={() => setDeletingTempKey(slide.tempKey)}>
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
              const draft = newDraftSlide();
              setSlides((current) => [...(current ?? []), draft]);
              setEditingTempKey(draft.tempKey);
            }}
          >
            Slide ekle
          </Button>
          <Button type="button" onClick={() => void handleSaveSlides()} disabled={!slidesDirty || isSavingSlides}>
            {isSavingSlides ? 'Kaydediliyor…' : 'Slide listesini kaydet'}
          </Button>
          <Button
            type="button"
            tone="quiet"
            onClick={() => {
              if (!confirmDiscardChanges(slidesDirty)) return;
              setLoadedRowVersion(null);
              void refetch();
            }}
          >
            Değişiklikleri geri al
          </Button>
        </div>
      </section>

      <section>
        <Button type="button" tone="danger" onClick={() => setIsDeleteSliderOpen(true)}>
          Slider'ı sil
        </Button>
      </section>

      <SlideEditDialog
        key={editingTempKey ?? 'closed'}
        open={editingTempKey !== null}
        slide={editingSlide}
        languages={languages}
        onSave={(next) => {
          updateSlide(next);
          setEditingTempKey(null);
        }}
        onCancel={() => setEditingTempKey(null)}
      />
      <ConfirmDialog
        open={deletingTempKey !== null}
        title="Slide'ı sil"
        description="Bu slide taslaktan kaldırılacak. Kaydetmeden listeyi terk ederseniz bu değişiklik uygulanmaz."
        onConfirm={() => {
          setSlides((current) => (current ?? []).filter((slide) => slide.tempKey !== deletingTempKey));
          setDeletingTempKey(null);
        }}
        onCancel={() => setDeletingTempKey(null)}
      />
      <ConfirmDialog
        open={isDeleteSliderOpen}
        title="Slider'ı sil"
        description="Bu slider ve tüm slide'ları kalıcı olarak silinecek. Bu slider'ı kullanan bir ana sayfa/içerik bloğu varsa bağlantısı kopar."
        onConfirm={() => {
          setIsDeleteSliderOpen(false);
          void handleDeleteSlider();
        }}
        onCancel={() => setIsDeleteSliderOpen(false)}
      />
    </div>
  );
}
