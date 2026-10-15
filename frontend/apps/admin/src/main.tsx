import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { getStoredTheme, getSystemTheme } from '@gm/design-tokens';
import '@fontsource/manrope/latin-ext-400.css';
import '@fontsource/manrope/latin-ext-600.css';
import '@fontsource/manrope/latin-ext-700.css';
import '@fontsource/manrope/latin-ext-800.css';
import '@fontsource/poppins/latin-ext-600.css';
import '@fontsource/poppins/latin-ext-700.css';
import '@gm/design-tokens/tokens.css';
import '@gm/design-tokens/base.css';
import './app.css';
import { App } from './App';

document.documentElement.classList.add(getStoredTheme() ?? getSystemTheme());

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
