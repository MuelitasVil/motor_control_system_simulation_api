import { HubConnectionBuilder } from '@microsoft/signalr'
import type { MotorState } from '../modules/dashboard/types'

export type ConnectionStatus = 'idle' | 'connecting' | 'connected' | 'reconnecting'

export function listenToMotors(
  ids: string[], onState: (state: MotorState) => void, onStatus: (status: ConnectionStatus) => void,
) {
  const connection = new HubConnectionBuilder().withUrl('/hubs/motor-telemetry').build()
  const subscribedIds = new Set(ids)
  let closed = false
  let retry: ReturnType<typeof setTimeout> | undefined

  connection.on('MotorStateUpdated', (state: MotorState) => {
    if (!closed && subscribedIds.has(state.associatedMotorId)) onState(state)
  })

  function scheduleReconnect() {
    if (closed || retry !== undefined) return
    onStatus('reconnecting')
    retry = setTimeout(() => {
      retry = undefined
      void connect()
    }, 2500)
  }

  async function connect() {
    try {
      await connection.start()
      for (const id of ids) {
        if (closed) return
        await connection.invoke('SubscribeToMotor', id)
      }
      if (!closed) onStatus('connected')
    } catch {
      if (!closed) {
        await connection.stop()
        scheduleReconnect()
      }
    }
  }

  connection.onclose(scheduleReconnect)
  onStatus('connecting')
  void connect()

  return () => {
    closed = true
    clearTimeout(retry)
    connection.off('MotorStateUpdated')
    void connection.stop()
  }
}
