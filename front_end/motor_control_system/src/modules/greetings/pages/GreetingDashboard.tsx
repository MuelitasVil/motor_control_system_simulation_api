import { APP_NAME } from '../../../config/constants'
import { StatusIndicator } from '../../../shared/components/StatusIndicator'
import { useDocumentTitle } from '../../../shared/hooks/useDocumentTitle'
import { useGreeting } from '../hooks/useGreeting'

export function GreetingDashboard() {
  const { message, error, status, reload } = useGreeting()
  const isLoading = status === 'loading'
  useDocumentTitle(`${APP_NAME} | Dashboard`)
  return (
    <main className="dashboard-shell">
      <header className="topbar">
        <a className="brand" href="/" aria-label={`${APP_NAME} home`}>
          <span className="brand-mark" aria-hidden="true">M</span>
          <span>{APP_NAME}</span>
        </a>
        <StatusIndicator
          status={status}
          label={status === 'success' ? 'API connected' : status === 'error' ? 'API unavailable' : 'Checking API'}
        />
      </header>

      <section className="hero-section">
        <p className="eyebrow">BACKEND CONNECTION CHECK</p>
        <h1>Welcome to your<br /><span>motor dashboard.</span></h1>
        <p className="hero-copy">
          This dashboard is connected to the Motor Control API. The greeting below is returned by the C# backend.
        </p>
      </section>

      <section className="api-card" aria-live="polite">
        <div className="card-heading">
          <div>
            <p className="eyebrow">GET /api/greeting</p>
            <h2>Backend response</h2>
          </div>
          <span className={`response-badge ${status}`}>
            {status === 'success' ? '200 OK' : status === 'error' ? 'ERROR' : 'PENDING'}
          </span>
        </div>

        <div className="response-content">
          {isLoading && <p className="response-message muted">Connecting to the API…</p>}
          {status === 'success' && <p className="response-message">{message}</p>}
          {status === 'error' && (
            <div className="error-message">
              <strong>Could not reach the backend.</strong>
              <span>{error}</span>
            </div>
          )}
        </div>

        <div className="card-footer">
          <span className="endpoint-label"><span className="method">GET</span> /api/greeting</span>
          <button className="refresh-button" type="button" onClick={() => void reload()} disabled={isLoading}>
            {isLoading ? 'Loading…' : 'Send request'}
            <span aria-hidden="true">↗</span>
          </button>
        </div>
      </section>

      <footer className="page-footer">
        <span>Motor Control System Simulation</span>
        <span>React · Vite · ASP.NET Core</span>
      </footer>
    </main>
  )
}
