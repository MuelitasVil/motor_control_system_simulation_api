using MotorControlSystemSimulationApi.Models.Enums;

namespace MotorControlSystemSimulationApi.Services;

/// <summary>Datos que el cliente envía para cambiar la velocidad simulada.</summary>
public sealed record UpdateSpeedRequest(decimal RequestedSpeed, DrivingMode DrivingMode = DrivingMode.Normal);
