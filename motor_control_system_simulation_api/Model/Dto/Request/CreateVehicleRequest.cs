namespace MotorControlSystemSimulationApi.Services;

public sealed record CreateVehicleRequest(
    string Identifier,
    string Color,
    decimal MaximumWeight,
    int MaximumOccupants);
