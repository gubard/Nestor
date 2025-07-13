using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nestor.ServerExample;
using Nestor.Shared;
using StreamJsonRpc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ExampleDbContext>(options => options.UseSqlite("Data Source=example.db"));
builder.Services.AddTransient<IExampleService, ExampleService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();
app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws-jsonrpc")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var calculatorService = context.RequestServices.GetRequiredService<IExampleService>();
            await using var handler = new WebSocketMessageHandler(webSocket);
            using var jsonRpc = new JsonRpc(handler, calculatorService);
            jsonRpc.StartListening();
            await jsonRpc.Completion;
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }
});

app.Run();