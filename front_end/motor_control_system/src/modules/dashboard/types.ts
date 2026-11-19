export const drivingModes = ['Eco', 'Normal', 'Sport'] as const
export type DrivingMode = typeof drivingModes[number]

export interface Vehicle {
  vehicleId: string
  identifier: string
  color: string
}

export interface Motor {
  motorId: string
  model: string
}

export interface AssociatedMotor {
  associatedMotorId: string
  vehicleId: string
  motorId: string
  operationState: 'Stopped' | 'InTrip'
  currentSpeed: number
  currentTemperature: number
  currentRpm: number
  currentPower: number
  batteryLevel: number
  drivingMode: DrivingMode
}

export interface Trip {
  tripId: string
  associatedMotorId: string
  startPoint: string
  endPoint: string
  endDate: string | null
}

export interface CrewMember {
  crewMemberId: string
  name: string
}

export interface CreateTripRequest {
  associatedMotorId: string
  startPoint: string
  endPoint: string
  totalDistance: number
  cargoWeight: number
  crewMemberIds: string[]
}

export interface MotorState {
  associatedMotorId: string
  requestedSpeed: number
  appliedSpeed: number
  speedLimit: number
  temperature: number
  rpm: number
  power: number
  batteryLevel: number
  drivingMode: DrivingMode
  updatedAt: string
}
