interface Step {
  title: string;
  text: string;
}

export function ProcessStepsList({ steps }: { steps: Step[] }) {
  return (
    <ol className="block-process-steps__list">
      {steps.map((step, index) => (
        <li key={index}>
          <h3>{step.title}</h3>
          <p>{step.text}</p>
        </li>
      ))}
    </ol>
  );
}

/** Tek başına (çiftlenmemiş) bir `process-steps` bloğu - ardışık ikili eşleşmesi `HomePage`'de yapılır. */
export function ProcessStepsBlock({ texts }: { texts: { title?: string; steps?: Step[] } | null }) {
  if (!texts?.steps || texts.steps.length === 0) return null;
  return (
    <section className="container section block-process-steps">
      {texts.title && <h2>{texts.title}</h2>}
      <ProcessStepsList steps={texts.steps} />
    </section>
  );
}
