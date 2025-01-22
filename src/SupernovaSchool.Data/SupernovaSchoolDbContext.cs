using Microsoft.EntityFrameworkCore;
using SupernovaSchool.Models;

namespace SupernovaSchool.Data;

public class SupernovaSchoolDbContext(DbContextOptions<SupernovaSchoolDbContext> dbContextOptions)
    : DbContext(dbContextOptions)
{
    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupernovaSchoolDbContext).Assembly);
    }
}