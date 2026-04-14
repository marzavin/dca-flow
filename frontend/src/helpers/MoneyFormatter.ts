function addThousandsSeparator(value: string): string {
  const parts = value.split('.');
  parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
  return parts.join('.');
}

// FULL FORMAT
// $1,234.56
// $999,999.99
export function formatMoneyFull(value: number): string {
  const fixed = value.toFixed(value % 1 === 0 ? 0 : 2);
  return `$${addThousandsSeparator(fixed)}`;
}

// COMPACT FORMAT
// $1.2K
// $3.4M
// $2.1B
export function formatMoneyCompact(value: number): string {
  const abs = Math.abs(value);

  if (abs >= 1_000_000_000) {
    return `$${(value / 1_000_000_000).toFixed(1).replace(/\.0$/, '')}B`;
  }

  if (abs >= 1_000_000) {
    return `$${(value / 1_000_000).toFixed(1).replace(/\.0$/, '')}M`;
  }

  if (abs >= 1_000) {
    return `$${(value / 1_000).toFixed(1).replace(/\.0$/, '')}K`;
  }

  return `$${value.toFixed(2).replace(/\.00$/, '')}`;
}

// ADAPTIVE FORMAT
export function formatMoneyAdaptive(value: number): string {
  const abs = Math.abs(value);

  if (abs < 1000) {
    return formatMoneyFull(value);
  }

  return formatMoneyCompact(value);
}

// AXIS FORMAT
export function formatMoneyAxis(value: number): string {
  const abs = Math.abs(value);

  if (abs >= 1_000_000_000) {
    return `$${Math.round(value / 1_000_000_000)}B`;
  }

  if (abs >= 1_000_000) {
    return `$${Math.round(value / 1_000_000)}M`;
  }

  if (abs >= 1_000) {
    return `$${Math.round(value / 1_000)}K`;
  }

  return `$${Math.round(value)}`;
}

// TOOLTIP FORMAT
export function formatMoneyTooltip(value: number): string {
  return formatMoneyFull(value);
}
