using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

/// <summary>Almacén compartido que simula las tablas de la base de datos mientras la API está activa.</summary>
public sealed class InMemoryDataStore
{
    public static readonly Guid VehicleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid MotorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AssociatedMotorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid TripId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid VehicleId2 = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid VehicleId3 = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid MotorId2 = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid MotorId3 = Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static readonly Guid AssociatedMotorId2 = Guid.Parse("99999999-9999-9999-9999-999999999999");

    public List<Vehicle> Vehicles { get; } = [];
    public List<Motor> Motors { get; } = [];
    public List<MotorAssociated> AssociatedMotors { get; } = [];
    public List<Trip> Trips { get; } = [];
    public List<MotorStateHistory> MotorStateHistory { get; } = [];
    public List<Command> Commands { get; } = [];
    public List<CrewMember> CrewMembers { get; } = [];
    public List<TripCrewMember> TripCrewMembers { get; } = [];
    public List<TripHistory> TripHistory { get; } = [];

    public InMemoryDataStore()
    {
        var vehicle = new Vehicle
        {
            VehicleId = VehicleId,
            Identifier = "EV-001",
            Color = "Azul",
            MaximumWeight = 1800m,
            MaximumOccupants = 5
        };
        var secondVehicle = new Vehicle
        {
            VehicleId = VehicleId2,
            Identifier = "EV-002",
            Color = "Blanco",
            MaximumWeight = 1600m,
            MaximumOccupants = 5
        };
        var thirdVehicle = new Vehicle
        {
            VehicleId = VehicleId3,
            Identifier = "EV-003",
            Color = "Rojo",
            MaximumWeight = 2000m,
            MaximumOccupants = 7
        };

        var motor = new Motor
        {
            MotorId = MotorId,
            Model = "Electric-X",
            MaximumSpeed = 180m,
            MaximumTemperature = 150m,
            BatteryCapacityKwh = 75m,
            MaximumPower = 220m
        };
        var secondMotor = new Motor
        {
            MotorId = MotorId2,
            Model = "City-Electric",
            MaximumSpeed = 120m,
            MaximumTemperature = 135m,
            BatteryCapacityKwh = 55m,
            MaximumPower = 160m
        };
        // Este motor queda registrado sin instalarse en un vehículo.
        var uninstalledMotor = new Motor
        {
            MotorId = MotorId3,
            Model = "Spare-Electric",
            MaximumSpeed = 160m,
            MaximumTemperature = 140m,
            BatteryCapacityKwh = 65m,
            MaximumPower = 190m
        };

        var associated = new MotorAssociated
        {
            AssociatedMotorId = AssociatedMotorId, VehicleId = VehicleId, Vehicle = vehicle,
            MotorId = MotorId, Motor = motor, InstallationDate = DateTimeOffset.UtcNow.AddMonths(-6),
            OperatingStatus = "Active", CurrentSpeed = 40m, BatteryLevel = 85m,
            CurrentTemperature = 70m, CurrentRpm = 1777, CurrentPower = 48.89m,
            DrivingMode = DrivingMode.Normal, OperationState = MotorOperationState.InTrip
        };
        var secondAssociated = new MotorAssociated
        {
            AssociatedMotorId = AssociatedMotorId2,
            VehicleId = VehicleId2,
            Vehicle = secondVehicle,
            MotorId = MotorId2,
            Motor = secondMotor,
            InstallationDate = DateTimeOffset.UtcNow.AddMonths(-2),
            OperatingStatus = "Active",
            CurrentSpeed = 0m,
            BatteryLevel = 100m,
            CurrentTemperature = 25m,
            CurrentRpm = 0,
            CurrentPower = 0m,
            DrivingMode = DrivingMode.Eco,
            OperationState = MotorOperationState.Stopped
        };
        vehicle.AssociatedMotors.Add(associated);
        motor.AssociatedMotors.Add(associated);
        secondVehicle.AssociatedMotors.Add(secondAssociated);
        secondMotor.AssociatedMotors.Add(secondAssociated);
        Vehicles.Add(vehicle);
        Vehicles.Add(secondVehicle);
        Vehicles.Add(thirdVehicle);
        Motors.Add(motor);
        Motors.Add(secondMotor);
        Motors.Add(uninstalledMotor);
        AssociatedMotors.Add(associated);
        AssociatedMotors.Add(secondAssociated);

        var trip = new Trip
        {
            TripId = TripId, AssociatedMotorId = AssociatedMotorId, AssociatedMotor = associated,
            StartPoint = "Bogotá", EndPoint = "Medellín", TotalDistance = 415m,
            CurrentDistance = 0m, CargoWeight = 120m, StartDate = DateTimeOffset.UtcNow
        };
        Trips.Add(trip);
        associated.Trips.Add(trip);

        var now = DateTime.UtcNow;
        MotorStateHistory.AddRange(new MotorStateHistory[] {
            new() { MotorStateId = Guid.NewGuid(), AssociatedMotorId = AssociatedMotorId, Timestamp = now.AddMinutes(-2), Speed = 35m, Temperature = 68m, Rpm = 1550, Power = 42.78m, BatteryLevel = 86m },
            new() { MotorStateId = Guid.NewGuid(), AssociatedMotorId = AssociatedMotorId, Timestamp = now.AddMinutes(-1), Speed = 40m, Temperature = 70m, Rpm = 1777, Power = 48.89m, BatteryLevel = 85.5m }
        });
        foreach (var state in MotorStateHistory)
        {
            associated.StateHistory.Add(state);
        }

        Commands.Add(new Command
        {
            AssociatedMotorId = AssociatedMotorId, AssociatedMotor = associated,
            Type = CommandType.IncreaseSpeed, Parameters = "speed=40", Result = "Initial seeded state",
            SentAt = DateTimeOffset.UtcNow.AddMinutes(-1), ProcessedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        });

        var crew = new CrewMember { Name = "Ana Pérez", Phone = "3000000001", EmergencyPhone = "3000000002", Weight = 60m };
        CrewMembers.Add(crew);
        var tripCrew = new TripCrewMember { TripId = TripId, Trip = trip, CrewMemberId = crew.CrewMemberId, CrewMember = crew };
        TripCrewMembers.Add(tripCrew);
        trip.CrewMembers.Add(tripCrew);
        crew.Trips.Add(tripCrew);

        TripHistory.Add(new TripHistory { TripId = TripId, Trip = trip, Status = "Created", Timestamp = DateTimeOffset.UtcNow, Details = "Viaje de prueba inicial." });
    }
}
