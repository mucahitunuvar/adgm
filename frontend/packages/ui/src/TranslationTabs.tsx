import type { ReactNode } from 'react';
import { Tabs } from './Tabs';

export interface TranslationLanguage {
  code: string;
  name: string;
}

export interface TranslationTabsProps {
  languages: readonly TranslationLanguage[];
  activeCode: string;
  onChange: (code: string) => void;
  renderPanel: (languageCode: string) => ReactNode;
}

/**
 * Dil sekmeleri. Tek aktif dil varsa sekme listesi gizlenir, panel doğrudan gösterilir
 * (Faz 2 Ortak ilkeler: "tek aktif dil varsa sekme gizlenir").
 */
export function TranslationTabs({ languages, activeCode, onChange, renderPanel }: TranslationTabsProps) {
  if (languages.length <= 1) {
    const only = languages[0];
    return only ? <>{renderPanel(only.code)}</> : null;
  }

  return (
    <Tabs
      items={languages.map((language) => ({ key: language.code, label: language.name, content: renderPanel(language.code) }))}
      activeKey={activeCode}
      onChange={onChange}
    />
  );
}
