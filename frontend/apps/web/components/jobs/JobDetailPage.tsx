import { notFound } from 'next/navigation';
import { ApiError, getPublicJobDetail, publicCompanyLogoUrl } from '@gm/api-client';
import { SafeHtml } from '@gm/ui';
import { createServerApiClient } from '../../lib/apiClient';
import { CANDIDATE_PORTAL_URL } from '../../lib/portalUrls';

export async function fetchJobDetail(slug: string) {
  try {
    return await getPublicJobDetail(createServerApiClient(), slug);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound();
    throw error;
  }
}

/**
 * Şirket adı/logosu görünür, public şirket profili sayfası YOK (ADR-024/karar özeti §4);
 * askerlik tercihi görünür, cinsiyet tercihi hiç yoktur (backend yanıtında da alan yok).
 * "Başvur" aday portalına gider - backend'de ilana başvuru ucu yok (A10/B1, raporlanmış).
 */
export async function JobDetailPage({ slug }: { slug: string }) {
  const job = await fetchJobDetail(slug);

  return (
    <article className="container section content-detail job-detail">
      <header className="content-detail__header">
        <div className="job-detail__company">
          {job.company.hasLogo && <img src={publicCompanyLogoUrl(job.company.id)} alt={job.company.name} />}
          <p>{job.company.name}</p>
        </div>
        <h1>{job.title}</h1>
        <ul className="job-detail__meta">
          {job.provinceName && <li>{job.provinceName}</li>}
          {job.employmentTypeName && <li>{job.employmentTypeName}</li>}
          {job.workLocationTypeName && <li>{job.workLocationTypeName}</li>}
          {job.positionName && <li>{job.positionName}</li>}
          {job.departmentName && <li>{job.departmentName}</li>}
          {job.isForDisabledCandidates && <li>Engelli kontenjanı</li>}
        </ul>
      </header>

      {job.descriptionHtml && <SafeHtml className="content-detail__body" html={job.descriptionHtml} />}

      <dl className="job-detail__requirements">
        {job.experienceLevelName && (
          <div>
            <dt>Deneyim</dt>
            <dd>{job.experienceLevelName}</dd>
          </div>
        )}
        {job.educationLevelNames.length > 0 && (
          <div>
            <dt>Eğitim</dt>
            <dd>{job.educationLevelNames.join(', ')}</dd>
          </div>
        )}
        {job.drivingLicenseNames.length > 0 && (
          <div>
            <dt>Sürücü belgesi</dt>
            <dd>{job.drivingLicenseNames.join(', ')}</dd>
          </div>
        )}
        {job.militaryStatusPreferenceNames.length > 0 && (
          <div>
            <dt>Askerlik durumu</dt>
            <dd>{job.militaryStatusPreferenceNames.join(', ')}</dd>
          </div>
        )}
        {job.languageRequirements.length > 0 && (
          <div>
            <dt>Dil</dt>
            <dd>
              {job.languageRequirements.map((req, index) => (
                <span key={index}>
                  {req.languageName}
                  {req.levelName && ` (${req.levelName})`}
                  {index < job.languageRequirements.length - 1 && ', '}
                </span>
              ))}
            </dd>
          </div>
        )}
      </dl>

      <a href={CANDIDATE_PORTAL_URL} className="button button--primary">
        Başvur
      </a>
    </article>
  );
}
