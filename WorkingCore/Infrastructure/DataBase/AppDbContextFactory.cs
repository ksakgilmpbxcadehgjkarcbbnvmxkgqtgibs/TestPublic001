using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorkingCore.Infrastructure.DataBase;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionBuilder.UseNpgsql("Host=localhost;Port=5432;Database=payment_db;Username=postgres;Password=password");

        return new AppDbContext(optionBuilder.Options);
    }
}
