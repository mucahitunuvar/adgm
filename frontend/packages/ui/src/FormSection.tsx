import type { ReactNode } from 'react';

export interface FormSectionProps {
  title: string;
  description?: string;
  children: ReactNode;
  actions?: ReactNode;
}

/** Bir form içinde ilişkili alanları başlık/açıklama ile görsel olarak gruplar. */
export function FormSection({ title, description, children, actions }: FormSectionProps) {
  return (
    <section className="form-section">
      <div className="form-section__header">
        <div>
          <h2>{title}</h2>
          {description && <p>{description}</p>}
        </div>
        {actions && <div className="form-section__actions">{actions}</div>}
      </div>
      <div className="form-section__body">{children}</div>
    </section>
  );
}
