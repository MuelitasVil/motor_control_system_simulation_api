using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

public static class InMemoryRepositoryRegistration
{
    public static IServiceCollection AddInMemoryRepositories(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryDataStore>();
        services.AddSingleton<VehicleRepository>();
        services.AddSingleton<MotorRepository>();
        services.AddSingleton<MotorAssociatedRepository>();
        services.AddSingleton<TripRepository>();
        services.AddSingleton<MotorStateHistoryRepository>();
        services.AddSingleton<CommandRepository>();
        services.AddSingleton<CrewMemberRepository>();
        services.AddSingleton<TripHistoryRepository>();
        services.AddSingleton<TripCrewMemberRepository>();

        services.AddSingleton<IEntityRepository<Vehicle>>(sp => sp.GetRequiredService<VehicleRepository>());
        services.AddSingleton<IEntityRepository<Motor>>(sp => sp.GetRequiredService<MotorRepository>());
        services.AddSingleton<IEntityRepository<MotorAssociated>>(sp => sp.GetRequiredService<MotorAssociatedRepository>());
        services.AddSingleton<IEntityRepository<Trip>>(sp => sp.GetRequiredService<TripRepository>());
        services.AddSingleton<IEntityRepository<MotorStateHistory>>(sp => sp.GetRequiredService<MotorStateHistoryRepository>());
        services.AddSingleton<IEntityRepository<Command>>(sp => sp.GetRequiredService<CommandRepository>());
        services.AddSingleton<IEntityRepository<CrewMember>>(sp => sp.GetRequiredService<CrewMemberRepository>());
        services.AddSingleton<IEntityRepository<TripHistory>>(sp => sp.GetRequiredService<TripHistoryRepository>());

        return services;
    }
}
