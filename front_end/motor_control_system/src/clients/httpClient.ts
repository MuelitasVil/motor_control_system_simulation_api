import { env } from '../config/env'
export async function requestJson<T>(path: string, options?: RequestInit): Promise<T> {
  const headers = new Headers(options?.headers)
  headers.set('Accept', 'application/json')
  if (options?.body) headers.set('Content-Type', 'application/json')
  const response = await fetch(`${env.apiBaseUrl}${path}`, { ...options, headers })

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    const validation = problem?.errors
      ? Object.values(problem.errors).flat().join(' ')
      : ''
    throw new Error(problem?.detail || validation || problem?.title || `Error HTTP ${response.status}`)
  }

  return response.json() as Promise<T>
}
