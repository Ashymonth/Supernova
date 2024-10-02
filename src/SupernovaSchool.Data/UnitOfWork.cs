using SupernovaSchool.Abstractions.Repositories;

namespace SupernovaSchool.Data;

public class UnitOfWork(SupernovaSchoolDbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}