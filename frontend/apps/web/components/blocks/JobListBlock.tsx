import { listPublicJobs, type PublicJobListItemResponse } from '@gm/api-client';
import { safeUrl } from '@gm/ui';
import { createServerApiClient } from '../../lib/apiClient';

/**
 * `job-list` bloğunun `data`'sı her zaman `null`'dır (ADR-024 §8.1: backend ilan verisini
 * kopyalamaz) - bu bileşen kendi `GET /public/jobs` çağrısını yapar, blok yalnız `count`/metin
 * ayarlarını taşır. `publicJobListingsEnabled` kapalıyken `HomePage` bu bloğu zaten çağırmaz.
 */
export async function JobListBlock({ texts, settings }: { texts: { title?: string; moreLabel?: string } | null; settings: { count?: number } | null }) {
  const client = createServerApiClient();
  const result = await listPublicJobs(client, { pageSize: settings?.count ?? 6 });
  if (result.items.length === 0) return null;

  return (
    <section className="container section block-job-list">
      <div className="block-content-list__header">
        {texts?.title && <h2>{texts.title}</h2>}
        {texts?.moreLabel && (
          <a className="button button--quiet" href="/ilanlar">
            {texts.moreLabel}
          </a>
        )}
      </div>
      <div className="block-content-list__grid">
        {result.items.map((job) => (
          <JobCard key={job.id} job={job} />
        ))}
      </div>
    </section>
  );
}

function JobCard({ job }: { job: PublicJobListItemResponse & { company: { hasLogo?: boolean } } }) {
  const href = safeUrl(`/ilanlar/${job.slug}`);
  return (
    <a href={href} className="content-card job-card">
      {job.company.hasLogo && <img src={`/api/v1/public/companies/${job.company.id}/logo`} alt={job.company.name} loading="lazy" />}
      <h3>{job.title}</h3>
      <p>{job.company.name}</p>
      {job.provinceName && <p className="field-hint">{job.provinceName}</p>}
    </a>
  );
}
