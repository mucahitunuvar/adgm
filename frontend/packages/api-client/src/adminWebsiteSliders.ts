import type { ApiClient } from './client';
import type { components } from './schema';

export type SliderSummaryResponse = components['schemas']['SliderSummaryResponse'];
export type SliderDetailResponse = components['schemas']['SliderDetailResponse'];
export type SlideResponse = components['schemas']['SlideResponse'];
export type SlideLinkResponse = components['schemas']['SlideLinkResponse'];
export type SliderTranslationResponse = components['schemas']['SliderTranslationResponse'];
export type SlideTranslationResponse = components['schemas']['SlideTranslationResponse'];
export type CreateSliderRequest = components['schemas']['CreateSliderRequest'];
export type CreateSliderResponse = components['schemas']['CreateSliderResponse'];
export type UpdateSliderRequest = components['schemas']['UpdateSliderRequest'];
export type SliderTranslationInput = components['schemas']['SliderTranslationInput'];
export type ReplaceSlidesRequest = components['schemas']['ReplaceSlidesRequest'];
export type SlideInput = components['schemas']['SlideInput'];
export type SlideLinkInput = components['schemas']['SlideLinkInput'];
export type SlideTranslationInput = components['schemas']['SlideTranslationInput'];

export function getSliders(client: ApiClient): Promise<SliderSummaryResponse[]> {
  return client.request('/api/v1/admin/website/sliders');
}

export function getSliderById(client: ApiClient, id: string): Promise<SliderDetailResponse> {
  return client.request(`/api/v1/admin/website/sliders/${encodeURIComponent(id)}`);
}

export function createSlider(client: ApiClient, body: CreateSliderRequest): Promise<CreateSliderResponse> {
  return client.request('/api/v1/admin/website/sliders', { method: 'POST', body });
}

/** Yalnız çeviriler (ad) değişir - slide'lar ayrı uçtan (`replaceSlides`). */
export function updateSlider(client: ApiClient, id: string, body: UpdateSliderRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/sliders/${encodeURIComponent(id)}`, { method: 'PUT', body });
}

export function deleteSlider(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/sliders/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

/** Tüm slide listesini değiştirir (ekleme/çıkarma/sıralama ayrı uç değil). `rowVersion` eşleşmezse 409. */
export function replaceSlides(client: ApiClient, sliderId: string, body: ReplaceSlidesRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/sliders/${encodeURIComponent(sliderId)}/slides`, { method: 'PUT', body });
}
