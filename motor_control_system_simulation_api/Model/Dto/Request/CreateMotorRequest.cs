namespace MotorControlSystemSimulationApi.Services;

public sealed record CreateMotorRequest(
    string Model,
    decimal MaximumSpeed,
    decimal MaximumTemperature,
    decimal BatteryCapacityKwh,
    decimal MaximumPower);
