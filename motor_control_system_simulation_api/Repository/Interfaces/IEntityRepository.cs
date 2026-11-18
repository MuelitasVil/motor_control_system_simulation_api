namespace MotorControlSystemSimulationApi.Repositories.Interfaces;

public interface IEntityRepository<T> where T : class
{
    Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        T entity,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetLatestAsync(
        Guid ownerId,
        int count,
        CancellationToken cancellationToken = default);
}