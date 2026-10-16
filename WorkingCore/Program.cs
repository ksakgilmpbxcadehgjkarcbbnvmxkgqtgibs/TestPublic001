using Microsoft.EntityFrameworkCore;
using WorkingCore.Features.CreateTask;
using WorkingCore.Features.DeleteTask;
using WorkingCore.Features.GetTasks;
using WorkingCore.Features.UpdateTask;
using WorkingCore.Infrastructure.DataBase;
using WorkingCore.Infrastructure.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(option =>
option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

builder.Services.AddSingleton<RabbitMqTaskPublisher>();
builder.Services.AddSingleton<ITaskEventPublisher>(sp => sp.GetRequiredService<RabbitMqTaskPublisher>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<RabbitMqTaskPublisher>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    context.Database.Migrate();
}

app.MapGetTasks();

app.MapPostTask();

app.MapPutTask();

app.MapDeleteTask();

app.Run();

namespace WorkingCore
{
    public partial class Program { }
}