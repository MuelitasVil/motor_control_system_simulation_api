using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

/// <summary>Implementación común en memoria. Los repositorios concretos configuran sus claves y relaciones.</summary>
public abstract class InMemoryRepositoryBase<T> : IEntityRepository<T> where T : class
{
    private readonly IList<T> _items;
    private readonly Func<T, Guid> _id;
    private readonly Func<T, Guid> _ownerId;
    private readonly Func<T, DateTimeOffset> _timestamp;
    private readonly object _sync = new();

    protected InMemoryRepositoryBase(
        IList<T> items,
        Func<T, Guid> id,
        Func<T, Guid> ownerId,
        Func<T, DateTimeOffset> timestamp)
    {
        _items = items;
        _id = id;
        _ownerId = ownerId;
        _timestamp = timestamp;
    }

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync) return Task.FromResult<IReadOnlyList<T>>(_items.ToList());
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync) return Task.FromResult(_items.FirstOrDefault(x => _id(x) == id));
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (_items.Any(x => _id(x) == _id(entity)))
                throw new InvalidOperationException($"Ya existe un registro de {typeof(T).Name} con ese ID.");
            _items.Add(entity);
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var index = -1;
            for (var i = 0; i < _items.Count; i++)
                if (_id(_items[i]) == _id(entity)) { index = i; break; }
            if (index < 0) throw new KeyNotFoundException($"No existe un registro de {typeof(T).Name} con ese ID.");
            _items[index] = entity;
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<T>> GetLatestAsync(Guid ownerId, int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "count debe ser mayor que cero.");
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            IReadOnlyList<T> result = _items
                .Where(x => _ownerId(x) == ownerId)
                .OrderByDescending(_timestamp)
                .Take(count)
                .ToList();
            return Task.FromResult(result);
        }
    }
}
