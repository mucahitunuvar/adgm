export function Brand({ compact = false, href = '/' }: { compact?: boolean; href?: string }) {
  return (
    <a className="brand" href={href} aria-label="Ahmet Demir Gençlik Merkezi ana sayfa">
      <span className="brand-mark" aria-hidden="true" />
      {!compact && (
        <span>
          <strong>Ahmet Demir</strong>
          <small>Gençlik Merkezi</small>
        </span>
      )}
    </a>
  );
}
