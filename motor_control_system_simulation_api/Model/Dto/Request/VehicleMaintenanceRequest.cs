namespace MotorControlSystemSimulationApi.Services;

/// <summary>Porcentaje final de carga que se desea dejar en las baterías del vehículo.</summary>
public sealed record VehicleMaintenanceRequest(decimal TargetBatteryLevel = 100m);
