using Microsoft.EntityFrameworkCore;
using WorkingCore.Infrastructure.DataBase;

namespace WorkingCore.Features.GetTasks;

public static class GetTasks
{
    public static void MapGetTasks(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks", async (IAppDbContext db, CancellationToken ct) =>
        {
            var tasks = await db.TaskItems.AsNoTracking().ToListAsync(ct);
            return Results.Ok(tasks);
        });
    }

}
