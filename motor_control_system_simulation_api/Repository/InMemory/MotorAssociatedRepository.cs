using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

public sealed class MotorAssociatedRepository : InMemoryRepositoryBase<MotorAssociated>, IEntityRepository<MotorAssociated>
{
    public MotorAssociatedRepository(InMemoryDataStore store)
        : base(store.AssociatedMotors, x => x.AssociatedMotorId, x => x.VehicleId, x => x.InstallationDate)
    {
    }
}

