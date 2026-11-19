import { useCallback, useEffect, useRef, useState } from 'react'
import { fleetClient } from '../../../clients/fleetClient'
import { listenToMotors, type ConnectionStatus } from '../../../clients/motorTelemetryClient'
import type { MotorState } from '../types'

type Fleet = Awaited<ReturnType<typeof fleetClient.getFleet>>

export const errorMessage = (error: unknown) =>
  error instanceof Error ? error.message : 'Ocurrió un error inesperado.'

export function useFleet() {
  const [fleet, setFleet] = useState<Fleet>()
  const [error, setError] = useState('')
  const [telemetry, setTelemetry] = useState<Record<string, MotorState>>({})
  const [status, setStatus] = useState<ConnectionStatus>('idle')
  const pending = useRef<AbortController | null>(null)

  const refresh = useCallback(async () => {
    pending.current?.abort()
    const request = new AbortController()
    pending.current = request
    try {
      const data = await fleetClient.getFleet(request.signal)
      if (request.signal.aborted) return
      setFleet(data)
      const activeIds = new Set(data.associations.filter((item) => item.operationState === 'InTrip')
        .map((item) => item.associatedMotorId))
      setTelemetry((current) => Object.fromEntries(
        Object.entries(current).filter(([id]) => activeIds.has(id)),
      ))
      setError('')
    } catch (cause) {
      if (!request.signal.aborted) setError(errorMessage(cause))
    } finally {
      if (pending.current === request) pending.current = null
    }
  }, [])

  useEffect(() => {
    const initialLoad = setTimeout(() => void refresh(), 0)
    const interval = setInterval(() => {
      if (!pending.current) void refresh()
    }, 12000)
    return () => {
      clearInterval(interval)
      clearTimeout(initialLoad)
      pending.current?.abort()
    }
  }, [refresh])

  const receive = useCallback((state: MotorState) => {
    setTelemetry((current) => {
      const previous = current[state.associatedMotorId]
      if (previous && Date.parse(previous.updatedAt) > Date.parse(state.updatedAt)) return current
      return { ...current, [state.associatedMotorId]: state }
    })
  }, [])

  const subscriptionKey = fleet?.associations
    .filter((motor) => motor.operationState === 'InTrip')
    .map((motor) => motor.associatedMotorId).sort().join(',') ?? ''

  useEffect(() => {
    if (!subscriptionKey) return
    return listenToMotors(subscriptionKey.split(','), receive, setStatus)
  }, [subscriptionKey, receive])

  return { fleet, error, refresh, telemetry, receive, status: subscriptionKey ? status : 'idle' }
}
