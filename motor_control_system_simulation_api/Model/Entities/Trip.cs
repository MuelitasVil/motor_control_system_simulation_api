namespace MotorControlSystemSimulationApi.Models.Entities;

public class Trip
{
    public Guid TripId { get; set; } = Guid.NewGuid();

    public Guid AssociatedMotorId { get; set; }
    public MotorAssociated? AssociatedMotor { get; set; }

    public string StartPoint { get; set; } = string.Empty;
    public string EndPoint { get; set; } = string.Empty;
    public decimal TotalDistance { get; set; }
    public decimal CurrentDistance { get; set; }
    public decimal CargoWeight { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    public ICollection<TripHistory> History { get; set; }
        = new List<TripHistory>();

    public ICollection<TripCrewMember> CrewMembers { get; set; }
        = new List<TripCrewMember>();
}