// Formats an ISO timestamp into a short, human-friendly relative string
// (e.g. "5 min ago"). Shared by feature mappers.
export const formatRelativeDate = (value: string): string => {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '-';

  const diffMs = Date.now() - date.getTime();
  const diffMinutes = Math.max(0, Math.floor(diffMs / 60_000));
  if (diffMinutes < 1) return 'just now';
  if (diffMinutes < 60) return `${diffMinutes} min ago`;

  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) return `${diffHours} hour${diffHours === 1 ? '' : 's'} ago`;

  const diffDays = Math.floor(diffHours / 24);
  return `${diffDays} day${diffDays === 1 ? '' : 's'} ago`;
};
