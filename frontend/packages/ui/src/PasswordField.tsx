'use client';

import { Eye, EyeOff } from 'lucide-react';
import { useState, type InputHTMLAttributes } from 'react';

interface PasswordFieldProps {
  id: string;
  label: string;
  hint?: string;
  error?: string;
  required?: boolean;
}

/** `TextField` ile aynı desen; erişilebilir göster/gizle düğmesi eklenmiş parola alanı. */
export function PasswordField({
  id,
  label,
  hint,
  error,
  required,
  ...props
}: PasswordFieldProps & Omit<InputHTMLAttributes<HTMLInputElement>, 'type' | 'id'>) {
  const [visible, setVisible] = useState(false);
  const descriptionId = `${id}-description`;

  return (
    <div className={`form-field${error ? ' form-field--error' : ''}`}>
      <label htmlFor={id}>
        {label}
        {required ? <span aria-hidden="true"> *</span> : <span className="optional"> (isteğe bağlı)</span>}
      </label>
      {hint && (
        <p className="field-hint" id={descriptionId}>
          {hint}
        </p>
      )}
      <div className="password-field">
        <input
          id={id}
          type={visible ? 'text' : 'password'}
          required={required}
          aria-invalid={Boolean(error)}
          aria-describedby={hint || error ? descriptionId : undefined}
          {...props}
        />
        <button
          type="button"
          className="password-field__toggle"
          onClick={() => setVisible((current) => !current)}
          aria-label={visible ? 'Parolayı gizle' : 'Parolayı göster'}
          aria-pressed={visible}
        >
          {visible ? <EyeOff aria-hidden="true" /> : <Eye aria-hidden="true" />}
        </button>
      </div>
      {error && (
        <p className="field-error" id={descriptionId}>
          {error}
        </p>
      )}
    </div>
  );
}
