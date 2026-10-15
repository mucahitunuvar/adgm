import { loadEnv } from 'vite';
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const backendOrigin = env.VITE_BACKEND_ORIGIN ?? env.BACKEND_ORIGIN ?? 'http://localhost:5016';

  return {
    base: '/admin/',
    plugins: [react()],
    server: {
      port: 5173,
      headers: { 'X-Robots-Tag': 'noindex' },
      proxy: {
        '/api': { target: backendOrigin, changeOrigin: true },
      },
    },
    preview: { port: 5173 },
    build: { outDir: 'dist' },
    test: {
      environment: 'jsdom',
      setupFiles: ['./vitest.setup.ts'],
      include: ['src/**/*.test.{ts,tsx}'],
      css: false,
    },
  };
});
