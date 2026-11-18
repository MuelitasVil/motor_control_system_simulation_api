namespace MotorControlSystemSimulationApi.Services;

/// <summary>CrewMemberIds es opcional; una lista vacía crea el viaje sin tripulantes.</summary>
public sealed record CreateTripRequest(
    Guid AssociatedMotorId,
    string StartPoint,
    string EndPoint,
    decimal TotalDistance,
    decimal CargoWeight,
    IReadOnlyList<Guid>? CrewMemberIds = null);
