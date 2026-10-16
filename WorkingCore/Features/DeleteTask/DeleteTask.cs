using Microsoft.EntityFrameworkCore;
using WorkingCore.Infrastructure.DataBase;

namespace WorkingCore.Features.DeleteTask;

public static class DeleteTask
{
    public static void MapDeleteTask(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/tasks/{id}", async (Guid id, IAppDbContext db, CancellationToken ct) =>
        {
            var row = await db.TaskItems.Where(x=>x.Id == id).ExecuteDeleteAsync();

            return row > 0 ? Results.Ok() : Results.NotFound(id);

        });
    }
}
