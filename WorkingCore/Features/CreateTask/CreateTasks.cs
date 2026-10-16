using WorkingCore.Domain.Models;
using WorkingCore.Infrastructure.DataBase;

namespace WorkingCore.Features.CreateTask;

public record CreateTaskRequest(string Title, Priority Priority = Priority.Medium);

public static class CreateTasks
{
    public static void MapPostTask(this IEndpointRouteBuilder app)
    {
        app.MapPost("/tasks", async (CreateTaskRequest request, IAppDbContext db, CancellationToken ct) =>
        {

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest("Title cannot be empty.");
            }

            if (request.Title.Length > 200)
            {
                return Results.BadRequest("Title must be 200 characters or fewer.");
            }


            var taskItem = new TaskItem()
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                CreatedAt = DateTimeOffset.UtcNow,
 
                IsCompleted = false,
                Priority = request.Priority
            };

            db.TaskItems.Add(taskItem);
            await db.SaveChangesAsync(ct);

            return Results.Ok(taskItem);
        });
    }
}
