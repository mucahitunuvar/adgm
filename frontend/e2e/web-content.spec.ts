import AxeBuilder from '@axe-core/playwright';
import { test, expect, type APIRequestContext } from '@playwright/test';

// KOŞU 3F madde 4: kalıcı Playwright e2e - gerçek backend + seed-dev (ana sayfa, içerik liste/detay,
// etkinlik liste/detay, ilan liste/detay, form gönderimi, yasal belge, çerez onayı, 404, yönlendirme,
// canonical, CSP/nonce, prod axe). Bakım modu (503→200) kasıtlı olarak BURADA tekrarlanmıyor -
// web-home.spec.ts'in kendi `mode: 'serial'` testi zaten bunu kapsıyor; aynı SiteSettings tekil
// satırını iki ayrı dosyadan paralel aç/kapat etmek madde 30'daki bilinen yarış koşulunu tekrar
// üretir (iki dosya `fullyParallel: true` altında farklı worker'larda aynı anda çalışabilir).
//
// Bu dosyanın tüm testleri TEK bir `beforeAll`'da kurulan paylaşılan sabit veriye (çerez politikası,
// aydınlatma metni, form, yönlendirmeler, ilan) dayanır - `mode: 'serial'` bu yüzden zorunlu (paralel
// çalışırsa `afterAll` bir test hâlâ çalışırken devreye girebilir). `afterAll` oluşturduğu HER ŞEYİ
// (redirect'ler, form+içerik, ilan onay zinciri değil - ilan backend'de soft-state, silinmiyor ama
// "Seed"/"E2E" önekiyle işaretli ve zaten seed-dev'in dokunmadığı Employer modülünde) ve değiştirdiği
// SiteSettings alanlarını (çerez politikası anahtarı) ORİJİNAL değerine geri döndürür.
const WEB_URL = 'http://localhost:3000';
const BACKEND_URL = 'http://localhost:5016';
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';
const RUN_ID = Date.now();

test.describe.configure({ mode: 'serial' });

interface LegalDraftDetail {
  rowVersion: string;
}

function authHeaders(token: string) {
  return { Authorization: `Bearer ${token}` };
}

async function loginAdmin(request: APIRequestContext): Promise<string> {
  const response = await request.post(`${BACKEND_URL}/api/v1/auth/login`, {
    data: { email: ADMIN_EMAIL, password: ADMIN_PASSWORD },
  });
  const body = (await response.json()) as { accessToken: string };
  return body.accessToken;
}

async function loginAs(request: APIRequestContext, email: string, password: string): Promise<string> {
  const response = await request.post(`${BACKEND_URL}/api/v1/auth/login`, { data: { email, password } });
  const body = (await response.json()) as { accessToken: string };
  return body.accessToken;
}

// Yayınlanmış bir `LegalDocument` ASLA silinemez (backend, ADR-024 §12.1 - denetim izi için kasıtlı
// bir kısıt: `DELETE` "LegalDocument.HasPublishedVersion" 409'u verir). Bu yüzden RUN_ID'li, her
// koşuda YENİ bir anahtar OLUŞTURMAK yerine SABİT bir anahtarla "bul, yoksa oluştur" yapılır - tıpkı
// `scripts/seed-dev.mjs`'in "Seed " önekli kayıtları kalıcı tutup yeniden kullanması gibi; aksi halde
// her koşu temizlenemeyen bir belge daha biriktirir.
async function getOrCreatePublishedLegalDocument(
  request: APIRequestContext,
  token: string,
  kind: 'CookiePolicy' | 'PrivacyNotice',
): Promise<{ key: string; id: string }> {
  const key = `e2e-${kind.toLowerCase()}-fixture`;

  const existing = (await (
    await request.get(`${BACKEND_URL}/api/v1/admin/website/legal-documents`, { headers: authHeaders(token) })
  ).json()) as { id: string; key: string; effectiveVersionNumber: number | null }[];
  const found = existing.find((d) => d.key === key);
  if (found && found.effectiveVersionNumber != null) {
    return { key, id: found.id };
  }

  const createResponse = found
    ? { ok: () => true, json: async () => ({ id: found.id }) }
    : await request.post(`${BACKEND_URL}/api/v1/admin/website/legal-documents`, {
        headers: authHeaders(token),
        data: { key, kind, defaultLanguageTitle: `E2E ${kind}` },
      });
  expect(createResponse.ok(), `create legal document (${kind})`).toBeTruthy();
  const created = (await createResponse.json()) as { id: string };

  async function getDetail(): Promise<LegalDraftDetail> {
    const response = await request.get(`${BACKEND_URL}/api/v1/admin/website/legal-documents/${created.id}`, {
      headers: authHeaders(token),
    });
    return (await response.json()) as LegalDraftDetail;
  }

  let detail = await getDetail();
  await request.post(`${BACKEND_URL}/api/v1/admin/website/legal-documents/${created.id}/versions/draft`, {
    headers: authHeaders(token),
    data: { rowVersion: detail.rowVersion, changeSummary: null },
  });

  detail = await getDetail();
  await request.put(`${BACKEND_URL}/api/v1/admin/website/legal-documents/${created.id}/versions/draft/translations/tr`, {
    headers: authHeaders(token),
    data: { rowVersion: detail.rowVersion, body: '<p>E2E test aydınlatma metni gövdesi.</p>' },
  });

  detail = await getDetail();
  const publishResponse = await request.post(
    `${BACKEND_URL}/api/v1/admin/website/legal-documents/${created.id}/versions/draft/publish`,
    { headers: authHeaders(token), data: { rowVersion: detail.rowVersion, effectiveAtUtc: null } },
  );
  expect(publishResponse.status(), `publish legal document (${kind})`).toBe(204);

  return { key, id: created.id as string };
}

interface SetupState {
  adminToken: string;
  originalCookiePolicyKey: string | null;
  cookiePolicyDocumentId: string;
  privacyNoticeDocumentId: string;
  formPagePath: string;
  formKey: string;
  formDefinitionId: string;
  formContentItemId: string;
  validRedirectFromPath: string;
  invalidRedirectFromPath: string;
  redirectIds: string[];
  jobSlug: string;
  originalPublicJobListingsEnabled: boolean;
  originalBotProtectionEnabled: boolean;
}

let setup: SetupState;

test.beforeAll(async ({ request }) => {
  // Varsayılan 30 sn hook zaman aşımı yetmez: kurulum, `apps/web`'in `getSite()` önbelleğinin (60 sn
  // `revalidate`) güncel SiteSettings'i görmesini kasıtlı olarak bekliyor (yukarıda açıklandı).
  test.setTimeout(150_000);

  const adminToken = await loginAdmin(request);

  // --- Bot koruması (form/çerez/bülten gönderimi testleri için) ---
  // KOŞU 3F'de bulundu: bu dev ortamında `BotProtectionEnabled: true` ama `turnstileSiteKey: ""` -
  // widget hiç render edilmez (`{turnstileSiteKey && <TurnstileWidget/>}`), yani HİÇBİR gönderim
  // `turnstileToken` taşıyamaz ve backend'in `IBotProtectionVerifier` kontrolü her zaman reddeder -
  // önceden var olan, bu dosyanın dokunmadığı bir ortam yapılandırma tutarsızlığı. `PublicSubmissionGuard`
  // bu bayrağı `SiteSettingsRepository`den DOĞRUDAN (önbelleksiz) okur, bu yüzden bu değişiklik için
  // `getSite()` önbellek beklemesi gerekmez.
  const botProtectionBefore = (await (
    await request.get(`${BACKEND_URL}/api/v1/admin/website/settings`, { headers: authHeaders(adminToken) })
  ).json()) as { rowVersion: string; botProtectionEnabled: boolean; turnstileSiteKey: string };
  const disableBotProtectionResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/settings/bot-protection`, {
    headers: authHeaders(adminToken),
    data: { rowVersion: botProtectionBefore.rowVersion, botProtectionEnabled: false, turnstileSiteKey: botProtectionBefore.turnstileSiteKey },
  });
  expect(disableBotProtectionResponse.status(), 'disable bot protection').toBe(204);

  // --- İlan (job): /ilanlar sayfası PublicJobListingsEnabled kapalıyken boş kalır - açık olduğundan
  // emin ol (dev'de şu an kapalı), sonunda orijinal değerine döndürülür. En başta yapılır çünkü
  // `apps/web`'in `getSite()`'ı bu yanıtı 60 sn `revalidate` ile önbelleğe alır (`lib/getSite.ts`) -
  // kurulumun geri kalanının aldığı gerçek zaman, aşağıdaki açık bekleme süresini kısaltır.
  const flagFlippedAtMs = Date.now();
  const featuresBefore = (await (
    await request.get(`${BACKEND_URL}/api/v1/admin/website/settings`, { headers: authHeaders(adminToken) })
  ).json()) as {
    rowVersion: string;
    globalSearchEnabled: boolean;
    newsletterEnabled: boolean;
    publicJobListingsEnabled: boolean;
    donationPageEnabled: boolean;
    allowSearchEngineIndexing: boolean;
  };
  const enableJobListingsResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/settings/features`, {
    headers: authHeaders(adminToken),
    data: {
      rowVersion: featuresBefore.rowVersion,
      globalSearchEnabled: featuresBefore.globalSearchEnabled,
      newsletterEnabled: featuresBefore.newsletterEnabled,
      publicJobListingsEnabled: true,
      donationPageEnabled: featuresBefore.donationPageEnabled,
      allowSearchEngineIndexing: featuresBefore.allowSearchEngineIndexing,
    },
  });
  expect(enableJobListingsResponse.status(), 'enable public job listings').toBe(204);

  // --- Çerez politikası (çerez onayı akışı testi için) ---
  const settingsBefore = (await (
    await request.get(`${BACKEND_URL}/api/v1/admin/website/settings`, { headers: authHeaders(adminToken) })
  ).json()) as { rowVersion: string; cookiePolicyKey: string | null };
  const cookiePolicy = await getOrCreatePublishedLegalDocument(request, adminToken, 'CookiePolicy');
  const cookieConfigResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/settings/cookie-consent`, {
    headers: authHeaders(adminToken),
    data: {
      rowVersion: settingsBefore.rowVersion,
      cookiePolicyKey: cookiePolicy.key,
      translations: [
        {
          languageCode: 'tr',
          bannerTitle: 'E2E Çerez Onayı',
          bannerText: 'Bu test sitesi çerez kullanır.',
          cookieCategoryNecessaryDescription: 'Gerekli',
          cookieCategoryAnalyticsDescription: 'Analitik',
          cookieCategoryMarketingDescription: 'Pazarlama',
        },
      ],
    },
  });
  expect(cookieConfigResponse.status(), 'configure cookie policy').toBe(204);
  const cookiePolicyConfiguredAtMs = Date.now();

  // --- Aydınlatma metni + form + form'un bağlı olduğu içerik (form gönderimi testi için) ---
  const privacyNotice = await getOrCreatePublishedLegalDocument(request, adminToken, 'PrivacyNotice');

  const formKey = `e2e-form-${RUN_ID}`;
  const createFormResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/forms`, {
    headers: authHeaders(adminToken),
    data: {
      key: formKey,
      retentionDays: 30,
      notificationEmails: [],
      privacyNoticeKey: privacyNotice.key,
      explicitConsents: [],
      defaultLanguageTitle: 'E2E İletişim Formu',
      defaultLanguageDescription: 'E2E test formu.',
      defaultLanguageSuccessMessage: 'Gönderiminiz alındı.',
      defaultLanguageSubmitButtonLabel: 'Gönder',
    },
  });
  expect(createFormResponse.ok(), 'create form definition').toBeTruthy();
  const createdForm = (await createFormResponse.json()) as { id: string };

  async function getFormDetail() {
    const response = await request.get(`${BACKEND_URL}/api/v1/admin/website/forms/${createdForm.id}`, {
      headers: authHeaders(adminToken),
    });
    return (await response.json()) as { rowVersion: string };
  }

  let formDetail = await getFormDetail();
  const setFieldsResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/forms/${createdForm.id}/fields`, {
    headers: authHeaders(adminToken),
    data: {
      rowVersion: formDetail.rowVersion,
      fields: [
        {
          key: 'email',
          type: 'Email',
          isRequired: true,
          sortOrder: 1,
          minLength: null,
          maxLength: null,
          options: [],
          dateMin: null,
          dateMax: null,
          allowedFileTypes: [],
          maxSizeMb: null,
          translations: [{ languageCode: 'tr', label: 'E-posta', placeholder: null, helpText: null }],
        },
      ],
    },
  });
  expect(setFieldsResponse.status(), 'set form fields').toBe(204);

  formDetail = await getFormDetail();
  const activateResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/forms/${createdForm.id}/activate`, {
    headers: authHeaders(adminToken),
    data: { rowVersion: formDetail.rowVersion },
  });
  expect(activateResponse.status(), 'activate form').toBe(204);

  const contentTypes = (await (
    await request.get(`${BACKEND_URL}/api/v1/admin/website/content-types`, { headers: authHeaders(adminToken) })
  ).json()) as { id: string; key: string }[];
  const pageTypeId = contentTypes.find((t) => t.key === 'page')!.id;

  const formSlug = `e2e-form-sayfasi-${RUN_ID}`;
  const createContentResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/contents`, {
    headers: authHeaders(adminToken),
    data: {
      contentTypeId: pageTypeId,
      parentId: null,
      sortOrder: 1,
      isFeatured: false,
      coverImageMediaId: null,
      detailImageMediaId: null,
      defaultLanguageTitle: 'E2E Form Sayfası',
      defaultLanguageSlug: formSlug,
      defaultLanguageSummary: null,
      defaultLanguageBody: '<p>Aşağıdaki formu doldurun.</p>',
      seo: {
        metaTitle: null,
        metaDescription: null,
        ogTitle: null,
        ogDescription: null,
        ogImageMediaId: null,
        canonicalUrl: null,
        noIndex: false,
      },
    },
  });
  expect(createContentResponse.ok(), 'create form content item').toBeTruthy();
  const createdContent = (await createContentResponse.json()) as { id: string; fullPath: string };

  async function getContentDetail() {
    const response = await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${createdContent.id}`, {
      headers: authHeaders(adminToken),
    });
    return (await response.json()) as { rowVersion: string };
  }

  let contentDetail = await getContentDetail();
  const setFormResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/contents/${createdContent.id}/form`, {
    headers: authHeaders(adminToken),
    data: { rowVersion: contentDetail.rowVersion, formDefinitionId: createdForm.id },
  });
  expect(setFormResponse.status(), 'attach form to content item').toBe(204);

  contentDetail = await getContentDetail();
  const publishContentResponse = await request.post(
    `${BACKEND_URL}/api/v1/admin/website/contents/${createdContent.id}/publish`,
    { headers: authHeaders(adminToken), data: { rowVersion: contentDetail.rowVersion, publishAtUtc: null, unpublishAtUtc: null } },
  );
  expect(publishContentResponse.status(), 'publish form content item').toBe(204);

  // --- Yönlendirmeler (301 geçerli + açık yönlendirme denemesi) ---
  const validRedirectFromPath = `e2e-eski-sayfa-${RUN_ID}`;
  const validRedirectResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/redirects`, {
    headers: authHeaders(adminToken),
    data: { languageCode: 'tr', fromPath: validRedirectFromPath, targetKind: 'Path', targetContentItemId: null, targetPath: 'haberler', statusCode: 'MovedPermanently' },
  });
  expect(validRedirectResponse.ok(), 'create valid redirect').toBeTruthy();
  const validRedirect = (await validRedirectResponse.json()) as { id: string };

  // Madde 2 (açık yönlendirme) regresyon kanıtı: `TargetPath` yalnız `Trim('/')` ile normalize edilir
  // (ters eğik çizgi dokunulmaz, bkz. Redirect.NormalizeTargetPath) - "\evil.example" hiçbir "/"
  // taşımadığı için backend'in kendi doğrulamasını (https zorunluluğu yalnız MUTLAK URI'lere uygulanır)
  // GEÇER ve ResolveRoute'ta `RoutePathFormat.BuildPublicPath` ile "/\evil.example" olarak üretilir -
  // tam olarak `safeRedirectTarget`'ın reddetmesi gereken biçim. Bu, gerçek admin API'siyle üretilebilen
  // GERÇEK bir senaryo, kurgusal değil.
  const invalidRedirectFromPath = `e2e-zararli-yonlendirme-${RUN_ID}`;
  const invalidRedirectResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/redirects`, {
    headers: authHeaders(adminToken),
    data: {
      languageCode: 'tr',
      fromPath: invalidRedirectFromPath,
      targetKind: 'Path',
      targetContentItemId: null,
      targetPath: '\\evil.example.com',
      statusCode: 'Found',
    },
  });
  expect(invalidRedirectResponse.ok(), 'create malicious-shaped redirect').toBeTruthy();
  const invalidRedirect = (await invalidRedirectResponse.json()) as { id: string };

  // --- İlan (job): işveren kaydı + onay + ilan oluştur + gönder + danışman onayı ---
  const advisorEmail = `e2e-danisman-${RUN_ID}@example.com`;
  const advisorPassword = 'Sifre123';
  const createAdvisorResponse = await request.post(`${BACKEND_URL}/api/v1/admin/career-advisors`, {
    headers: authHeaders(adminToken),
    data: { email: advisorEmail, password: advisorPassword, firstName: 'E2E', lastName: 'Danışman', phoneNumber: null },
  });
  expect(createAdvisorResponse.ok(), 'create career advisor').toBeTruthy();
  const createdAdvisor = (await createAdvisorResponse.json()) as { careerAdvisorId: string };

  const employerEmail = `e2e-firma-${RUN_ID}@example.com`;
  const employerPassword = 'Sifre123';
  const registerEmployerResponse = await request.post(`${BACKEND_URL}/api/v1/employer/register`, {
    data: {
      email: employerEmail,
      password: employerPassword,
      name: `E2E Firma ${RUN_ID}`,
      sectorId: crypto.randomUUID(),
      foundedYear: null,
      employeeCount: null,
      websiteUrl: null,
      countryId: crypto.randomUUID(),
      provinceId: crypto.randomUUID(),
      districtId: crypto.randomUUID(),
      address: 'E2E Test Adresi',
      aboutHtml: null,
      contactFirstName: 'E2E',
      contactLastName: 'Yetkili',
      contactPhone: '05551234567',
      taxOfficeId: crypto.randomUUID(),
      taxNumber: String(Math.floor(1_000_000_000 + Math.random() * 8_000_000_000)),
      marketingConsent: false,
    },
  });
  expect(registerEmployerResponse.ok(), 'register employer').toBeTruthy();
  const registeredEmployer = (await registerEmployerResponse.json()) as { companyId: string };

  const approveCompanyResponse = await request.post(
    `${BACKEND_URL}/api/v1/admin/companies/${registeredEmployer.companyId}/approve`,
    { headers: authHeaders(adminToken) },
  );
  expect(approveCompanyResponse.status(), 'approve company').toBe(204);

  const company = (await (
    await request.get(`${BACKEND_URL}/api/v1/admin/companies/${registeredEmployer.companyId}`, { headers: authHeaders(adminToken) })
  ).json()) as { careerAdvisorId: string | null };
  const assignedAdvisorToken =
    company.careerAdvisorId === createdAdvisor.careerAdvisorId
      ? await loginAs(request, advisorEmail, advisorPassword)
      : await (async () => {
          throw new Error('Unexpected advisor assignment in e2e setup - only one advisor exists, assignment must match it.');
        })();

  const employerToken = await loginAs(request, employerEmail, employerPassword);
  const createJobResponse = await request.post(`${BACKEND_URL}/api/v1/employer/jobs`, {
    headers: authHeaders(employerToken),
    data: {
      title: `E2E İlan ${RUN_ID}`,
      isForDisabledCandidates: false,
      employmentTypeId: crypto.randomUUID(),
      workLocationTypeId: crypto.randomUUID(),
      positionId: crypto.randomUUID(),
      departmentId: crypto.randomUUID(),
      provinceId: crypto.randomUUID(),
      descriptionHtml: '<p>E2E test ilan açıklaması.</p>',
      experienceLevelId: crypto.randomUUID(),
      genderPreferenceIds: [],
      militaryStatusPreferenceIds: [],
      educationLevelPreferenceIds: [],
      drivingLicensePreferenceIds: [],
      languageRequirements: [],
    },
  });
  expect(createJobResponse.ok(), 'create job').toBeTruthy();
  const createdJob = (await createJobResponse.json()) as { jobId: string };

  const submitJobResponse = await request.post(`${BACKEND_URL}/api/v1/employer/jobs/${createdJob.jobId}/submit`, {
    headers: authHeaders(employerToken),
  });
  expect(submitJobResponse.status(), 'submit job').toBe(204);

  const approveJobResponse = await request.post(`${BACKEND_URL}/api/v1/career-advisor/jobs/${createdJob.jobId}/approve`, {
    headers: authHeaders(assignedAdvisorToken),
  });
  expect(approveJobResponse.status(), 'approve job').toBe(204);

  const publicJobs = (await (
    await request.get(`${BACKEND_URL}/api/v1/public/jobs?companyId=${registeredEmployer.companyId}`)
  ).json()) as { items: { id: string; slug: string }[] };
  const publishedJob = publicJobs.items.find((j) => j.id === createdJob.jobId);
  expect(publishedJob, 'published job appears in public jobs list').toBeTruthy();

  // `apps/web`'in `getSite()`'ı (60 sn `revalidate`) ne PublicJobListingsEnabled'ın ne de çerez
  // politikası anahtarının yeni değerini henüz görmemiş olabilir - yukarıdaki kurulum zaten gerçek
  // zaman harcadı, ikisinin EN GEÇ olanından itibaren yalnız kalan süre beklenir. Next'in `revalidate`
  // önbelleği ayrıca "stale-while-revalidate"tir: 60 sn geçtikten sonraki İLK istek yine eski veriyi
  // döner ve arka planda yenilemeyi yalnız TETİKLER - sabit bir ek bekleme yerine, gerçekten
  // yenilendiğini `/ilanlar`'ın kendisini (200 dönene kadar) yoklayarak doğrulanır - tahmini bir süre
  // değil, aradığımız şeyin bizzat kendisi.
  const lastSettingsMutationAtMs = Math.max(flagFlippedAtMs, cookiePolicyConfiguredAtMs);
  const remainingCacheMs = 61_000 - (Date.now() - lastSettingsMutationAtMs);
  if (remainingCacheMs > 0) {
    await new Promise((resolve) => setTimeout(resolve, remainingCacheMs));
  }
  for (let attempt = 0; attempt < 20; attempt++) {
    const probe = await request.get(`${WEB_URL}/ilanlar`);
    if (probe.status() === 200) break;
    await new Promise((resolve) => setTimeout(resolve, 1_000));
  }

  setup = {
    adminToken,
    originalCookiePolicyKey: settingsBefore.cookiePolicyKey,
    cookiePolicyDocumentId: cookiePolicy.id,
    privacyNoticeDocumentId: privacyNotice.id,
    formPagePath: `/${createdContent.fullPath}`,
    formKey,
    formDefinitionId: createdForm.id,
    formContentItemId: createdContent.id,
    validRedirectFromPath,
    invalidRedirectFromPath,
    redirectIds: [validRedirect.id, invalidRedirect.id],
    jobSlug: publishedJob!.slug,
    originalPublicJobListingsEnabled: featuresBefore.publicJobListingsEnabled,
    originalBotProtectionEnabled: botProtectionBefore.botProtectionEnabled,
  };
});

test.afterAll(async ({ request }) => {
  if (!setup) return;
  const headers = authHeaders(setup.adminToken);

  for (const id of setup.redirectIds) {
    await request.delete(`${BACKEND_URL}/api/v1/admin/website/redirects/${id}`, { headers });
  }

  // Form, içeriğe bağlıyken silinemez ("FormDefinition.InUse") - önce içerikten koparılır (taze
  // RowVersion ile). İçerik de YAYINDAYKEN çöp kutusuna taşınamaz ("ContentItem.CannotTrashPublishedContent")
  // - önce yayından kaldırılır, sonra çöp kutusuna taşınır (kalıcı silme değil - admin-rich-text.spec.ts
  // ile aynı emsal, 30 gün saklama), en son form silinir.
  async function getContentRowVersion(): Promise<string> {
    const detail = (await (
      await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${setup.formContentItemId}`, { headers })
    ).json()) as { rowVersion: string };
    return detail.rowVersion;
  }

  await request.put(`${BACKEND_URL}/api/v1/admin/website/contents/${setup.formContentItemId}/form`, {
    headers,
    data: { rowVersion: await getContentRowVersion(), formDefinitionId: null },
  });
  await request.post(`${BACKEND_URL}/api/v1/admin/website/contents/${setup.formContentItemId}/unpublish`, {
    headers,
    data: { rowVersion: await getContentRowVersion() },
  });
  await request.delete(`${BACKEND_URL}/api/v1/admin/website/contents/${setup.formContentItemId}`, {
    headers,
    data: { rowVersion: await getContentRowVersion() },
  });

  await request.delete(`${BACKEND_URL}/api/v1/admin/website/forms/${setup.formDefinitionId}`, { headers });

  // Çerez politikası/aydınlatma metni belgeleri SİLİNMEZ - yayınlanmış bir `LegalDocument` backend'de
  // kalıcıdır (yukarıdaki `getOrCreatePublishedLegalDocument` yorumu); bir sonraki koşu aynı sabit
  // anahtarla bulup yeniden kullanır.

  // Çerez politikasını orijinal duruma döndür.
  const settingsNow = (await (
    await request.get(`${BACKEND_URL}/api/v1/admin/website/settings`, { headers })
  ).json()) as {
    rowVersion: string;
    globalSearchEnabled: boolean;
    newsletterEnabled: boolean;
    donationPageEnabled: boolean;
    allowSearchEngineIndexing: boolean;
  };
  await request.put(`${BACKEND_URL}/api/v1/admin/website/settings/cookie-consent`, {
    headers,
    data: { rowVersion: settingsNow.rowVersion, cookiePolicyKey: setup.originalCookiePolicyKey, translations: [] },
  });

  // PublicJobListingsEnabled'ı orijinal değerine döndür (taze rowVersion gerekir - üstteki çağrı
  // satırı değiştirdi).
  const settingsForFeatureRestore = (await (
    await request.get(`${BACKEND_URL}/api/v1/admin/website/settings`, { headers })
  ).json()) as { rowVersion: string };
  await request.put(`${BACKEND_URL}/api/v1/admin/website/settings/features`, {
    headers,
    data: {
      rowVersion: settingsForFeatureRestore.rowVersion,
      globalSearchEnabled: settingsNow.globalSearchEnabled,
      newsletterEnabled: settingsNow.newsletterEnabled,
      publicJobListingsEnabled: setup.originalPublicJobListingsEnabled,
      donationPageEnabled: settingsNow.donationPageEnabled,
      allowSearchEngineIndexing: settingsNow.allowSearchEngineIndexing,
    },
  });

  // Bot korumasını orijinal duruma döndür (taze rowVersion gerekir).
  const settingsForBotProtectionRestore = (await (
    await request.get(`${BACKEND_URL}/api/v1/admin/website/settings`, { headers })
  ).json()) as { rowVersion: string; turnstileSiteKey: string };
  await request.put(`${BACKEND_URL}/api/v1/admin/website/settings/bot-protection`, {
    headers,
    data: {
      rowVersion: settingsForBotProtectionRestore.rowVersion,
      botProtectionEnabled: setup.originalBotProtectionEnabled,
      turnstileSiteKey: settingsForBotProtectionRestore.turnstileSiteKey,
    },
  });
});

// --- Ana sayfa blokları ---

test('ana sayfa seed-dev bloklarını render eder (hero slider başlığı görünür)', async ({ page }) => {
  await page.goto(`${WEB_URL}/`);
  await expect(page.locator('.hero-slide h2').first()).toBeVisible();
});

// --- Liste/detay (haber) ---

test('haber listesi seed-dev öğelerini listeler', async ({ page }) => {
  await page.goto(`${WEB_URL}/haberler`);
  await expect(page.getByRole('heading', { name: 'Seed Haber Bir', level: 3 })).toBeVisible();
});

test('haber detayı başlık ve gövdeyi render eder', async ({ page }) => {
  await page.goto(`${WEB_URL}/haberler/seed-dev-haber-1`);
  await expect(page.getByRole('heading', { name: 'Seed Haber Bir', level: 1 })).toBeVisible();
});

// --- Etkinlik liste/detay ---

test('etkinlik listesi seed-dev etkinliğini listeler', async ({ page }) => {
  await page.goto(`${WEB_URL}/etkinlikler`);
  await expect(page.getByText('Seed Etkinlik')).toBeVisible();
});

test('etkinlik detayı kayıt durumu ve takvim bağlantısını gösterir', async ({ page }) => {
  await page.goto(`${WEB_URL}/etkinlikler/seed-dev-etkinlik-1`);
  await expect(page.getByRole('heading', { name: 'Kayıt' })).toBeVisible();
  await expect(page.getByRole('link', { name: /takvime ekle/i })).toBeVisible();
});

// --- İlan liste/detay ---

test('ilan listesi onaylanan E2E ilanını listeler', async ({ page }) => {
  await page.goto(`${WEB_URL}/ilanlar`);
  await expect(page.getByText(`E2E İlan ${RUN_ID}`)).toBeVisible();
});

test('ilan detayı slug ile açılır', async ({ page }) => {
  await page.goto(`${WEB_URL}/ilanlar/${setup.jobSlug}`);
  await expect(page.getByRole('heading', { name: `E2E İlan ${RUN_ID}`, level: 1 })).toBeVisible();
});

// --- Form gönderimi ---

// `PublicSubmissionGuard.MinimumFillDuration` (3 sn): gönderim, submission token alındıktan EN AZ 3
// sn sonra yapılmalı, aksi halde honeypot'tan BAĞIMSIZ olarak zaten reddedilir - aşağıdaki iki testte
// de bu minimum süre kasıtlı olarak beklenir, aksi halde honeypot testi YANLIŞ GEREKÇEYLE geçebilir.
const MIN_FILL_DURATION_MS = 3_100;

test('form gönderimi: honeypot dolu olunca reddedilir', async ({ page }) => {
  await page.goto(`${WEB_URL}${setup.formPagePath}`);
  await page.locator('#website').fill('http://spam.example');
  await page.getByLabel('E-posta').fill(`e2e-honeypot-${RUN_ID}@example.com`);
  await page.waitForTimeout(MIN_FILL_DURATION_MS);

  const responsePromise = page.waitForResponse((r) => r.url().includes('/api/v1/public/forms/') && r.url().includes('/submissions'));
  await page.getByRole('button', { name: 'Gönder' }).click();
  const response = await responsePromise;

  expect(response.ok(), 'honeypot-filled submission must not succeed').toBeFalsy();
});

test('form gönderimi: normal gönderim kabul edilir', async ({ page }) => {
  await page.goto(`${WEB_URL}${setup.formPagePath}`);
  await page.getByLabel('E-posta').fill(`e2e-normal-${RUN_ID}@example.com`);
  await page.waitForTimeout(MIN_FILL_DURATION_MS);

  // Ağ yanıtını (`waitForResponse`) yakalamak yerine sonuç DOM durumuna bakılır: `FormRenderer` tek
  // sayfalık bir SPA akışıdır (gerçek bir gezinme yok), ama paylaşılan geliştirme makinesinde aynı anda
  // birden çok ağır süreç (backend + 3 Vite dev sunucusu + bu Playwright koşusu) çalışırken ağ yanıt
  // gövdesini okumaya çalışmak ara sıra CDP "navigated away" hatasına yol açtı - gerçek sonucu (başarı
  // mesajının görünmesi) doğrudan beklemek daha sağlam.
  await page.getByRole('button', { name: 'Gönder' }).click();

  await expect(page.getByText('Gönderiminiz alındı.')).toBeVisible();
  await expect(page.getByText(/Referans numarası/)).toBeVisible();
});

// --- Yasal belge sayfası ---

test('yasal belge sayfası yayınlanan aydınlatma metnini gösterir', async ({ request }) => {
  const privacyKey = 'e2e-privacynotice-fixture';
  const response = await request.get(`${WEB_URL}/legal/${privacyKey}`);
  expect(response.status()).toBe(200);
  const html = await response.text();
  expect(html).toContain('E2E PrivacyNotice');
  expect(html).toContain('E2E test aydınlatma metni gövdesi.');
});

// --- Çerez onayı akışı ---

test('çerez onayı: banner görünür, tümünü kabul et sonrası kaybolur ve çerez yazılır', async ({ page }) => {
  await page.context().clearCookies();
  await page.goto(`${WEB_URL}/`);

  const banner = page.getByRole('region', { name: /çerez/i });
  await expect(banner).toBeVisible();

  await banner.getByRole('button', { name: 'Tümünü kabul et' }).click();
  await expect(banner).not.toBeVisible();

  const cookies = await page.context().cookies();
  expect(cookies.some((c) => c.name === 'gm_consent')).toBeTruthy();

  const storageKeys = await page.evaluate(() => ({
    localStorage: Object.keys(window.localStorage),
    sessionStorage: Object.keys(window.sessionStorage),
  }));
  // K-07: tema dışında localStorage/sessionStorage'a hiçbir şey yazılmaz - çerez onayı da dahil.
  expect(storageKeys.sessionStorage).toEqual([]);
  expect(storageKeys.localStorage.every((k) => k === 'theme')).toBeTruthy();
});

test('çerez onayı: kaydedilen tercih admin özetinde görünür', async ({ request }) => {
  const summaryResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/cookie-consents/summary`, {
    headers: authHeaders(setup.adminToken),
  });
  expect(summaryResponse.ok()).toBeTruthy();
  const summary = (await summaryResponse.json()) as { totalRecords: number };
  expect(summary.totalRecords).toBeGreaterThan(0);
});

// --- 404 ---

test('bilinmeyen yol 404 döner', async ({ request }) => {
  const response = await request.get(`${WEB_URL}/olmayan-bir-e2e-sayfasi-${RUN_ID}`);
  expect(response.status()).toBe(404);
});

// --- Yönlendirme ---

test('yönlendirme: geçerli hedef 301 ile takip edilir', async ({ page }) => {
  const response = await page.goto(`${WEB_URL}/${setup.validRedirectFromPath}`);
  expect(page.url()).toBe(`${WEB_URL}/haberler`);
  expect(response?.status()).toBe(200);
});

test('yönlendirme: açık yönlendirme biçimindeki hedef 404 ile reddedilir, dış adrese gidilmez', async ({ page }) => {
  const response = await page.goto(`${WEB_URL}/${setup.invalidRedirectFromPath}`);
  expect(response?.status()).toBe(404);
  expect(page.url()).toBe(`${WEB_URL}/${setup.invalidRedirectFromPath}`);
});

// --- Canonical mutlak yol ---

test('liste sayfasının canonical bağlantısı mutlak URL\'dir', async ({ request }) => {
  const response = await request.get(`${WEB_URL}/haberler`);
  const html = await response.text();
  const match = html.match(/<link rel="canonical" href="([^"]+)"/);
  expect(match, 'canonical link must be present').toBeTruthy();
  expect(match![1]).toBe(`${WEB_URL}/haberler`);
});

// --- CSP / nonce ---

test('CSP başlığı nonce taşır ve sayfadaki satır içi script aynı nonce\'u kullanır', async ({ request }) => {
  const response = await request.get(`${WEB_URL}/`);
  const csp = response.headers()['content-security-policy'];
  expect(csp).toBeTruthy();
  const nonceMatch = csp.match(/'nonce-([^']+)'/);
  expect(nonceMatch, 'CSP must declare a script nonce').toBeTruthy();

  const html = await response.text();
  expect(html).toContain(`nonce="${nonceMatch![1]}"`);
});

// --- Prod axe taraması ---

test('üretim derlemesine karşı axe: ilan detayı erişilebilirlik ihlali üretmez', async ({ page }) => {
  await page.goto(`${WEB_URL}/ilanlar/${setup.jobSlug}`);
  const results = await new AxeBuilder({ page }).analyze();
  expect(results.violations).toEqual([]);
});
