import { useState } from 'react'
import { Activity, CarFront, CircleParking, Cpu, Link2, Plus, Radio, Route } from 'lucide-react'
import { fleetClient } from '../../../clients/fleetClient'
import { Button } from '../../../shared/components/Button'
import { MetricCard } from '../../../shared/components/MetricCard'
import { Popup } from '../../../shared/components/Popup'
import { DrivingDialog } from '../components/DrivingDialog'
import { TripDialog } from '../components/TripDialog'
import { VehicleTable, type VehicleRow } from '../components/VehicleTable'
import { errorMessage, useFleet } from '../hooks/useFleet'

type Dialog = { kind: 'trip' } | { kind: 'speed' | 'mode' | 'finish' | 'recharge'; id: string }

const statusText = {
  idle: 'Sin viajes activos', connecting: 'Conectando', connected: 'Telemetría en vivo', reconnecting: 'Reconectando',
}

export function FleetDashboard() {
  const { fleet, error, refresh, telemetry, receive, status } = useFleet()
  const [dialog, setDialog] = useState<Dialog | null>(null)
  const [busy, setBusy] = useState(false)
  const [actionError, setActionError] = useState('')
  const [notice, setNotice] = useState('')
  const motors = new Map(fleet?.motors.map((motor) => [motor.motorId, motor.model]))
  const associations = fleet?.associations ?? []
  const vehicles = fleet?.vehicles ?? []
  const vehicleNames = new Map(vehicles.map((vehicle) => [vehicle.vehicleId, vehicle.identifier]))
  const active = associations.filter((motor) => motor.operationState === 'InTrip')
  const activeVehicles = new Set(active.map((motor) => motor.vehicleId))
  const installedMotors = new Set(associations.map((motor) => motor.motorId))
  const openTrips = new Map(fleet?.trips.filter((trip) => !trip.endDate).map((trip) => [trip.associatedMotorId, trip]))
  const running: VehicleRow[] = active.map((motor) => {
    const id = motor.associatedMotorId
    const trip = openTrips.get(id)
    const state = telemetry[id]
    return {
      id, vehicleId: motor.vehicleId, name: vehicleNames.get(motor.vehicleId) ?? motor.vehicleId,
      motors: motors.get(motor.motorId) ?? 'Motor asociado',
      route: trip ? `${trip.startPoint} → ${trip.endPoint}` : undefined, tripId: trip?.tripId,
      speed: state?.appliedSpeed ?? motor.currentSpeed, limit: state?.speedLimit,
      temperature: state?.temperature ?? motor.currentTemperature,
      rpm: state?.rpm ?? motor.currentRpm, power: state?.power ?? motor.currentPower,
      batteries: [state?.batteryLevel ?? motor.batteryLevel], mode: state?.drivingMode ?? motor.drivingMode,
    }
  })
  const stopped: VehicleRow[] = vehicles.filter((vehicle) => !activeVehicles.has(vehicle.vehicleId)).map((vehicle) => {
    const installed = associations.filter((motor) => motor.vehicleId === vehicle.vehicleId)
    return {
      id: vehicle.vehicleId, vehicleId: vehicle.vehicleId, name: vehicle.identifier,
      motors: installed.map((motor) => motors.get(motor.motorId) ?? 'Motor asociado').join(', '),
      batteries: installed.map((motor) => motor.batteryLevel),
    }
  })
  const tripOptions = associations.filter((motor) => motor.operationState === 'Stopped' &&
    !activeVehicles.has(motor.vehicleId)).map((motor) => ({
    id: motor.associatedMotorId,
    label: `${vehicleNames.get(motor.vehicleId) ?? motor.vehicleId} · ${motors.get(motor.motorId) ?? 'Motor asociado'}`,
  }))
  const target = dialog && dialog.kind !== 'trip'
    ? [...running, ...stopped].find((row) => row.id === dialog.id) : undefined

  function open(next: Dialog) {
    setActionError('')
    setNotice('')
    setDialog(next)
  }

  function complete(message: string) {
    setNotice(message)
    setDialog(null)
    void refresh()
  }

  async function confirm() {
    if (!dialog || !target || busy) return
    setBusy(true)
    setActionError('')
    try {
      if (dialog.kind === 'finish' && target.tripId) {
        await fleetClient.finishTrip(target.tripId)
        complete(`Viaje de ${target.name} finalizado.`)
      } else if (dialog.kind === 'recharge') {
        await fleetClient.recharge(target.vehicleId)
        complete(`Batería de ${target.name} recargada al 100 %.`)
      }
    } catch (cause) {
      setActionError(errorMessage(cause))
    } finally {
      setBusy(false)
    }
  }

  const tableActions = {
    busy,
    onDrive: (row: VehicleRow, kind: 'speed' | 'mode') => open({ id: row.id, kind }),
    onFinish: (row: VehicleRow) => open({ id: row.id, kind: 'finish' }),
    onRecharge: (row: VehicleRow) => open({ id: row.id, kind: 'recharge' }),
  }

  return <>
    <header className="topbar"><div className="topbar-inner">
      <a className="brand" href="./"><span className="brand-icon"><Activity size={20} /></span> Motor Control</a>
      <span role="status" className={`connection connection--${status}`}><Radio size={15} /> {statusText[status]}</span>
    </div></header>
    <main className="dashboard">
      <div className="dashboard-heading">
        <div><h1>Panel de flota</h1><p>Supervisa los viajes y el estado de los motores.</p></div>
        <Button variant="primary" disabled={!tripOptions.length} onClick={() => open({ kind: 'trip' })}>
          <Plus size={19} /> Crear viaje
        </Button>
      </div>
      <section className="metrics" aria-label="Resumen de flota">
        <MetricCard label="Motores sin asociar" value={fleet?.motors.filter((motor) => !installedMotors.has(motor.motorId)).length} icon={<Cpu size={21} />} tone="neutral" />
        <MetricCard label="Motores asociados" value={fleet && installedMotors.size} icon={<Link2 size={21} />} tone="blue" />
        <MetricCard label="Vehículos detenidos" value={fleet && stopped.length} icon={<CircleParking size={21} />} tone="amber" />
        <MetricCard label="Vehículos en viaje" value={fleet && activeVehicles.size} icon={<Route size={21} />} tone="green" />
      </section>
      {error && <div className="alert" role="alert"><span>No se pudo actualizar la flota. {error}</span>
        <Button onClick={() => void refresh()}>Reintentar</Button></div>}
      {notice && <p className="notice" role="status">{notice}</p>}
      {!fleet && !error && <p className="notice" role="status">Cargando flota…</p>}
      {fleet && <>
        <section className="panel" aria-labelledby="running-heading">
          <header className="panel-heading"><span className="section-icon section-icon--active"><Route size={20} /></span>
            <div><h2 id="running-heading">Vehículos en viaje</h2><p>Velocidad y estado de cada motor durante el viaje.</p></div>
          </header>
          <VehicleTable {...tableActions} rows={running} running />
        </section>
        <section className="panel" aria-labelledby="stopped-heading">
          <header className="panel-heading"><span className="section-icon"><CarFront size={20} /></span>
            <div><h2 id="stopped-heading">Vehículos detenidos</h2><p>Consulta la batería o recárgala al 100 %.</p></div>
          </header>
          <VehicleTable {...tableActions} rows={stopped} running={false} />
        </section>
      </>}
    </main>
    {dialog?.kind === 'trip' && <TripDialog options={tripOptions} onClose={() => setDialog(null)} onCreated={() => complete('Viaje iniciado.')} />}
    {dialog && target && (dialog.kind === 'speed' || dialog.kind === 'mode') &&
      <DrivingDialog kind={dialog.kind} id={target.id} vehicle={target.name}
        speed={target.speed ?? 0} mode={target.mode ?? 'Normal'} onClose={() => setDialog(null)}
        onUpdated={(state, message) => { receive(state); complete(message) }} />}
    {dialog && target && (dialog.kind === 'finish' || dialog.kind === 'recharge') &&
      <Popup title={dialog.kind === 'finish' ? 'Finalizar viaje' : 'Recargar batería'} busy={busy} onClose={() => setDialog(null)}>
        <p className="muted">{dialog.kind === 'finish'
          ? `El viaje de ${target.name} terminará y su motor se detendrá.`
          : `Los motores de ${target.name} se recargarán al 100 %.`}</p>
        {actionError && <p className="form-error" role="alert">{actionError}</p>}
        <footer><Button disabled={busy} onClick={() => setDialog(null)}>Cancelar</Button>
          <Button variant="primary" disabled={busy} onClick={() => void confirm()}>{busy ? 'Procesando…' : 'Confirmar'}</Button></footer>
      </Popup>}
    {dialog && dialog.kind !== 'trip' && !target &&
      <Popup title="Estado actualizado" busy={false} onClose={() => setDialog(null)}>
        <p className="muted">El vehículo cambió de estado. Consulta la tabla para ver las acciones disponibles.</p>
      </Popup>}
  </>
}
