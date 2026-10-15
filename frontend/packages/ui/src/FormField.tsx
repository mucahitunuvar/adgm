import type { InputHTMLAttributes, SelectHTMLAttributes, TextareaHTMLAttributes } from 'react';

interface BaseProps {
  id: string;
  label: string;
  hint?: string;
  error?: string;
  required?: boolean;
}

export function TextField({ id, label, hint, error, required, ...props }: BaseProps & InputHTMLAttributes<HTMLInputElement>) {
  const descriptionId = `${id}-description`;
  return (
    <div className={`form-field${error ? ' form-field--error' : ''}`}>
      <label htmlFor={id}>
        {label}
        {required ? <span aria-hidden="true"> *</span> : <span className="optional"> (isteğe bağlı)</span>}
      </label>
      {hint && <p className="field-hint" id={descriptionId}>{hint}</p>}
      <input id={id} required={required} aria-invalid={Boolean(error)} aria-describedby={hint || error ? descriptionId : undefined} {...props} />
      {error && <p className="field-error" id={descriptionId}>{error}</p>}
    </div>
  );
}

export function TextAreaField({ id, label, hint, error, required, ...props }: BaseProps & TextareaHTMLAttributes<HTMLTextAreaElement>) {
  const descriptionId = `${id}-description`;
  return (
    <div className={`form-field${error ? ' form-field--error' : ''}`}>
      <label htmlFor={id}>
        {label}
        {required ? <span aria-hidden="true"> *</span> : <span className="optional"> (isteğe bağlı)</span>}
      </label>
      {hint && <p className="field-hint" id={descriptionId}>{hint}</p>}
      <textarea id={id} required={required} aria-invalid={Boolean(error)} aria-describedby={hint || error ? descriptionId : undefined} {...props} />
      {error && <p className="field-error" id={descriptionId}>{error}</p>}
    </div>
  );
}

export function SelectField({ id, label, hint, error, required, children, ...props }: BaseProps & SelectHTMLAttributes<HTMLSelectElement>) {
  const descriptionId = `${id}-description`;
  return (
    <div className={`form-field${error ? ' form-field--error' : ''}`}>
      <label htmlFor={id}>
        {label}
        {required ? <span aria-hidden="true"> *</span> : <span className="optional"> (isteğe bağlı)</span>}
      </label>
      {hint && <p className="field-hint" id={descriptionId}>{hint}</p>}
      <select id={id} required={required} aria-invalid={Boolean(error)} aria-describedby={hint || error ? descriptionId : undefined} {...props}>
        {children}
      </select>
      {error && <p className="field-error" id={descriptionId}>{error}</p>}
    </div>
  );
}

/** `required`/`optional` ibaresi yoktur: işaretli/işaretsiz bir anahtar (flag), zorunluluk kavramı anlamsız. */
export function CheckboxField({
  id,
  label,
  hint,
  ...props
}: Omit<BaseProps, 'required' | 'error'> & InputHTMLAttributes<HTMLInputElement>) {
  const descriptionId = `${id}-description`;
  return (
    <div className="checkbox-field">
      <input id={id} type="checkbox" aria-describedby={hint ? descriptionId : undefined} {...props} />
      <div>
        <label htmlFor={id}>{label}</label>
        {hint && <p className="field-hint" id={descriptionId}>{hint}</p>}
      </div>
    </div>
  );
}
