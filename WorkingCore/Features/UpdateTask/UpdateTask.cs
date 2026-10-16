using Microsoft.EntityFrameworkCore;
using WorkingCore.Infrastructure.DataBase;
using WorkingCore.Infrastructure.RabbitMq;

namespace WorkingCore.Features.UpdateTask;

public static class UpdateTask
{
    public static void MapPutTask(this IEndpointRouteBuilder app)
    {
        app.MapPut("/tasks/{id:guid}/complete", async (
            Guid id, 
            IAppDbContext db, 
            ITaskEventPublisher publisher, 
            CancellationToken ct) =>
        {
            var task = await db.TaskItems.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (task == null)
            {
                return Results.NotFound();
            }

            task.IsCompleted = true;
            task.CompletedAt = DateTimeOffset.UtcNow;

            try
            {
                await db.SaveChangesAsync(ct);

                publisher.Publish(new TaskCompletedEvent(
                                      task.Id,
                                      task.Title,
                                      task.CompletedAt!.Value,
                                      task.Priority.ToString()
                                  ));

                return Results.Ok(task);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Results.Conflict("Задача уже была изменена другим пользователем.");
            }
            
        });
    }
}
