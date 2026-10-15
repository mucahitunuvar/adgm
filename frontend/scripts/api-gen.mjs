#!/usr/bin/env node
/**
 * 1) docs/api/enums.v1.json'dan enum kod listesini üretir (packages/api-client/src/enums.generated.ts).
 *    Ağ bağlantısı gerekmez; backend ayakta olmasa da çalışır.
 * 2) Backend OpenAPI şemasını indirir (packages/api-client/openapi.json) ve ondan
 *    TypeScript tiplerini üretir (packages/api-client/src/schema.d.ts). Backend ayakta olmalı.
 * K-05: Tipler yalnız bu betikle üretilir; elle API tipi yazılmaz.
 */
import { readFile } from 'node:fs/promises';
import { writeFileSync } from 'node:fs';
import { fileURLToPath, pathToFileURL } from 'node:url';
import path from 'node:path';
import { createRequire } from 'node:module';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.join(__dirname, '..');
const apiClientDir = path.join(root, 'packages', 'api-client');
const openapiPath = path.join(apiClientDir, 'openapi.json');
const schemaPath = path.join(apiClientDir, 'src', 'schema.d.ts');
const enumsManifestPath = path.join(root, '..', 'docs', 'api', 'enums.v1.json');
const enumsOutputPath = path.join(apiClientDir, 'src', 'enums.generated.ts');
const backendOrigin = process.env.BACKEND_ORIGIN ?? 'http://localhost:5016';

async function generateEnums() {
  const manifest = JSON.parse(await readFile(enumsManifestPath, 'utf8'));
  const owningModule = new Map();
  const enumBlocks = [];
  const codeEntries = [];

  for (const [moduleName, moduleEnums] of Object.entries(manifest)) {
    for (const [enumName, members] of Object.entries(moduleEnums)) {
      if (owningModule.has(enumName)) {
        throw new Error(
          `Enum adı çakışması: "${enumName}" hem "${owningModule.get(enumName)}" hem "${moduleName}" modülünde var. ` +
            'Bu üretici (scripts/api-gen.mjs) ad çakışmasını otomatik çözmez; elle ad alanı ekleyin.',
        );
      }
      owningModule.set(enumName, moduleName);

      const memberNames = Object.keys(members);
      const selfMapBody = memberNames.map((name) => `  ${name}: ${JSON.stringify(name)},`).join('\n');
      enumBlocks.push(
        `/** Kaynak: ${moduleName}.${enumName} (docs/api/enums.v1.json). */\n` +
          `export const ${enumName} = {\n${selfMapBody}\n} as const;\n` +
          `export type ${enumName} = (typeof ${enumName})[keyof typeof ${enumName}];`,
      );

      const codesBody = memberNames.map((name) => `    ${name}: ${members[name]},`).join('\n');
      codeEntries.push(`  ${enumName}: {\n${codesBody}\n  },`);
    }
  }

  const header =
    '/**\n' +
    ' * ÜRETİLDİ: elle düzenlemeyin.\n' +
    ' * Kaynak: docs/api/enums.v1.json (`pnpm api:gen` ile üretilir).\n' +
    ' * Enum kod listesinin TEK kaynağı budur; başka yerde enum üye adları elle yazılmaz.\n' +
    ' */\n';
  const registry =
    `export const ENUM_NUMERIC_CODES = {\n${codeEntries.join('\n')}\n} as const;\n\n` +
    'export type EnumName = keyof typeof ENUM_NUMERIC_CODES;\n';

  const output = `${header}\n${enumBlocks.join('\n\n')}\n\n${registry}`;
  writeFileSync(enumsOutputPath, output, 'utf8');
  console.log(`Yazıldı: ${path.relative(root, enumsOutputPath)}`);
}

async function generateSchema() {
  const url = `${backendOrigin}/openapi/v1.json`;
  console.log(`OpenAPI indiriliyor: ${url}`);

  const response = await fetch(url);
  if (!response.ok) {
    console.error(`OpenAPI indirilemedi: ${response.status} ${response.statusText}`);
    process.exit(1);
  }
  const json = await response.json();
  writeFileSync(openapiPath, `${JSON.stringify(json, null, 2)}\n`, 'utf8');
  console.log(`Yazıldı: ${path.relative(root, openapiPath)}`);

  // openapi-typescript yalnız packages/api-client bağımlılığıdır; onun kendi
  // node_modules'ünden çözülür (kök workspace'e hoist edilmemiş olabilir).
  // require.resolve CJS girişini bulur; ESM girişini paket.json "exports"ından okuyoruz.
  const require = createRequire(import.meta.url);
  const pkgJsonPath = require.resolve('openapi-typescript/package.json', { paths: [apiClientDir] });
  const pkgJson = JSON.parse(await readFile(pkgJsonPath, 'utf8'));
  const esmEntry = path.join(path.dirname(pkgJsonPath), pkgJson.exports['.'].import);
  const { default: openapiTS, astToString } = await import(pathToFileURL(esmEntry).href);

  const ast = await openapiTS(json);
  const astOutput = astToString(ast);
  writeFileSync(schemaPath, astOutput, 'utf8');
  console.log(`Yazıldı: ${path.relative(root, schemaPath)}`);
}

await generateEnums();
await generateSchema();
