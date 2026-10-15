# docs/frontend: İçindekiler ve kullanım

Tarih: 14 Ekim 2026. Bu klasör, Gençlik Merkezi frontend'inin tek doküman kaynağıdır. Backend dokümanları (`AGENTS.md`, `docs/SECURITY.md`, `docs/DECISIONS/ADR-*.md`) aynen geçerlidir; frontend onlara bağlıdır.

## Dosyalar ve okuma sırası

| # | Dosya | Ne işe yarar |
|---|---|---|
| 1 | `FRONTEND-KARARLAR.md` | Bağlayıcı kararlar (ADR biçiminde). Çelişki olursa bu dosya kazanır. |
| 2 | `FRONTEND-AGENTS.md` | Claude Code için çalışma kuralları. `frontend/AGENTS.md` olarak kopyalanır. |
| 3 | `FRONTEND-MIMARI.md` | Klasör yapısı, uygulamalar, yol/host, veri akışı, dağıtım. |
| 4 | `FRONTEND-GUVENLIK.md` | Frontend güvenlik kuralları (SECURITY.md'nin frontend karşılığı). |
| 5 | `FRONTEND-TASARIM-SISTEMI.md` | Renk, tipografi, köşe, bileşenler, kontrast tablosu. |
| 6 | `FRONTEND-API-SOZLESMESI.md` | Gerçek backend uçları ve frontend'in uyacağı kurallar. |
| 7 | `DEMO-TASIMA-PLANI.md` | Demodan (genclikmerkeziweb) ne taşınır, ne yazılır, ne silinir. |
| 8 | `FRONTEND-FAZ-PLANI.md` | Fazlar, kabul ölçütleri, sıra. |
| 9 | `FRONTEND-FAZ0-MASTER-PROMPT.md` | Claude Code'a verilecek ilk görev (Faz 0). |
| 10 | `FRONTEND-ACIK-MADDELER.md` | Henüz kapanmamış kararlar ve doğrulanacak backend maddeleri. |
| 11 | `FRONTEND-OPENAPI-PRODUCES-PROMPT.md` | **Backend** görevi: OpenAPI yanıt şemalarını tamamlar (Faz 1'den önce Görev 1). `docs/prompts/` altına da konur. |
| 12 | `FRONTEND-FAZ1-MASTER-PROMPT.md` | Faz 1: kimlik + yönetim kabuğu. |

## Kurulum (bir kez)
1. Bu dosyaları `docs/frontend/` altına koyun.
2. `FRONTEND-AGENTS.md` dosyasını `frontend/AGENTS.md` olarak kopyalayın (klasörü Faz 0 oluşturacak; siz de elle açabilirsiniz).
3. Kök `CLAUDE.md` dosyasının sonuna şu satırı ekleyin:

```text
Frontend (frontend/ klasörü) üzerinde çalışırken önce docs/frontend/README.md ve docs/frontend/FRONTEND-AGENTS.md dosyalarını oku. Backend kuralları (bu dosya, AGENTS.md, SECURITY.md) frontend için de geçerlidir.
```

## Claude Code'u başlatma
Backend'i yerelde çalıştırın (`http://localhost:5016`). VS Code'da Claude Code'a yazın:

```text
docs/frontend/README.md dosyasını oku, sonra FRONTEND-KARARLAR.md, FRONTEND-AGENTS.md, FRONTEND-MIMARI.md ve FRONTEND-GUVENLIK.md dosyalarını oku. Ardından docs/frontend/FRONTEND-FAZ0-MASTER-PROMPT.md dosyasını uygula. Faz sonunda dur ve raporla.
```

## Beklenti notu (dürüst durum)
Demo (`genclikmerkeziweb/frontend`) yaklaşık 1.350 satırlık sayfa kodu olan, mock veriyle çalışan bir iskelettir. Panel sayfalarının çoğu 15-50 satırlık genel şablondur. Demodan gerçekten alınan şeyler: kabuk (header, menü, layout'lar), UI bileşenleri, CSS yapısı, rota fikirleri, testler. Panel ekranlarının büyük kısmı gerçek backend uçlarına göre **yeniden yazılacaktır**. `DEMO-TASIMA-PLANI.md` bunu dosya dosya gösterir.
