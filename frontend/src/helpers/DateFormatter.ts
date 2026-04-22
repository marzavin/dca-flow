export const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

function pad(value: number): string {
  return value.toString().padStart(2, '0');
}

function toDate(input: Date | string | number): Date {
  return input instanceof Date ? input : new Date(input);
}

// SHORT DATE
// 07 Apr 2026
export function formatDateShort(input: Date | string | number): string {
  const date = toDate(input);

  const day = pad(date.getDate());
  const month = MONTHS[date.getMonth()];
  const year = date.getFullYear();

  return `${day} ${month} ${year}`;
}

// DATE + TIME
// 07 Apr 2026, 14:35
export function formatDateTime(input: Date | string | number): string {
  const date = toDate(input);

  const day = pad(date.getDate());
  const month = MONTHS[date.getMonth()];
  const year = date.getFullYear();
  const hours = pad(date.getHours());
  const minutes = pad(date.getMinutes());

  return `${day} ${month} ${year}, ${hours}:${minutes}`;
}

// AXIS FORMAT
// 07 Apr
export function formatDateAxis(input: Date | string | number): string {
  const date = toDate(input);

  const day = pad(date.getDate());
  const month = MONTHS[date.getMonth()];

  return `${day} ${month}`;
}

// TIME ONLY
// 14:35
export function formatTimeOnly(input: Date | string | number): string {
  const date = toDate(input);

  const hours = pad(date.getHours());
  const minutes = pad(date.getMinutes());

  return `${hours}:${minutes}`;
}

// RELATIVE TIME
// just now
// 2 min ago
// 3 hours ago
// Yesterday
// 5 days ago
export function formatDateRelative(input: Date | string | number): string {
  const date = toDate(input);
  const now = new Date();

  const diffMs = now.getTime() - date.getTime();
  const diffSec = Math.floor(diffMs / 1000);
  const diffMin = Math.floor(diffSec / 60);
  const diffHours = Math.floor(diffMin / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffSec < 30) {
    return 'just now';
  }
  if (diffMin < 60) {
    return `${diffMin} min ago`;
  }
  if (diffHours < 24) {
    return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
  }
  if (diffDays === 1) {
    return 'Yesterday';
  }
  if (diffDays < 7) {
    return `${diffDays} days ago`;
  }

  return formatDateShort(date);
}

// TOOLTIP FORMAT
export function formatDateTooltip(input: Date | string | number): string {
  return formatDateTime(input);
}
