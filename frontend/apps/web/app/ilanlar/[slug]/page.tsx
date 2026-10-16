import { notFound } from 'next/navigation';
import type { Metadata } from 'next';
import { getSite } from '../../../lib/getSite';
import { JobDetailPage, fetchJobDetail } from '../../../components/jobs/JobDetailPage';

interface JobDetailPageProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata({ params }: JobDetailPageProps): Promise<Metadata> {
  const { slug } = await params;
  const site = await getSite();
  if (!site.publicJobListingsEnabled) return {};
  const job = await fetchJobDetail(slug);
  return { title: `${job.title} · ${job.company.name}` };
}

export default async function JobDetailRoute({ params }: JobDetailPageProps) {
  const { slug } = await params;
  const site = await getSite();
  if (!site.publicJobListingsEnabled) notFound();
  return <JobDetailPage slug={slug} />;
}
