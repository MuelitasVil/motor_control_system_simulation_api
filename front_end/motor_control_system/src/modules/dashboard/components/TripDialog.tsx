import { useEffect, useState, type FormEvent } from 'react'
import { fleetClient } from '../../../clients/fleetClient'
import { Button } from '../../../shared/components/Button'
import { Popup } from '../../../shared/components/Popup'
import { errorMessage } from '../hooks/useFleet'
import type { CreateTripRequest, CrewMember } from '../types'

export function TripDialog({ options, onClose, onCreated }: {
  options: { id: string; label: string }[]; onClose: () => void; onCreated: () => void
}) {
  const [crew, setCrew] = useState<CrewMember[]>([])
  const [crewError, setCrewError] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  useEffect(() => {
    const request = new AbortController()
    fleetClient.getCrew(request.signal).then(setCrew).catch(() => {
      if (!request.signal.aborted) setCrewError('No se pudo cargar la lista de tripulantes.')
    })
    return () => request.abort()
  }, [])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    const request: CreateTripRequest = {
      associatedMotorId: String(form.get('motor')),
      startPoint: String(form.get('origin')).trim(),
      endPoint: String(form.get('destination')).trim(),
      totalDistance: Number(form.get('distance')),
      cargoWeight: Number(form.get('cargo')),
      crewMemberIds: form.getAll('crew').map(String),
    }
    if (!request.startPoint || !request.endPoint) {
      setError('Escribe el origen y el destino.')
      return
    }
    setBusy(true)
    setError('')
    try {
      await fleetClient.createTrip(request)
      onCreated()
    } catch (cause) {
      setError(errorMessage(cause))
      setBusy(false)
    }
  }

  return <Popup title="Crear viaje" busy={busy} onClose={onClose}>
    <form onSubmit={submit}>
      <fieldset disabled={busy}>
        <label>Vehículo y motor<select name="motor" defaultValue="" required>
          <option value="" disabled>Selecciona un vehículo</option>
          {options.map(({ id, label }) => <option key={id} value={id}>{label}</option>)}
        </select></label>
        <div className="form-grid">
          <label>Origen<input name="origin" placeholder="Bogotá" required /></label>
          <label>Destino<input name="destination" placeholder="Tunja" required /></label>
          <label>Distancia (km)<input name="distance" type="number" min="0.01" step="any" required /></label>
          <label>Carga (kg)<input name="cargo" type="number" min="0" step="any" defaultValue="0" required /></label>
        </div>
        {crew.length > 0 && <details><summary>Tripulantes (opcional)</summary>
          <div className="crew-list">{crew.map((member) => <label key={member.crewMemberId}>
            <input type="checkbox" name="crew" value={member.crewMemberId} /> {member.name}
          </label>)}</div>
        </details>}
        {crewError && <p className="muted">{crewError}</p>}
      </fieldset>
      {error && <p className="form-error" role="alert">{error}</p>}
      <footer><Button onClick={onClose} disabled={busy}>Cancelar</Button>
        <Button type="submit" variant="primary" disabled={busy || options.length === 0}>{busy ? 'Creando…' : 'Iniciar viaje'}</Button></footer>
    </form>
  </Popup>
}
