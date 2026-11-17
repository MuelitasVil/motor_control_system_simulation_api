namespace MotorControlSystemSimulationApi.Models.Entities;

public class Vehicle
{
    public Guid VehicleId { get; set; } = Guid.NewGuid();
    public string Identifier { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal MaximumWeight { get; set; }
    public int MaximumOccupants { get; set; }

    public ICollection<MotorAssociated> AssociatedMotors { get; set; }
        = new List<MotorAssociated>();
}
