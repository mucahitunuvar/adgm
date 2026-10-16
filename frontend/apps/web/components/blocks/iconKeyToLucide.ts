import { BarChart3, Briefcase, Calendar, Heart, type LucideIcon, Sparkles, Users } from 'lucide-react';

/** Admin'in serbest metin `iconKey` alanı (`[a-z0-9-]`) için bilinen küçük bir eşleme; tanınmayan anahtar güvenle genel bir ikona düşer. */
const ICON_MAP: Record<string, LucideIcon> = {
  users: Users,
  heart: Heart,
  calendar: Calendar,
  briefcase: Briefcase,
  chart: BarChart3,
};

export function iconKeyToLucide(iconKey: string | null | undefined): LucideIcon {
  if (!iconKey) return Sparkles;
  return ICON_MAP[iconKey] ?? Sparkles;
}
