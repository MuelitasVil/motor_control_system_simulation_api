import { useCallback, useEffect, useState } from 'react'
import { motorControlClient } from '../../../clients/motorControlClient'
import type { RequestStatus } from '../../../shared/types/request'
import { getErrorMessage } from '../../../shared/utils/getErrorMessage'

export function useGreeting() {
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [status, setStatus] = useState<RequestStatus>('idle')
  const loadGreeting = useCallback(async () => {
    setStatus('loading')
    setError('')

    try {
      const greeting = await motorControlClient.getGreeting()
      setMessage(greeting.replace(/^"|"$/g, ''))
      setStatus('success')
    } catch (requestError) {
      setError(getErrorMessage(requestError))
      setStatus('error')
    }
  }, [])

  useEffect(() => {
    void loadGreeting()
  }, [loadGreeting])

  return { message, error, status, reload: loadGreeting }
}
