namespace MotorControlSystemSimulationApi.Models.Entities;

public class Motor
{
    public Guid MotorId { get; set; } = Guid.NewGuid();
    public string Model { get; set; } = string.Empty;
    public decimal MaximumSpeed { get; set; }
    public decimal MaximumTemperature { get; set; }
    public decimal BatteryCapacityKwh { get; set; }
    public decimal MaximumPower { get; set; }

    public ICollection<MotorAssociated> AssociatedMotors { get; set; }
        = new List<MotorAssociated>();
}