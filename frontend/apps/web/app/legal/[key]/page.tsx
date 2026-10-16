import { notFound } from 'next/navigation';
import type { Metadata } from 'next';
import { ApiError, getPublicLegalDocument } from '@gm/api-client';
import { SafeHtml, formatDateTime } from '@gm/ui';
import { createServerApiClient } from '../../../lib/apiClient';

interface LegalDocumentPageProps {
  params: Promise<{ key: string }>;
}

async function fetchDocument(key: string) {
  try {
    return await getPublicLegalDocument(createServerApiClient(), key);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound();
    throw error;
  }
}

export async function generateMetadata({ params }: LegalDocumentPageProps): Promise<Metadata> {
  const { key } = await params;
  const document = await fetchDocument(key);
  return { title: document.title };
}

/**
 * ADR-024 §12.1: yayınlanan sürüm değiştirilemez, her zaman yürürlükteki sürüm döner. Menü/form
 * bağlantıları bu sabit `/legal/{key}` yoluna `InternalPath` (`LinkTarget`) olarak işaret eder -
 * backend `LegalDocument`'ı bir `ContentItem`/yol ile ilişkilendirmez, bu yüzden frontend'in kendi
 * sabit rotasıdır (`/giris`, `/newsletter/confirm` ile aynı desen).
 */
export default async function LegalDocumentPage({ params }: LegalDocumentPageProps) {
  const { key } = await params;
  const document = await fetchDocument(key);

  return (
    <article className="container section content-detail">
      <h1>{document.title}</h1>
      <p className="content-detail__date">Yürürlük tarihi: {formatDateTime(document.effectiveAtUtc)}</p>
      <SafeHtml className="content-detail__body" html={document.body} />
    </article>
  );
}
