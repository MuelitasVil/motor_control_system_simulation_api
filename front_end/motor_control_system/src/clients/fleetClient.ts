import { requestJson } from './httpClient'
import type {
  AssociatedMotor, CreateTripRequest, CrewMember, DrivingMode, Motor, MotorState, Trip, Vehicle,
} from '../modules/dashboard/types'

const post = <T>(path: string, body?: object) =>
  requestJson<T>(path, { method: 'POST', body: body && JSON.stringify(body) })

export const fleetClient = {
  async getFleet(signal?: AbortSignal) {
    const [vehicles, motors, associations, trips] = await Promise.all([
      requestJson<Vehicle[]>('/vehicles', { signal }),
      requestJson<Motor[]>('/motors', { signal }),
      requestJson<AssociatedMotor[]>('/associated-motors', { signal }),
      requestJson<Trip[]>('/trips', { signal }),
    ])
    return { vehicles, motors, associations, trips }
  },
  getCrew: (signal: AbortSignal) => requestJson<CrewMember[]>('/crew-members', { signal }),
  createTrip: (data: CreateTripRequest) => post<Trip>('/trips', data),
  finishTrip: (id: string) => post<Trip>(`/trips/${id}/finish`),
  updateSpeed: (id: string, requestedSpeed: number, drivingMode: DrivingMode) =>
    post<MotorState>(`/associated-motors/${id}/speed`, { requestedSpeed, drivingMode }),
  recharge: (vehicleId: string) =>
    post<unknown>(`/vehicles/${vehicleId}/maintenance`, { targetBatteryLevel: 100 }),
}
