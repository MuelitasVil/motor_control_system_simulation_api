using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;


namespace MotorControlSystemSimulationApi.Models.Entities;

public class MotorAssociated
{
    public Guid AssociatedMotorId { get; set; } = Guid.NewGuid();

    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Guid MotorId { get; set; }
    public Motor? Motor { get; set; }

    public DateTimeOffset InstallationDate { get; set; }
    public string OperatingStatus { get; set; } = "Active";
    public MotorOperationState OperationState { get; set; } = MotorOperationState.Stopped;

    // Estado actual de esta instalación de motor
    public decimal CurrentSpeed { get; set; }
    public decimal BatteryLevel { get; set; }
    public decimal CurrentTemperature { get; set; }
    public int CurrentRpm { get; set; }
    public decimal CurrentPower { get; set; }
    public DrivingMode DrivingMode { get; set; } = DrivingMode.Normal;

    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public ICollection<MotorStateHistory> StateHistory { get; set; }
        = new List<MotorStateHistory>();
    public ICollection<Command> Commands { get; set; } = new List<Command>();
}
