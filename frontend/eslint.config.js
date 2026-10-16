import js from '@eslint/js';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import tseslint from 'typescript-eslint';

const storageRestrictions = [
  {
    selector: "MemberExpression[object.name='window'][property.name='localStorage']",
    message: 'localStorage yasak (K-07). Yalnız packages/design-tokens/src/theme.ts tema anahtarı için izinlidir.',
  },
  {
    selector: "MemberExpression[object.name='window'][property.name='sessionStorage']",
    message: 'sessionStorage yasak (K-07, FRONTEND-GUVENLIK.md §2).',
  },
  {
    selector: "Identifier[name='localStorage']",
    message: 'localStorage yasak (K-07). Yalnız packages/design-tokens/src/theme.ts tema anahtarı için izinlidir.',
  },
  {
    selector: "Identifier[name='sessionStorage']",
    message: 'sessionStorage yasak (K-07, FRONTEND-GUVENLIK.md §2).',
  },
  {
    selector: "MemberExpression[object.name='document'][property.name='cookie']",
    message: 'document.cookie yasak. Yalnız apps/web/lib/consentStore.ts (çerez tercihi) için izinlidir (FRONTEND-FAZ3-MASTER-PROMPT.md "Depolama kuralı").',
  },
];

const restrictedGlobals = [
  ...storageRestrictions,
  {
    selector: "JSXAttribute[name.name='dangerouslySetInnerHTML']",
    message: 'dangerouslySetInnerHTML yalnız packages/ui/src/SafeHtml.tsx içinde izinlidir.',
  },
  {
    selector: "MemberExpression[property.name='innerHTML']",
    message: 'innerHTML yasak (FRONTEND-GUVENLIK.md §3).',
  },
  {
    selector: "CallExpression[callee.name='eval']",
    message: 'eval yasak (FRONTEND-GUVENLIK.md §3).',
  },
  {
    selector: "NewExpression[callee.name='Function']",
    message: 'new Function yasak (FRONTEND-GUVENLIK.md §3).',
  },
  {
    selector: "MemberExpression[object.name='document'][property.name='write']",
    message: 'document.write yasak (FRONTEND-GUVENLIK.md §3).',
  },
];

export default tseslint.config(
  { ignores: ['**/dist/**', '**/.next/**', '**/node_modules/**', '**/playwright-report/**', '**/test-results/**', '**/coverage/**'] },
  js.configs.recommended,
  ...tseslint.configs.recommended,
  {
    files: ['**/*.{ts,tsx}'],
    languageOptions: { ecmaVersion: 2022 },
    plugins: { 'react-hooks': reactHooks, 'react-refresh': reactRefresh },
    rules: {
      ...reactHooks.configs.recommended.rules,
      'react-refresh/only-export-components': 'off',
      '@typescript-eslint/no-explicit-any': 'error',
      'no-restricted-syntax': ['error', ...restrictedGlobals],
      'no-restricted-globals': ['error', { name: 'localStorage', message: 'localStorage yasak (K-07).' }, { name: 'sessionStorage', message: 'sessionStorage yasak (K-07).' }],
    },
  },
  {
    files: ['packages/ui/src/SafeHtml.tsx'],
    rules: { 'no-restricted-syntax': 'off' },
  },
  {
    files: ['packages/design-tokens/src/theme.ts'],
    rules: { 'no-restricted-syntax': 'off', 'no-restricted-globals': 'off' },
  },
  {
    files: ['apps/web/lib/consentStore.ts'],
    rules: { 'no-restricted-syntax': 'off' },
  },
  {
    // Testler (K-07/FRONTEND-GUVENLIK.md §10): storage'ın BOŞ olduğunu doğrulamak amacıyla
    // okuma yapar; yazmaz. Bu yüzden yalnız localStorage/sessionStorage erişim yasağından muaf -
    // dangerouslySetInnerHTML/innerHTML/eval/document.write yasağı testlerde de geçerli kalır.
    files: ['**/*.test.{ts,tsx}', '**/e2e/**/*.ts'],
    rules: {
      'no-restricted-globals': 'off',
      'no-restricted-syntax': ['error', ...restrictedGlobals.filter((rule) => !storageRestrictions.includes(rule))],
    },
  },
);
