using Microsoft.EntityFrameworkCore;
using WorkingCore.Domain.Models;

namespace WorkingCore.Infrastructure.DataBase;

public interface IAppDbContext
{
    public DbSet<TaskItem> TaskItems { get; }

    public Task SaveChangesAsync(CancellationToken cancel);
}
