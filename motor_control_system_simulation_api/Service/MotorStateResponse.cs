using MotorControlSystemSimulationApi.Models.Enums;

namespace MotorControlSystemSimulationApi.Services;

/// <summary>Estado calculado que devuelve la API después de procesar la orden.</summary>
public sealed record MotorStateResponse(
    Guid AssociatedMotorId,
    decimal RequestedSpeed,
    decimal AppliedSpeed,
    decimal SpeedLimit,
    decimal Temperature,
    int Rpm,
    decimal Power,
    decimal BatteryLevel,
    DrivingMode DrivingMode,
    DateTimeOffset UpdatedAt);
