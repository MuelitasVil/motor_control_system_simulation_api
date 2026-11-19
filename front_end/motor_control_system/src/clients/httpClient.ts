import { env } from '../config/env'

export async function getText(path: string, signal?: AbortSignal): Promise<string> {
  console.log('Fetching from API Base URL:', env.apiBaseUrl)
  const response = await fetch(`${env.apiBaseUrl}${path}`, {
    method: 'GET',
    headers: { Accept: 'text/plain, application/json' },
    signal,
  })

  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || `Request failed with status ${response.status}`)
  }

  const contentType = response.headers.get('content-type') ?? ''
  if (contentType.includes('application/json')) {
    return response.json() as Promise<string>
  }

  return response.text()
}
