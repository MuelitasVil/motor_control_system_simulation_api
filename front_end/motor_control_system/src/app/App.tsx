import { AppProviders } from './providers/AppProviders'
import { AppRouter } from './router/AppRouter'
import { DashboardLayout } from './layouts/DashboardLayout'

export function App() {
  return (
    <AppProviders>
      <DashboardLayout>
        <AppRouter />
      </DashboardLayout>
    </AppProviders>
  )
}
