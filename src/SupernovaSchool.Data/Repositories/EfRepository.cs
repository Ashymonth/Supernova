using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using SupernovaSchool.Abstractions.Repositories;

namespace SupernovaSchool.Data.Repositories;

public class EfRepository<TEntity> : RepositoryBase<TEntity>, IRepository<TEntity> where TEntity : class
{
    private readonly SupernovaSchoolDbContext _dbContext;

    public EfRepository(SupernovaSchoolDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public EfRepository(SupernovaSchoolDbContext dbContext, ISpecificationEvaluator specificationEvaluator) : base(
        dbContext,
        specificationEvaluator)
    {
        _dbContext = dbContext;
    }

    public IUnitOfWork UnitOfWork => new UnitOfWork(_dbContext);

    public new async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await _dbContext.AddAsync(entity, ct);
    }

    public async Task AddAsync(IEnumerable<TEntity> entity, CancellationToken ct = default)
    {
        await _dbContext.AddRangeAsync(entity, ct);
    }

    public void Update(TEntity entity)
    {
        _dbContext.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        _dbContext.Remove(entity);
    }
}
