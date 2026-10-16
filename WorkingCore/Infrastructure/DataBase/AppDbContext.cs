using Microsoft.EntityFrameworkCore;
using WorkingCore.Domain.Models;

namespace WorkingCore.Infrastructure.DataBase;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions db) : base(db)
    {

    }

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<TaskItem>()
        .Property<uint>("xmin")       
        .HasColumnType("xid")         
        .ValueGeneratedOnAddOrUpdate()
        .IsConcurrencyToken();
    }

    Task IAppDbContext.SaveChangesAsync(CancellationToken cancel)
    {
        return SaveChangesAsync(cancel);
    }
}
