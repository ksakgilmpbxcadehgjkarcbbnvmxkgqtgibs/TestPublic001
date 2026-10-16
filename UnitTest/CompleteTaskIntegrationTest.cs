using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using WorkingCore.Domain.Models;
using WorkingCore.Features.CreateTask;
using WorkingCore.Infrastructure.RabbitMq;

namespace UnitTest;

public class CompleteTaskIntegrationTest : IClassFixture<WebApplicationFactory<WorkingCore.Program>>
{
    private readonly WebApplicationFactory<WorkingCore.Program> _factory;

    public CompleteTaskIntegrationTest(WebApplicationFactory<WorkingCore.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompleteTask_ShouldUpdateTask_AndPublishRabbitMqEvent()
    {
        TaskCompletedEvent? publishedEvent = null;

        var mockPublisher = new Mock<ITaskEventPublisher>();
        mockPublisher
            .Setup(p => p.Publish(It.IsAny<TaskCompletedEvent>()))
            .Callback<TaskCompletedEvent>(e => publishedEvent = e);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(mockPublisher.Object);
            });
        }).CreateClient();

        var createRequest = new CreateTaskRequest("Buy coffee", Priority.High);
        var postResponse = await client.PostAsJsonAsync("/tasks", createRequest);

        postResponse.EnsureSuccessStatusCode();
        var createdTask = await postResponse.Content.ReadFromJsonAsync<TaskItem>();

        createdTask.ShouldNotBeNull();

        var putResponse = await client.PutAsync($"/tasks/{createdTask.Id}/complete", null);

        putResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        publishedEvent.ShouldNotBeNull();
        publishedEvent.TaskId.ShouldBe(createdTask.Id);
        publishedEvent.Title.ShouldBe("Buy coffee");
        publishedEvent.Priority.ShouldBe("High");
    }
}
