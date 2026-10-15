#!/usr/bin/env node
/**
 * packages/design-tokens/tokens.css içindeki renk çiftlerini FRONTEND-TASARIM-SISTEMI.md
 * §2 kontrast tablosundaki eşiklere karşı doğrular (WCAG 2.x oranı).
 * Eşiğin altında kalan çift bulunursa işlem 1 ile çıkar.
 */
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const tokensPath = path.join(__dirname, '..', 'packages', 'design-tokens', 'tokens.css');
const css = readFileSync(tokensPath, 'utf8');

function extractBlock(css, selectorRegex) {
  const match = css.match(selectorRegex);
  if (!match) throw new Error(`Blok bulunamadı: ${selectorRegex}`);
  const start = match.index + match[0].length;
  let depth = 1;
  let i = start;
  while (depth > 0 && i < css.length) {
    if (css[i] === '{') depth++;
    if (css[i] === '}') depth--;
    i++;
  }
  return css.slice(start, i - 1);
}

function parseVars(block) {
  const vars = {};
  const re = /--([a-z0-9-]+):\s*([^;]+);/gi;
  let m;
  while ((m = re.exec(block))) {
    vars[m[1]] = m[2].trim();
  }
  return vars;
}

const lightBlock = extractBlock(css, /:root,\s*\n?\.theme-light\s*\{/);
const darkBlock = extractBlock(css, /\n\.theme-dark\s*\{/);
const lightVars = parseVars(lightBlock);
const darkVars = parseVars(darkBlock);

function resolve(vars, token) {
  if (token.startsWith('#')) return token;
  const value = vars[token];
  if (!value) throw new Error(`Token bulunamadı: --${token}`);
  return value;
}

function hexToRgb(hex) {
  const clean = hex.replace('#', '');
  const full = clean.length === 3 ? clean.split('').map((c) => c + c).join('') : clean;
  const num = parseInt(full, 16);
  return { r: (num >> 16) & 255, g: (num >> 8) & 255, b: num & 255 };
}

function srgbToLinear(c) {
  const v = c / 255;
  return v <= 0.03928 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4);
}

function relativeLuminance({ r, g, b }) {
  return 0.2126 * srgbToLinear(r) + 0.7152 * srgbToLinear(g) + 0.0722 * srgbToLinear(b);
}

function contrastRatio(hex1, hex2) {
  const l1 = relativeLuminance(hexToRgb(hex1));
  const l2 = relativeLuminance(hexToRgb(hex2));
  const lighter = Math.max(l1, l2);
  const darker = Math.min(l1, l2);
  return (lighter + 0.05) / (darker + 0.05);
}

// FRONTEND-TASARIM-SISTEMI.md §2 kontrast tablosu (yalnız "✔" ile işaretli, kullanılan çiftler)
const checks = [
  { theme: 'light', fg: '#ffffff', bg: 'color-accent-fill', min: 4.5, label: 'White / Ember (buton metni)' },
  { theme: 'light', fg: '#ffffff', bg: 'color-accent-fill-hover', min: 4.5, label: 'White / Ember-strong (hover)' },
  { theme: 'light', fg: 'color-text', bg: 'color-bg', min: 4.5, label: 'Navy / White (gövde metni)' },
  { theme: 'light', fg: 'color-text', bg: 'color-bg-secondary', min: 4.5, label: 'Navy / Mist' },
  { theme: 'light', fg: 'color-text-secondary', bg: 'color-bg', min: 4.5, label: 'text-secondary / White' },
  { theme: 'light', fg: 'color-link', bg: 'color-bg', min: 4.5, label: 'Teal / White (bağlantı)' },
  { theme: 'light', fg: '#ffffff', bg: 'color-link', min: 4.5, label: 'White / Teal (dolgu metni)' },
  { theme: 'light', fg: 'color-control-border', bg: 'color-bg', min: 3, label: 'control-border / White (form sınırı)' },
  { theme: 'light', fg: 'color-success', bg: 'color-bg', min: 4.5, label: 'success (açık) / White' },
  { theme: 'light', fg: 'color-warning', bg: 'color-bg', min: 4.5, label: 'warning (açık) / White' },
  { theme: 'light', fg: 'color-danger', bg: 'color-bg', min: 4.5, label: 'danger (açık) / White' },
  { theme: 'light', fg: 'color-info', bg: 'color-bg', min: 4.5, label: 'info (açık) / White' },

  { theme: 'dark', fg: 'color-text', bg: 'color-bg', min: 4.5, label: 'White / Navy (koyu tema metni)' },
  { theme: 'dark', fg: 'color-text', bg: 'color-surface', min: 4.5, label: 'White / Moss' },
  { theme: 'dark', fg: 'color-text-secondary', bg: 'color-bg', min: 4.5, label: 'text-secondary (koyu) / Navy' },
  { theme: 'dark', fg: 'color-link', bg: 'color-bg', min: 4.5, label: 'teal-on-dark / Navy' },
  { theme: 'dark', fg: 'color-accent-text', bg: 'color-bg', min: 4.5, label: 'ember-on-dark / Navy' },
  { theme: 'dark', fg: 'color-control-border', bg: 'color-bg', min: 3, label: 'control-border (koyu) / Navy' },
  { theme: 'dark', fg: 'color-success', bg: 'color-bg', min: 4.5, label: 'success (koyu) / Navy' },
  { theme: 'dark', fg: 'color-warning', bg: 'color-bg', min: 4.5, label: 'warning (koyu) / Navy' },
  { theme: 'dark', fg: 'color-danger', bg: 'color-bg', min: 4.5, label: 'danger (koyu) / Navy' },
  { theme: 'dark', fg: 'color-info', bg: 'color-bg', min: 4.5, label: 'info (koyu) / Navy' },
];

let failed = false;
for (const check of checks) {
  const vars = check.theme === 'light' ? lightVars : darkVars;
  const fgHex = resolve(vars, check.fg.startsWith('#') ? check.fg : check.fg);
  const bgHex = resolve(vars, check.bg.startsWith('#') ? check.bg : check.bg);
  const ratio = contrastRatio(fgHex, bgHex);
  const ok = ratio >= check.min;
  if (!ok) failed = true;
  const status = ok ? 'OK ' : 'FAIL';
  console.log(`[${status}] (${check.theme}) ${check.label}: ${ratio.toFixed(2)} (min ${check.min})`);
}

if (failed) {
  console.error('\nKontrast kontrolü başarısız: bazı çiftler WCAG eşiğinin altında.');
  process.exit(1);
}
console.log('\nTüm kontrast çiftleri eşiği karşılıyor.');
