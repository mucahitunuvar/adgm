import { SelectField } from '@gm/ui';
import type { PublicLookupItemResponse } from '@gm/api-client';

interface JobSearchFormProps {
  defaultQuery?: string;
  defaultDisabledOnly?: boolean;
  provinces: PublicLookupItemResponse[];
  positions: PublicLookupItemResponse[];
  departments: PublicLookupItemResponse[];
  employmentTypes: PublicLookupItemResponse[];
  workLocationTypes: PublicLookupItemResponse[];
  defaultProvinceId?: string;
  defaultPositionId?: string;
  defaultDepartmentId?: string;
  defaultEmploymentTypeId?: string;
  defaultWorkLocationTypeId?: string;
}

function LookupSelect({
  id,
  name,
  label,
  items,
  defaultValue,
}: {
  id: string;
  name: string;
  label: string;
  items: PublicLookupItemResponse[];
  defaultValue?: string;
}) {
  return (
    <SelectField id={id} name={name} label={label} defaultValue={defaultValue ?? ''}>
      <option value="">Tümü</option>
      {items.map((item) => (
        <option key={item.id} value={item.id}>
          {item.name}
        </option>
      ))}
    </SelectField>
  );
}

/**
 * JS'siz GET formu (`FRONTEND-ACIK-MADDELER.md` madde 39'un kapanışı): il/pozisyon/departman/çalışma
 * şekli/çalışma yeri tipi dropdown'ları artık `GET /public/reference-data/*` uçlarından (yalnız
 * `{id, name}`, `apps/web/app/ilanlar/page.tsx`'te SSR ile 60 sn önbellekli çekilir) besleniyor -
 * seçenekte AD gösterilir, formun `value`'su (ve dolayısıyla gönderilen URL/sorgu parametresi) hep
 * backend'in `GetPublicJobsQuery`'sinin beklediği GUID'dir.
 */
export function JobSearchForm({
  defaultQuery,
  defaultDisabledOnly,
  provinces,
  positions,
  departments,
  employmentTypes,
  workLocationTypes,
  defaultProvinceId,
  defaultPositionId,
  defaultDepartmentId,
  defaultEmploymentTypeId,
  defaultWorkLocationTypeId,
}: JobSearchFormProps) {
  return (
    <form method="GET" action="/ilanlar" className="job-search-form" role="search">
      <label htmlFor="job-search-input" className="sr-only">
        İlan ara
      </label>
      <input id="job-search-input" type="search" name="q" defaultValue={defaultQuery} placeholder="Pozisyon veya şirket adı ara..." minLength={2} maxLength={100} />

      <LookupSelect id="job-search-province" name="provinceId" label="İl" items={provinces} defaultValue={defaultProvinceId} />
      <LookupSelect id="job-search-position" name="positionId" label="Pozisyon" items={positions} defaultValue={defaultPositionId} />
      <LookupSelect id="job-search-department" name="departmentId" label="Departman" items={departments} defaultValue={defaultDepartmentId} />
      <LookupSelect
        id="job-search-employment-type"
        name="employmentTypeId"
        label="Çalışma şekli"
        items={employmentTypes}
        defaultValue={defaultEmploymentTypeId}
      />
      <LookupSelect
        id="job-search-work-location-type"
        name="workLocationTypeId"
        label="Çalışma yeri tipi"
        items={workLocationTypes}
        defaultValue={defaultWorkLocationTypeId}
      />

      <label className="checkbox-field checkbox-field--inline">
        <input type="checkbox" name="isForDisabledCandidates" value="true" defaultChecked={defaultDisabledOnly} />
        <span>Engelli kontenjanı</span>
      </label>
      <button type="submit" className="button button--primary">
        Ara
      </button>
    </form>
  );
}
