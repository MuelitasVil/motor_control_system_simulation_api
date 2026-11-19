import type { PropsWithChildren } from 'react'

export function DashboardLayout({ children }: PropsWithChildren) {
  return <div className="app-background">{children}</div>
}
