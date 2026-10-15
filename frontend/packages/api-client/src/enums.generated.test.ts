import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import { describe, expect, it } from 'vitest';
import { ENUM_NUMERIC_CODES } from './enums.generated';

/**
 * `enums.generated.ts` elle düzenlenmez; bu test onun hâlâ `docs/api/enums.v1.json`
 * manifestiyle eşleştiğini doğrular. Manifest değişip `pnpm api:gen` çalıştırılmazsa
 * bu test düşer (Görev 0b, "CI drift" kontrolü).
 */
describe('enums.generated.ts <-> docs/api/enums.v1.json', () => {
  it('stays in sync with the manifest the file was generated from', () => {
    const __dirname = path.dirname(fileURLToPath(import.meta.url));
    const manifestPath = path.join(__dirname, '..', '..', '..', '..', 'docs', 'api', 'enums.v1.json');
    const manifest = JSON.parse(readFileSync(manifestPath, 'utf8')) as Record<string, Record<string, Record<string, number>>>;

    const expectedCodes: Record<string, Record<string, number>> = {};
    for (const moduleEnums of Object.values(manifest)) {
      for (const [enumName, members] of Object.entries(moduleEnums)) {
        expectedCodes[enumName] = members;
      }
    }

    expect(ENUM_NUMERIC_CODES).toEqual(expectedCodes);
  });
});
