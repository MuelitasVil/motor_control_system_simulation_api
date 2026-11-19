import { BatteryCharging, CircleStop, Gauge, SlidersHorizontal } from 'lucide-react'
import { Button } from '../../../shared/components/Button'
import { DataTable, type Column } from '../../../shared/components/DataTable'
import type { DrivingMode } from '../types'

export interface VehicleRow {
  id: string
  vehicleId: string
  name: string
  motors: string
  batteries: number[]
  tripId?: string
  route?: string
  speed?: number
  limit?: number
  temperature?: number
  rpm?: number
  power?: number
  mode?: DrivingMode
}

const format = (value: number) => value.toLocaleString('es-CO', { maximumFractionDigits: 2 })

export function VehicleTable({ rows, running, busy, onDrive, onFinish, onRecharge }: {
  rows: VehicleRow[]; running: boolean; busy: boolean
  onDrive: (row: VehicleRow, kind: 'speed' | 'mode') => void
  onFinish: (row: VehicleRow) => void
  onRecharge: (row: VehicleRow) => void
}) {
  const columns: Column<VehicleRow>[] = [
    { label: 'Vehículo', render: (row) => <div className="cell-main"><strong>{row.name}</strong><small>{row.motors || 'Sin motor asociado'}</small></div> },
    ...(running ? [
      { label: 'Trayecto', render: (row: VehicleRow) => row.route ?? '—' },
      { label: 'Velocidad', render: (row: VehicleRow) => <div className="cell-main">
        <strong>{format(row.speed ?? 0)} <span className="unit">km/h</span></strong>
        {row.limit !== undefined && <small>Límite {format(row.limit)} km/h</small>}
      </div> },
    ] : []),
    { label: 'Batería', render: (row) => row.batteries.length
      ? <div className="batteries">{row.batteries.map((level, index) => <div className="battery" key={index}>
        <span>{format(level)} %</span><progress aria-label={`Batería del motor ${index + 1}`} max="100" value={level} />
      </div>)}</div> : '—' },
    ...(running ? [
      { label: 'Temperatura', render: (row: VehicleRow) => `${format(row.temperature ?? 0)} °C` },
      { label: 'RPM', render: (row: VehicleRow) => format(row.rpm ?? 0) },
      { label: 'Potencia', render: (row: VehicleRow) => `${format(row.power ?? 0)} kW` },
      { label: 'Modo', render: (row: VehicleRow) => <span className="mode">{row.mode}</span> },
    ] : []),
    { label: 'Acciones', render: (row) => <div className="actions">
      {running ? <>
        <Button disabled={busy} onClick={() => onDrive(row, 'speed')}><Gauge size={15} /> Velocidad</Button>
        <Button disabled={busy} onClick={() => onDrive(row, 'mode')}><SlidersHorizontal size={15} /> Modo</Button>
        <Button disabled={busy || !row.tripId} onClick={() => onFinish(row)}><CircleStop size={15} /> Finalizar</Button>
      </> : <Button disabled={busy || row.batteries.length === 0} onClick={() => onRecharge(row)}>
        <BatteryCharging size={16} /> Recargar
      </Button>}
    </div> },
  ]

  return <DataTable label={running ? 'Vehículos en viaje' : 'Vehículos detenidos'}
    rows={rows} rowKey={(row) => row.id} columns={columns}
    empty={running ? 'No hay viajes en curso.' : 'No hay vehículos detenidos.'} />
}
