#!/usr/bin/env node
/**
 * Faz 3 Koşu 2C (BLOK-ESLEMESI.md "Dev verisi" bölümü): yalnız yerel geliştirme için, gerçek admin
 * uçlarıyla (Node yerleşik `fetch`), ana sayfa düzenindeki 15 blok tipinin TAMAMINDAN en az bir örnek
 * üretir ki `GET /public/home` canlı örneğiyle `image-text`/`video-feature` dahil her blok tipinin
 * gerçek `data` şekli doğrulanabilsin. Uygulama paketlerinin (apps/web) DIŞINDADIR, bundler'sız düz
 * `.mjs` - TypeScript/`import type` kullanılmaz (K-05 bu betik için geçerli değil: bu betik hiçbir
 * zaman derlenen bir uygulamaya dahil olmaz, yalnız bir kerelik admin API çağrısı dizisidir).
 *
 * Kurallar (master prompt): yalnız `localhost` kaynağına karşı çalışır (aksi halde reddeder),
 * idempotent (ikinci çalıştırma aynı kayıtları yeniden kullanır, çoğaltmaz), tüm kayıtlar
 * "Seed " önekli/tanınabilir, `--remove` ile oluşturduğu her şeyi geri alır. Kimlik bilgisi
 * depoya girmez - yalnız ortam değişkeninden (`SEED_ADMIN_EMAIL`/`SEED_ADMIN_PASSWORD`, varsayılan
 * docs/DEVELOPMENT.md §44.1'deki tek-seferlik yerel Admin).
 *
 * İlan (job) KASITLI olarak seed edilmez: `job-list` bloğu backend'den veri çekmez (ADR-024 §8.1),
 * ilan bölümü dev'de boş kalırsa bu doğru davranıştır (Employer modülü bu betiğin kapsamı dışında).
 */
import { deflateSync } from 'node:zlib';

const BACKEND_ORIGIN = process.env.BACKEND_ORIGIN ?? 'http://localhost:5016';
const ADMIN_EMAIL = process.env.SEED_ADMIN_EMAIL ?? 'admin@dev.local';
const ADMIN_PASSWORD = process.env.SEED_ADMIN_PASSWORD ?? 'DevAdmin#2026';
const SEED_LABEL_PREFIX = 'Seed ';
const SEED_SLUG_PREFIX = 'seed-dev-';
const REMOVE = process.argv.includes('--remove');

function assertLocalhost(origin) {
  const url = new URL(origin);
  if (url.hostname !== 'localhost' && url.hostname !== '127.0.0.1') {
    throw new Error(`seed-dev yalnız localhost'a karşı çalışır, reddedildi: ${origin}`);
  }
}

// ---- PNG üretimi (node:zlib, yeni paket yok) ----
function crc32(buffer) {
  let crc = ~0;
  for (const byte of buffer) {
    crc ^= byte;
    for (let i = 0; i < 8; i++) {
      crc = crc & 1 ? (crc >>> 1) ^ 0xedb88320 : crc >>> 1;
    }
  }
  return ~crc >>> 0;
}

function pngChunk(type, data) {
  const typeBuffer = Buffer.from(type, 'ascii');
  const length = Buffer.alloc(4);
  length.writeUInt32BE(data.length, 0);
  const crcInput = Buffer.concat([typeBuffer, data]);
  const crc = Buffer.alloc(4);
  crc.writeUInt32BE(crc32(crcInput), 0);
  return Buffer.concat([length, typeBuffer, data, crc]);
}

/** Düz renkli, geçerli bir PNG (8-bit RGB, filtre yok) - seed slider/partner/içerik görselleri için. */
function createSolidColorPng(width, height, [r, g, b]) {
  const signature = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(width, 0);
  ihdr.writeUInt32BE(height, 4);
  ihdr[8] = 8; // bit depth
  ihdr[9] = 2; // color type: RGB
  ihdr[10] = 0;
  ihdr[11] = 0;
  ihdr[12] = 0;

  const rowBytes = 1 + width * 3;
  const raw = Buffer.alloc(rowBytes * height);
  for (let y = 0; y < height; y++) {
    const rowStart = y * rowBytes;
    raw[rowStart] = 0; // filtre: yok
    for (let x = 0; x < width; x++) {
      const pixelStart = rowStart + 1 + x * 3;
      raw[pixelStart] = r;
      raw[pixelStart + 1] = g;
      raw[pixelStart + 2] = b;
    }
  }
  const idatData = deflateSync(raw);

  return Buffer.concat([
    signature,
    pngChunk('IHDR', ihdr),
    pngChunk('IDAT', idatData),
    pngChunk('IEND', Buffer.alloc(0)),
  ]);
}

// ---- HTTP yardımcıları ----
async function api(token, method, path, body) {
  const headers = { ...(token ? { Authorization: `Bearer ${token}` } : {}) };
  let payload;
  if (body instanceof FormData) {
    payload = body;
  } else if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
    payload = JSON.stringify(body);
  }
  const response = await fetch(`${BACKEND_ORIGIN}${path}`, { method, headers, body: payload });
  if (!response.ok) {
    const text = await response.text().catch(() => '');
    throw new Error(`${method} ${path} -> ${response.status}\n${text}`);
  }
  if (response.status === 204) return undefined;
  const contentType = response.headers.get('Content-Type') ?? '';
  return contentType.includes('json') ? response.json() : undefined;
}

async function login() {
  const result = await api(null, 'POST', '/api/v1/auth/login', { email: ADMIN_EMAIL, password: ADMIN_PASSWORD });
  return result.accessToken;
}

async function uploadMedia(token, buffer, filename, altText) {
  const existing = await api(token, 'GET', `/api/v1/admin/website/media?search=${encodeURIComponent(filename)}&pageSize=5`);
  const found = existing.items.find((item) => item.originalFileName === filename);
  if (found) return found.id;

  const form = new FormData();
  form.set('file', new Blob([buffer], { type: 'image/png' }), filename);
  form.set('altText', altText);
  const uploaded = await api(token, 'POST', '/api/v1/admin/website/media', form);
  return uploaded.id;
}

async function getContentTypeIdByKey(token, key) {
  const types = await api(token, 'GET', '/api/v1/admin/website/content-types');
  const match = types.find((t) => t.key === key);
  if (!match) throw new Error(`İçerik tipi bulunamadı: ${key} (migration seed'i değişmiş olabilir)`);
  return match.id;
}

/** Slug'a göre idempotent arama; yoksa oluşturup varsayılan dil çevirisini günceller ve yayınlar. */
async function ensureContentItem(token, { contentTypeId, slug, title, summary, body, coverImageMediaId, isFeatured }) {
  // ContentItemSummaryResponse slug taşımıyor (yalnız title) - idempotent arama title ile yapılır;
  // bu betiğin ürettiği her başlık benzersizdir ("Seed ..." öneki + ayırt edici metin).
  const list = await api(token, 'GET', `/api/v1/admin/website/contents?contentTypeId=${contentTypeId}&search=${encodeURIComponent(title)}&pageSize=20`);
  const existingSummary = list.items.find((item) => item.title === title);
  if (existingSummary) return existingSummary.id;

  const created = await api(token, 'POST', '/api/v1/admin/website/contents', {
    contentTypeId,
    parentId: null,
    sortOrder: 0,
    isFeatured: Boolean(isFeatured),
    coverImageMediaId: coverImageMediaId ?? null,
    detailImageMediaId: null,
    defaultLanguageTitle: title,
    defaultLanguageSlug: slug,
    defaultLanguageSummary: summary ?? null,
    defaultLanguageBody: body ?? null,
    seo: {
      metaTitle: null,
      metaDescription: null,
      metaKeywords: null,
      ogTitle: null,
      ogDescription: null,
      ogImageMediaId: null,
      canonicalUrl: null,
      noIndex: false,
    },
  });

  const detail = await api(token, 'GET', `/api/v1/admin/website/contents/${created.id}`);
  await api(token, 'POST', `/api/v1/admin/website/contents/${created.id}/publish`, {
    rowVersion: detail.rowVersion,
    publishAtUtc: null,
    unpublishAtUtc: null,
  });
  return created.id;
}

async function ensureImpactMetric(token, { label, value, unit, period, source, iconKey, sortOrder }) {
  const list = await api(token, 'GET', `/api/v1/admin/website/impact-metrics?search=${encodeURIComponent(label)}&pageSize=20`);
  const existing = list.items.find((item) => item.label === label);
  if (existing) {
    if (!existing.isActive) {
      const detail = await api(token, 'GET', `/api/v1/admin/website/impact-metrics/${existing.id}`);
      await api(token, 'POST', `/api/v1/admin/website/impact-metrics/${existing.id}/activate`, { rowVersion: detail.rowVersion });
    }
    return existing.id;
  }

  const created = await api(token, 'POST', '/api/v1/admin/website/impact-metrics', {
    value,
    iconKey: iconKey ?? null,
    sortOrder,
    defaultLanguageLabel: label,
    defaultLanguageUnit: unit ?? null,
    defaultLanguagePeriod: period,
    defaultLanguageSource: source,
  });
  // ADR-024 §8.2: doğrulanmamış rakam yayınlanmaz - oluşturma varsayılan olarak pasiftir, Period/
  // Source zaten dolu olduğu için burada doğrudan etkinleştirilebilir.
  const detail = await api(token, 'GET', `/api/v1/admin/website/impact-metrics/${created.id}`);
  await api(token, 'POST', `/api/v1/admin/website/impact-metrics/${created.id}/activate`, { rowVersion: detail.rowVersion });
  return created.id;
}

async function ensurePartner(token, { name, logoMediaId, websiteUrl, sortOrder }) {
  const list = await api(token, 'GET', '/api/v1/admin/website/partners?pageSize=50');
  const existing = list.items.find((item) => item.name === name);
  if (existing) return existing.id;

  const created = await api(token, 'POST', '/api/v1/admin/website/partners', {
    logoMediaId,
    websiteUrl: websiteUrl ?? null,
    sortOrder,
    defaultLanguageName: name,
    defaultLanguageDescription: null,
  });
  return created.id;
}

async function ensureVideo(token, { title, youTubeUrl, sortOrder }) {
  const list = await api(token, 'GET', '/api/v1/admin/website/videos?pageSize=50');
  const existing = list.items.find((item) => item.title === title);
  if (existing) return existing.id;

  const created = await api(token, 'POST', '/api/v1/admin/website/videos', {
    youTubeUrl,
    coverImageMediaId: null,
    sortOrder,
    defaultLanguageTitle: title,
    defaultLanguageDescription: null,
  });
  return created.id;
}

async function ensureSlider(token, key, defaultLanguageName) {
  const list = await api(token, 'GET', '/api/v1/admin/website/sliders');
  const existing = list.find((item) => item.key === key);
  if (existing) return existing.id;
  const created = await api(token, 'POST', '/api/v1/admin/website/sliders', { key, defaultLanguageName });
  return created.id;
}

async function ensureMenuItem(token, location, { label, link }) {
  const menu = await api(token, 'GET', `/api/v1/admin/website/menus/${location}`);
  if (menu.items.some((item) => item.translations.some((t) => t.label === label))) return;

  const items = menu.items.map((item) => ({
    tempId: item.id,
    parentTempId: item.parentId,
    sortOrder: item.sortOrder,
    isActive: item.isActive,
    link: item.link,
    openInNewTab: item.openInNewTab,
    iconKey: item.iconKey,
    translations: item.translations,
  }));
  items.push({
    tempId: crypto.randomUUID(),
    parentTempId: null,
    sortOrder: items.length,
    isActive: true,
    link,
    openInNewTab: false,
    iconKey: null,
    translations: [{ languageCode: 'tr', label }],
  });
  await api(token, 'PUT', `/api/v1/admin/website/menus/${location}`, { rowVersion: menu.rowVersion, items });
}

async function removeSeedMenuItems(token, location) {
  const menu = await api(token, 'GET', `/api/v1/admin/website/menus/${location}`);
  const kept = menu.items.filter((item) => !item.translations.some((t) => t.label.startsWith(SEED_LABEL_PREFIX)));
  if (kept.length === menu.items.length) return;
  const items = kept.map((item) => ({
    tempId: item.id,
    parentTempId: item.parentId,
    sortOrder: item.sortOrder,
    isActive: item.isActive,
    link: item.link,
    openInNewTab: item.openInNewTab,
    iconKey: item.iconKey,
    translations: item.translations,
  }));
  await api(token, 'PUT', `/api/v1/admin/website/menus/${location}`, { rowVersion: menu.rowVersion, items });
}

async function main() {
  assertLocalhost(BACKEND_ORIGIN);
  const token = await login();

  if (REMOVE) {
    await removeSeedData(token);
    console.log('seed-dev: oluşturulan kayıtlar geri alındı.');
    return;
  }

  console.log('seed-dev: medya yükleniyor...');
  const heroImage1 = await uploadMedia(token, createSolidColorPng(1600, 900, [7, 31, 50]), 'seed-dev-hero-1.png', 'Seed hero görseli 1');
  const heroImage2 = await uploadMedia(token, createSolidColorPng(1600, 900, [196, 67, 37]), 'seed-dev-hero-2.png', 'Seed hero görseli 2');
  const introImage = await uploadMedia(token, createSolidColorPng(1200, 800, [10, 104, 107]), 'seed-dev-intro.png', 'Seed tanıtım görseli');
  const partnerLogo = await uploadMedia(token, createSolidColorPng(400, 200, [217, 74, 43]), 'seed-dev-partner-logo.png', 'Seed firma logosu');
  const galleryImage = await uploadMedia(token, createSolidColorPng(800, 600, [16, 42, 46]), 'seed-dev-gallery.png', 'Seed galeri görseli');

  console.log('seed-dev: slider...');
  const sliderId = await ensureSlider(token, 'seed-dev-hero', 'Seed Hero Slider');
  const sliderDetail = await api(token, 'GET', `/api/v1/admin/website/sliders/${sliderId}`);
  await api(token, 'PUT', `/api/v1/admin/website/sliders/${sliderId}/slides`, {
    rowVersion: sliderDetail.rowVersion,
    slides: [
      {
        desktopImageMediaId: heroImage1,
        mobileImageMediaId: null,
        link: { kind: 'InternalPath', contentItemId: null, contentTypeId: null, internalPath: '/hakkimizda', externalUrl: null },
        sortOrder: 0,
        isActive: true,
        publishAtUtc: null,
        unpublishAtUtc: null,
        translations: [{ languageCode: 'tr', eyebrow: 'Dernek', title: 'Seed Slayt 1', text: 'Örnek slayt metni.', buttonLabel: 'Keşfet', altTextOverride: null }],
      },
      {
        desktopImageMediaId: heroImage2,
        mobileImageMediaId: null,
        link: { kind: 'None', contentItemId: null, contentTypeId: null, internalPath: null, externalUrl: null },
        sortOrder: 1,
        isActive: true,
        publishAtUtc: null,
        unpublishAtUtc: null,
        translations: [{ languageCode: 'tr', eyebrow: null, title: 'Seed Slayt 2', text: null, buttonLabel: null, altTextOverride: null }],
      },
    ],
  });

  console.log('seed-dev: rakamlar şeridi...');
  const metricIds = [
    await ensureImpactMetric(token, { label: 'Seed Mezun Sayısı', value: 1240, unit: 'kişi', period: '2020-2026', source: 'Dernek kayıtları', iconKey: 'users', sortOrder: 0 }),
    await ensureImpactMetric(token, { label: 'Seed Gönüllü Sayısı', value: 340, unit: 'kişi', period: '2026', source: 'Dernek kayıtları', iconKey: 'heart', sortOrder: 1 }),
    await ensureImpactMetric(token, { label: 'Seed Etkinlik Sayısı', value: 58, unit: 'etkinlik', period: '2026', source: 'Dernek kayıtları', iconKey: 'calendar', sortOrder: 2 }),
  ];

  console.log('seed-dev: firma bandı...');
  const partnerId = await ensurePartner(token, { name: 'Seed Örnek Firma', logoMediaId: partnerLogo, websiteUrl: 'https://example.org', sortOrder: 0 });

  console.log('seed-dev: video...');
  await ensureVideo(token, { title: 'Seed Tanıtım Videosu', youTubeUrl: 'https://www.youtube.com/watch?v=dQw4w9WgXcQ', sortOrder: 0 });

  console.log('seed-dev: içerikler...');
  const newsTypeId = await getContentTypeIdByKey(token, 'news');
  const announcementTypeId = await getContentTypeIdByKey(token, 'announcement');
  const activityTypeId = await getContentTypeIdByKey(token, 'activity');
  const successStoryTypeId = await getContentTypeIdByKey(token, 'success-story');
  const teamTypeId = await getContentTypeIdByKey(token, 'team');
  const eventTypeId = await getContentTypeIdByKey(token, 'event');
  const faqTypeId = await getContentTypeIdByKey(token, 'faq');

  await ensureContentItem(token, {
    contentTypeId: newsTypeId,
    slug: `${SEED_SLUG_PREFIX}haber-1`,
    title: 'Seed Haber Bir',
    summary: 'Örnek haber özeti.',
    body: '<p>Örnek haber gövdesi.</p>',
    coverImageMediaId: galleryImage,
  });
  await ensureContentItem(token, {
    contentTypeId: newsTypeId,
    slug: `${SEED_SLUG_PREFIX}haber-2`,
    title: 'Seed Haber İki',
    summary: 'İkinci örnek haber özeti.',
    body: '<p>İkinci örnek haber gövdesi.</p>',
    coverImageMediaId: galleryImage,
  });
  await ensureContentItem(token, {
    contentTypeId: announcementTypeId,
    slug: `${SEED_SLUG_PREFIX}duyuru-1`,
    title: 'Seed Duyuru',
    summary: 'Örnek duyuru özeti.',
    body: '<p>Örnek duyuru gövdesi.</p>',
  });
  await ensureContentItem(token, {
    contentTypeId: activityTypeId,
    slug: `${SEED_SLUG_PREFIX}faaliyet-1`,
    title: 'Seed Faaliyet',
    summary: 'Örnek faaliyet özeti.',
    body: '<p>Örnek faaliyet gövdesi.</p>',
  });
  await ensureContentItem(token, {
    contentTypeId: successStoryTypeId,
    slug: `${SEED_SLUG_PREFIX}basari-1`,
    title: 'Seed Başarı Hikayesi',
    summary: 'Örnek başarı hikayesi özeti (madde 3 kararı: videosuz).',
    body: '<p>Örnek başarı hikayesi gövdesi.</p>',
    coverImageMediaId: introImage,
    isFeatured: true,
  });
  await ensureContentItem(token, {
    contentTypeId: teamTypeId,
    slug: `${SEED_SLUG_PREFIX}ekip-1`,
    title: 'Seed Ekip Üyesi',
    summary: 'Genel Müdür',
    body: null,
    coverImageMediaId: galleryImage,
  });

  await ensureContentItem(token, {
    contentTypeId: faqTypeId,
    slug: `${SEED_SLUG_PREFIX}sss-1`,
    title: 'Seed Üyelik nasıl başlar?',
    summary: 'Üyelik süreciyle ilgili örnek soru-cevap.',
    body: '<p>Örnek cevap metni.</p>',
  });

  console.log('seed-dev: etkinlik + takvim...');
  const eventContentId = await ensureContentItem(token, {
    contentTypeId: eventTypeId,
    slug: `${SEED_SLUG_PREFIX}etkinlik-1`,
    title: 'Seed Etkinlik',
    summary: 'Örnek etkinlik özeti.',
    body: '<p>Örnek etkinlik gövdesi.</p>',
    coverImageMediaId: introImage,
  });
  const startsAt = new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString();
  const endsAt = new Date(Date.now() + 14 * 24 * 60 * 60 * 1000 + 3 * 60 * 60 * 1000).toISOString();
  await api(token, 'PUT', `/api/v1/admin/website/contents/${eventContentId}/event`, {
    rowVersion: null,
    startsAtUtc: startsAt,
    endsAtUtc: endsAt,
    format: 'InPerson',
    onlineLink: null,
    capacity: 40,
    registrationEnabled: true,
    registrationOpensAtUtc: null,
    registrationClosesAtUtc: null,
    minAge: null,
    maxAge: null,
    autoConfirm: true,
    waitlistEnabled: true,
    translations: [
      {
        languageCode: 'tr',
        venueName: 'Seed Merkez Salonu',
        venueAddress: 'Test Mah. No:1, Ankara',
        feeInfo: 'Ücretsiz',
        instructors: null,
        programFlow: null,
      },
    ],
  }).catch(async (error) => {
    // Zaten bir EventSchedule varsa (idempotent ikinci çalıştırma) rowVersion gerekir.
    if (!String(error).includes('409') && !String(error).includes('400')) throw error;
    const current = await api(token, 'GET', `/api/v1/admin/website/contents/${eventContentId}/event`);
    await api(token, 'PUT', `/api/v1/admin/website/contents/${eventContentId}/event`, {
      rowVersion: current.rowVersion,
      startsAtUtc: startsAt,
      endsAtUtc: endsAt,
      format: 'InPerson',
      onlineLink: null,
      capacity: 40,
      registrationEnabled: true,
      registrationOpensAtUtc: null,
      registrationClosesAtUtc: null,
      minAge: null,
      maxAge: null,
      autoConfirm: true,
      waitlistEnabled: true,
      translations: [
        { languageCode: 'tr', venueName: 'Seed Merkez Salonu', venueAddress: 'Test Mah. No:1, Ankara', feeInfo: 'Ücretsiz', instructors: null, programFlow: null },
      ],
    });
  });

  console.log('seed-dev: menüler...');
  await ensureMenuItem(token, 'Header', {
    label: 'Seed Kurumsal',
    link: { kind: 'InternalPath', contentItemId: null, contentTypeId: null, internalPath: '/hakkimizda', externalUrl: null },
  });
  await ensureMenuItem(token, 'Footer', { label: 'Seed Kurumsal', link: null });

  console.log('seed-dev: ana sayfa düzeni (15 blok)...');
  const homeLayout = await api(token, 'GET', '/api/v1/admin/website/layouts/home');
  const nonSeedBlocks = homeLayout.draftBlocks.filter((block) => !isSeedBlock(block));
  const seedBlocks = [
    { blockTypeKey: 'hero-slider', isActive: true, settings: { sliderId }, texts: null },
    {
      blockTypeKey: 'image-text',
      isActive: true,
      settings: { imageMediaId: introImage, imagePosition: 'left', link: { kind: 'None', contentItemId: null, contentTypeId: null, internalPath: null, externalUrl: null } },
      texts: { tr: { title: 'Seed Tanıtım', body: '<p>Seed tanıtım gövdesi.</p>', linkLabel: null } },
    },
    { blockTypeKey: 'impact-stats', isActive: true, settings: { metricIds }, texts: { tr: { title: 'Seed Rakamlarla Biz' } } },
    { blockTypeKey: 'job-list', isActive: true, settings: { count: 6 }, texts: { tr: { title: 'Seed Öne Çıkan İlanlar', moreLabel: 'Tümünü gör' } } },
    {
      blockTypeKey: 'process-steps',
      isActive: true,
      settings: { stepCount: 3 },
      texts: { tr: { title: 'Seed Aday', steps: [{ title: '1. Kayıt', text: 'Profilinizi oluşturun.' }, { title: '2. Eşleşme', text: 'Size uygun ilanları görün.' }, { title: '3. Görüşme', text: 'Firmalarla görüşün.' }] } },
    },
    {
      blockTypeKey: 'process-steps',
      isActive: true,
      settings: { stepCount: 2 },
      texts: { tr: { title: 'Seed Firma', steps: [{ title: '1. İlan ver', text: 'İlanınızı yayınlayın.' }, { title: '2. Aday bulun', text: 'Uygun adaylarla buluşun.' }] } },
    },
    { blockTypeKey: 'video-feature', isActive: true, settings: { videoId: await lookupVideoId(token, 'Seed Tanıtım Videosu') }, texts: { tr: { eyebrow: 'Video', title: 'Seed Video Tanıtım', text: null, link: null, linkLabel: null } } },
    { blockTypeKey: 'upcoming-events', isActive: true, settings: { contentTypeKeys: ['event'], count: 6 }, texts: { tr: { title: 'Seed Etkinlikler', moreLabel: 'Tümünü gör' } } },
    { blockTypeKey: 'content-list', isActive: true, settings: { contentTypeKey: 'success-story', count: 6, featuredOnly: false, categoryId: null, view: 'cards' }, texts: { tr: { title: 'Seed Başarı Hikayeleri', moreLabel: null } } },
    { blockTypeKey: 'logo-strip', isActive: true, settings: { maxItems: 10 }, texts: { tr: { title: 'Seed Firmalar' } } },
    { blockTypeKey: 'content-list', isActive: true, settings: { contentTypeKey: 'news', count: 3, featuredOnly: false, categoryId: null, view: 'list' }, texts: { tr: { title: 'Seed Haberler', moreLabel: null } } },
    { blockTypeKey: 'quick-links', isActive: true, settings: { items: [{ iconKey: 'briefcase', link: { kind: 'InternalPath', contentItemId: null, contentTypeId: null, internalPath: '/ilanlar', externalUrl: null } }] }, texts: { tr: { items: [{ label: 'Seed İlanlar', description: null }] } } },
    { blockTypeKey: 'gallery', isActive: true, settings: { mediaIds: [galleryImage] }, texts: { tr: { title: 'Seed Galeri' } } },
    { blockTypeKey: 'faq', isActive: true, settings: { contentTypeKey: 'faq', categoryId: null, count: 5 }, texts: { tr: { title: 'Seed SSS' } } },
    { blockTypeKey: 'cta', isActive: true, settings: { buttons: [{ link: { kind: 'InternalPath', contentItemId: null, contentTypeId: null, internalPath: '/uye-ol', externalUrl: null }, style: 'primary' }] }, texts: { tr: { eyebrow: null, title: 'Seed Harekete Geç', buttons: [{ label: 'Üye Ol' }] } } },
    { blockTypeKey: 'rich-text', isActive: true, settings: {}, texts: { tr: { body: '<p>Seed düz metin bloğu.</p>' } } },
  ].map((block, index) => ({ ...block, sortOrder: nonSeedBlocks.length + index }));

  await api(token, 'PUT', '/api/v1/admin/website/layouts/home/draft', {
    rowVersion: homeLayout.rowVersion,
    blocks: [...nonSeedBlocks, ...seedBlocks],
  });
  const afterDraft = await api(token, 'GET', '/api/v1/admin/website/layouts/home');
  await api(token, 'POST', '/api/v1/admin/website/layouts/home/publish', { rowVersion: afterDraft.rowVersion });

  void partnerId;
  console.log('seed-dev: tamam. GET /api/v1/public/home artık 15 blok tipinin tamamından örnek içeriyor.');
}

function isSeedBlock(block) {
  const settingsText = JSON.stringify(block.settings ?? {});
  return settingsText.includes('seed-dev') || SEED_BLOCK_TYPE_KEYS.has(block.blockTypeKey);
}

// content-list/process-steps gibi birden fazla amaçla kullanılabilen tipler settings içinde seed
// olduğunu doğrudan taşımıyor (backend bilinmeyen alan kabul etmiyor) - bu yüzden bu betiğin ürettiği
// TÜM blok tipleri burada listelenir ve her çalıştırmada tamamen yeniden inşa edilir (idempotent:
// iki çalıştırma aynı listeyi üretir, çoğaltmaz). Elle eklenmiş aynı tipte bir blok varsa bu betik
// onu da kapsar - bu betik yalnız dev ortamında, bilerek kullanılır.
const SEED_BLOCK_TYPE_KEYS = new Set([
  'hero-slider', 'image-text', 'impact-stats', 'job-list', 'process-steps', 'video-feature',
  'upcoming-events', 'content-list', 'logo-strip', 'quick-links', 'gallery', 'faq', 'cta', 'rich-text',
]);

async function lookupVideoId(token, title) {
  const list = await api(token, 'GET', '/api/v1/admin/website/videos?pageSize=50');
  const match = list.items.find((item) => item.title === title);
  if (!match) throw new Error(`Video bulunamadı: ${title}`);
  return match.id;
}

async function removeSeedData(token) {
  console.log('seed-dev --remove: ana sayfa düzeni geri alınıyor...');
  const homeLayout = await api(token, 'GET', '/api/v1/admin/website/layouts/home');
  const kept = homeLayout.draftBlocks.filter((block) => !isSeedBlock(block));
  await api(token, 'PUT', '/api/v1/admin/website/layouts/home/draft', { rowVersion: homeLayout.rowVersion, blocks: kept });
  const afterDraft = await api(token, 'GET', '/api/v1/admin/website/layouts/home');
  await api(token, 'POST', '/api/v1/admin/website/layouts/home/publish', { rowVersion: afterDraft.rowVersion });

  console.log('seed-dev --remove: menüler geri alınıyor...');
  await removeSeedMenuItems(token, 'Header');
  await removeSeedMenuItems(token, 'Footer');

  console.log('seed-dev --remove: içerikler çöp kutusuna taşınıyor...');
  const typesToClean = ['news', 'announcement', 'activity', 'success-story', 'team', 'event', 'faq'];
  for (const typeKey of typesToClean) {
    const typeId = await getContentTypeIdByKey(token, typeKey).catch(() => null);
    if (!typeId) continue;
    const list = await api(token, 'GET', `/api/v1/admin/website/contents?contentTypeId=${typeId}&search=${encodeURIComponent(SEED_LABEL_PREFIX.trim())}&pageSize=50`);
    for (const item of list.items.filter((i) => i.title.startsWith(SEED_LABEL_PREFIX))) {
      const detail = await api(token, 'GET', `/api/v1/admin/website/contents/${item.id}`);
      if (detail.status === 'Published') {
        await api(token, 'POST', `/api/v1/admin/website/contents/${item.id}/unpublish`, { rowVersion: detail.rowVersion });
      }
      const afterUnpublish = await api(token, 'GET', `/api/v1/admin/website/contents/${item.id}`);
      await api(token, 'DELETE', `/api/v1/admin/website/contents/${item.id}`, { rowVersion: afterUnpublish.rowVersion });
      // Çöp kutusu (30 gün) yerine kalıcı silme - bu betik dev'de iz bırakmamalı, otomatik
      // temizlik job'ını beklemez; medya kullanım kontrolü de bu yüzden ANCAK burada serbest kalır.
      await api(token, 'DELETE', `/api/v1/admin/website/trash/${item.id}`).catch(() => {});
    }
  }

  // Yukarıdaki arama yalnız AKTİF içerikleri bulur - bir önceki --remove çalışması kalıcı silmeyi
  // tamamlayamadan kesilirse (ör. hata) öğeler çöp kutusunda kalır ve bir sonraki normal çalıştırma
  // onları bir daha hiç görmez (slug/FullPath çakışmasıyla yeniden oluşturmayı da engeller) - bu
  // yüzden çöp kutusu da ayrıca "Seed " önekine göre taranıp kalıcı silinir.
  const trash = await api(token, 'GET', '/api/v1/admin/website/trash?pageSize=200');
  for (const item of trash.items.filter((i) => i.title.startsWith(SEED_LABEL_PREFIX))) {
    await api(token, 'DELETE', `/api/v1/admin/website/trash/${item.id}`).catch(() => {});
  }

  console.log('seed-dev --remove: slider, rakamlar, firma, video siliniyor...');
  const sliders = await api(token, 'GET', '/api/v1/admin/website/sliders');
  const seedSlider = sliders.find((s) => s.key === 'seed-dev-hero');
  if (seedSlider) await api(token, 'DELETE', `/api/v1/admin/website/sliders/${seedSlider.id}`).catch(() => {});

  const metrics = await api(token, 'GET', '/api/v1/admin/website/impact-metrics?pageSize=100');
  for (const metric of metrics.items.filter((m) => m.label.startsWith(SEED_LABEL_PREFIX))) {
    await api(token, 'DELETE', `/api/v1/admin/website/impact-metrics/${metric.id}`).catch(() => {});
  }

  const partners = await api(token, 'GET', '/api/v1/admin/website/partners?pageSize=100');
  for (const partner of partners.items.filter((p) => p.name.startsWith(SEED_LABEL_PREFIX))) {
    await api(token, 'DELETE', `/api/v1/admin/website/partners/${partner.id}`).catch(() => {});
  }

  const videos = await api(token, 'GET', '/api/v1/admin/website/videos?pageSize=100');
  for (const video of videos.items.filter((v) => v.title.startsWith(SEED_LABEL_PREFIX))) {
    await api(token, 'DELETE', `/api/v1/admin/website/videos/${video.id}`).catch(() => {});
  }

  console.log('seed-dev --remove: medya siliniyor...');
  const media = await api(token, 'GET', '/api/v1/admin/website/media?search=seed-dev&pageSize=100');
  for (const asset of media.items.filter((m) => m.originalFileName?.startsWith('seed-dev-'))) {
    await api(token, 'DELETE', `/api/v1/admin/website/media/${asset.id}`).catch(() => {});
  }
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
