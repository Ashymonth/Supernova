using Ardalis.Specification;

namespace SupernovaSchool.Abstractions.Repositories;

public interface IRepository<TEntity> : IReadRepositoryBase<TEntity> where TEntity : class
{
    IUnitOfWork UnitOfWork { get; }

    Task AddAsync(TEntity entity, CancellationToken ct = default);

    Task AddAsync(IEnumerable<TEntity> entity, CancellationToken ct = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}