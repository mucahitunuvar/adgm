/**
 * Faz 3 Koşu 1 madde 6: medya host'u (CSP `img-src`, ileride `next/image` `remotePatterns`) tek bu
 * değişkenden türetilir. Backend'in döndürdüğü `Small/Medium/Large/Original` URL'leri zaten tam
 * (absolute) string olduğu için apps/web bu değeri onları İNŞA ETMEK için kullanmaz - yalnız CSP'nin
 * hangi host'a izin vereceğini bilmesi için. `proxy.ts` (Edge/Node middleware) kendi
 * `process.env` okumasını yapar; bu dosya yalnız Sunucu Bileşenleri için.
 */
export const mediaOrigin = process.env.NEXT_PUBLIC_MEDIA_ORIGIN ?? process.env.BACKEND_ORIGIN ?? 'http://localhost:5016';
