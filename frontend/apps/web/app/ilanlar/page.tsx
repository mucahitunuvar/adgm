import { notFound } from 'next/navigation';
import type { Metadata } from 'next';
import {
  listPublicJobs,
  listPublicProvinces,
  listPublicPositions,
  listPublicDepartments,
  listPublicEmploymentTypes,
  listPublicWorkLocationTypes,
} from '@gm/api-client';
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

// GET formunun kendi gönderimi, seçilmemiş her alan için BOŞ bir değer (ör. `provinceId=`) üretir -
// bu, `undefined`'dan (hiç gönderilmemiş parametre) farklıdır ve GUID bekleyen filtrelere (`provinceId`
// vb.) öylece iletilirse backend'de 500'e düşer (minimal API model binding boş dizgeyi `Guid?`'e
// çözemez). Bu yüzden her okuma boş dizgeyi de `undefined`'a indirger - yalnız `q`/sayfalama değil,
// HER filtre parametresi için.
function firstParam(value: string | string[] | undefined): string | undefined {
  const first = Array.isArray(value) ? value[0] : value;
  return first ? first : undefined;
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
  const provinceId = firstParam(query.provinceId);
  const positionId = firstParam(query.positionId);
  const departmentId = firstParam(query.departmentId);
  const employmentTypeId = firstParam(query.employmentTypeId);
  const workLocationTypeId = firstParam(query.workLocationTypeId);

  const [result, provinces, positions, departments, employmentTypes, workLocationTypes] = await Promise.all([
    listPublicJobs(client, {
      q,
      isForDisabledCandidates: isForDisabledCandidates || undefined,
      provinceId,
      positionId,
      departmentId,
      employmentTypeId,
      workLocationTypeId,
      page,
      pageSize: PAGE_SIZE,
    }),
    listPublicProvinces(client),
    listPublicPositions(client),
    listPublicDepartments(client),
    listPublicEmploymentTypes(client),
    listPublicWorkLocationTypes(client),
  ]);

  const filterQuery = {
    q,
    isForDisabledCandidates: isForDisabledCandidates ? 'true' : undefined,
    provinceId,
    positionId,
    departmentId,
    employmentTypeId,
    workLocationTypeId,
  };

  return (
    <div className="container section content-list-page">
      <h1>İş İlanları</h1>
      <JobSearchForm
        defaultQuery={q}
        defaultDisabledOnly={isForDisabledCandidates}
        provinces={provinces.items}
        positions={positions.items}
        departments={departments.items}
        employmentTypes={employmentTypes.items}
        workLocationTypes={workLocationTypes.items}
        defaultProvinceId={provinceId}
        defaultPositionId={positionId}
        defaultDepartmentId={departmentId}
        defaultEmploymentTypeId={employmentTypeId}
        defaultWorkLocationTypeId={workLocationTypeId}
      />

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
        query={filterQuery}
      />
    </div>
  );
}
