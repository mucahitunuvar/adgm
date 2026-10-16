import { notFound } from 'next/navigation';
import type { Metadata } from 'next';
import { listPublicJobs } from '@gm/api-client';
import { createServerApiClient } from '../../lib/apiClient';
import { getSite } from '../../lib/getSite';
import { JobCard } from '../../components/jobs/JobCard';
import { JobSearchForm } from '../../components/jobs/JobSearchForm';
import { LinkPagination } from '../../components/content/LinkPagination';

export const metadata: Metadata = { title: 'İş İlanları', description: 'Açık iş ilanlarını görüntüleyin ve arayın.' };

const PAGE_SIZE = 12;

interface JobListingsPageProps {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}

function firstParam(value: string | string[] | undefined): string | undefined {
  return Array.isArray(value) ? value[0] : value;
}

/**
 * `ilanlar` ADR-024 §4.1'de frontend'in kendi sabit yolu olarak rezerve edilmiştir (Employer
 * modülünün ilan verisi ContentType değildir) - yakalayıcı `[[...slug]]`'dan önce buraya düşer.
 */
export default async function JobListingsPage({ searchParams }: JobListingsPageProps) {
  const client = createServerApiClient();
  const site = await getSite();
  if (!site.publicJobListingsEnabled) notFound();

  const query = await searchParams;
  const q = firstParam(query.q);
  const isForDisabledCandidates = firstParam(query.isForDisabledCandidates) === 'true';
  const page = Number(firstParam(query.page)) || 1;

  const result = await listPublicJobs(client, {
    q,
    isForDisabledCandidates: isForDisabledCandidates || undefined,
    page,
    pageSize: PAGE_SIZE,
  });

  return (
    <div className="container section content-list-page">
      <h1>İş İlanları</h1>
      <JobSearchForm defaultQuery={q} defaultDisabledOnly={isForDisabledCandidates} />

      {result.items.length === 0 ? (
        <p className="page-lead">Aramanıza uygun ilan bulunamadı.</p>
      ) : (
        <div className="block-content-list__grid">
          {result.items.map((job) => (
            <JobCard key={job.id} job={job} />
          ))}
        </div>
      )}

      <LinkPagination
        basePath="/ilanlar"
        page={Number(result.page)}
        pageSize={Number(result.pageSize)}
        totalCount={Number(result.totalCount)}
        query={{ q, isForDisabledCandidates: isForDisabledCandidates ? 'true' : undefined }}
      />
    </div>
  );
}
