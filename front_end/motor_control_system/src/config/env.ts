const apiBaseUrl = 'http://localhost:5168/api'

export const env = {
  apiBaseUrl: apiBaseUrl.replace(/\/$/, ''),
} as const
