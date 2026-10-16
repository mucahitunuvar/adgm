import { FileText } from 'lucide-react';
import { safeUrl } from '@gm/ui';
import type { PublicContentDetailResponse } from '@gm/api-client';

function formatFileSize(sizeInBytes: number | string): string {
  const bytes = Number(sizeInBytes);
  if (!Number.isFinite(bytes) || bytes <= 0) return '';
  const megabytes = bytes / (1024 * 1024);
  if (megabytes >= 1) return `${megabytes.toFixed(1)} MB`;
  return `${Math.max(1, Math.round(bytes / 1024))} KB`;
}

export function AttachmentListSection({ attachments }: { attachments: PublicContentDetailResponse['attachments'] }) {
  if (attachments.length === 0) return null;
  return (
    <section className="content-detail__section">
      <h2>Ekli dosyalar</h2>
      <ul className="attachment-list">
        {attachments.map((attachment, index) => {
          const href = safeUrl(attachment.url);
          if (!href) return null;
          return (
            <li key={`${attachment.name}-${index}`}>
              <a href={href} download>
                <FileText aria-hidden="true" />
                <span>{attachment.name}</span>
                <span className="attachment-list__meta">
                  {attachment.extension.toUpperCase()} · {formatFileSize(attachment.sizeInBytes)}
                </span>
              </a>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
