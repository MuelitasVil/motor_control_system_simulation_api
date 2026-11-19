import type { ReactNode } from 'react'

export function MetricCard({ label, value, icon, tone }: {
  label: string; value: number | undefined; icon: ReactNode; tone: string
}) {
  return <article className={`metric metric--${tone}`}>
    <div className="metric-icon" aria-hidden="true">{icon}</div>
    <div><strong>{value ?? '—'}</strong><p>{label}</p></div>
  </article>
}
