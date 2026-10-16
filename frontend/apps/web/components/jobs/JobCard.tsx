import { safeUrl, formatDateTime } from '@gm/ui';
import { publicCompanyLogoUrl } from '@gm/api-client';
import type { PublicJobListItemResponse } from '@gm/api-client';

export function JobCard({ job }: { job: PublicJobListItemResponse }) {
  const href = safeUrl(`/ilanlar/${job.slug}`);
  return (
    <a href={href} className="content-card job-card">
      {job.company.hasLogo && <img src={publicCompanyLogoUrl(job.company.id)} alt={job.company.name} loading="lazy" />}
      <h3>{job.title}</h3>
      <p>{job.company.name}</p>
      <p className="content-card__date">{formatDateTime(job.publishedAtUtc)}</p>
      <ul className="job-card__meta">
        {job.provinceName && <li>{job.provinceName}</li>}
        {job.employmentTypeName && <li>{job.employmentTypeName}</li>}
        {job.workLocationTypeName && <li>{job.workLocationTypeName}</li>}
      </ul>
    </a>
  );
}
