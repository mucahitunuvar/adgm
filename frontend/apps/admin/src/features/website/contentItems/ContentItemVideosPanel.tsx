import { getApiErrorMessage, getVideos, setContentItemVideos, type ContentItemDetailResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, useToast } from '@gm/ui';
import { useMutation, useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { useVideoPicker } from './useVideoPicker';

const MAX_VIDEOS = 20;

export function ContentItemVideosPanel({
  contentItemId,
  detail,
  onSaved,
}: {
  contentItemId: string;
  detail: ContentItemDetailResponse;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const { pickVideo, pickerElement } = useVideoPicker();
  const [videoIds, setVideoIds] = useState<string[]>([...detail.videoIds]);
  const isDirty = JSON.stringify(videoIds) !== JSON.stringify(detail.videoIds);

  // Seçili videoların başlığını göstermek için kütüphanenin tamamı çekiliyor (video sayısı
  // yönetilebilir ölçekte bekleniyor, `GetVideos` için ayrı bir "id listesiyle getir" ucu yok).
  const { data: allVideos } = useQuery({ queryKey: ['website-videos-all'], queryFn: () => getVideos(client, { pageSize: 200 }) });
  const titleById = new Map((allVideos?.items ?? []).map((v) => [v.id, v.title]));

  const saveMutation = useMutation({
    mutationFn: () => setContentItemVideos(client, contentItemId, { rowVersion: detail.rowVersion, videoIds }),
    onSuccess: () => {
      showToast('Video listesi güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  async function handleAdd() {
    const video = await pickVideo();
    if (!video || videoIds.includes(video.id)) return;
    setVideoIds([...videoIds, video.id]);
  }

  function handleRemove(id: string) {
    setVideoIds(videoIds.filter((x) => x !== id));
  }

  function handleMove(index: number, direction: -1 | 1) {
    const target = index + direction;
    if (target < 0 || target >= videoIds.length) return;
    const next = [...videoIds];
    [next[index], next[target]] = [next[target]!, next[index]!];
    setVideoIds(next);
  }

  return (
    <div>
      <ul className="field-list">
        {videoIds.map((id, index) => (
          <li key={id} className="field-row">
            <span>{titleById.get(id) ?? id}</span>
            <Button type="button" tone="quiet" onClick={() => handleMove(index, -1)} disabled={index === 0}>
              Yukarı
            </Button>
            <Button type="button" tone="quiet" onClick={() => handleMove(index, 1)} disabled={index === videoIds.length - 1}>
              Aşağı
            </Button>
            <Button type="button" tone="quiet" onClick={() => handleRemove(id)}>
              Kaldır
            </Button>
          </li>
        ))}
      </ul>
      <Button type="button" tone="secondary" onClick={() => void handleAdd()} disabled={videoIds.length >= MAX_VIDEOS}>
        Video ekle
      </Button>{' '}
      <Button type="button" onClick={() => saveMutation.mutate()} disabled={!isDirty || saveMutation.isPending}>
        {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
      {pickerElement}
    </div>
  );
}
