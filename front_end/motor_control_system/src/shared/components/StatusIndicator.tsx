import type { RequestStatus } from '../types/request'

type StatusIndicatorProps = {
  status: RequestStatus
  label: string
}

export function StatusIndicator({ status, label }: StatusIndicatorProps) {
  return (
    <div className="connection-indicator">
      <span className={`status-dot ${status}`} />
      {label}
    </div>
  )
}
