/**
 * Format date string (YYYY-MM-DD) as "DD Month YYYY" (e.g. 16 February 2025).
 */
export function formatDateDMY(dateStr: string): string {
  const d = new Date(dateStr + 'T12:00:00');
  if (Number.isNaN(d.getTime())) return dateStr;
  const day = d.getDate();
  const month = d.toLocaleString('en-IN', { month: 'long' });
  const year = d.getFullYear();
  return `${day} ${month} ${year}`;
}

/**
 * Format time string (HH:mm or HH:mm:ss) as 12-hour format (e.g. 9:00 AM, 2:30 PM).
 */
export function formatTime12h(timeStr: string): string {
  const part = timeStr.trim().slice(0, 5); // "HH:mm"
  const [hStr, mStr] = part.split(':');
  const h = parseInt(hStr ?? '0', 10);
  const m = parseInt(mStr ?? '0', 10);
  if (Number.isNaN(h)) return timeStr;
  const period = h < 12 ? 'AM' : 'PM';
  const hour12 = h === 0 ? 12 : h > 12 ? h - 12 : h;
  const mm = String(m).padStart(2, '0');
  return `${hour12}:${mm} ${period}`;
}
