import { useState, type FormEvent } from 'react'
import { fleetClient } from '../../../clients/fleetClient'
import { Button } from '../../../shared/components/Button'
import { Popup } from '../../../shared/components/Popup'
import { errorMessage } from '../hooks/useFleet'
import { drivingModes, type DrivingMode, type MotorState } from '../types'

export function DrivingDialog({ kind, id, vehicle, speed, mode, onClose, onUpdated }: {
  kind: 'speed' | 'mode'; id: string; vehicle: string; speed: number; mode: DrivingMode
  onClose: () => void; onUpdated: (state: MotorState, notice: string) => void
}) {
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    const requestedSpeed = kind === 'speed' ? Number(form.get('speed')) : speed
    const drivingMode = kind === 'mode' ? form.get('mode') as DrivingMode : mode
    setBusy(true)
    setError('')
    try {
      const updated = await fleetClient.updateSpeed(id, requestedSpeed, drivingMode)
      const applied = updated.appliedSpeed.toLocaleString('es-CO', { maximumFractionDigits: 2 })
      onUpdated(updated, `${vehicle} · ${applied} km/h · modo ${updated.drivingMode}.`)
    } catch (cause) {
      setBusy(false)
      setError(errorMessage(cause))
    }
  }

  return <Popup title={kind === 'speed' ? 'Cambiar velocidad' : 'Cambiar modo'} busy={busy} onClose={onClose}>
    <p className="muted">{vehicle} · {speed.toLocaleString('es-CO', { maximumFractionDigits: 2 })} km/h · {mode}</p>
    <form onSubmit={submit}>
      <fieldset disabled={busy}>
        {kind === 'speed'
          ? <label>Velocidad solicitada (km/h)<input name="speed" type="number" min="0" step="any" defaultValue={speed} required /></label>
          : <label>Modo de conducción<select name="mode" defaultValue={mode}>
            {drivingModes.map((option) => <option key={option}>{option}</option>)}
          </select></label>}
      </fieldset>
      {error && <p className="form-error" role="alert">{error}</p>}
      <footer><Button onClick={onClose} disabled={busy}>Cancelar</Button>
        <Button type="submit" variant="primary" disabled={busy}>{busy ? 'Guardando…' : 'Aplicar cambio'}</Button></footer>
    </form>
  </Popup>
}
