namespace MotorControlSystemSimulationApi.Services;

public sealed record CreateMotorAssociationRequest(Guid VehicleId, Guid MotorId);
