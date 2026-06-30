using PiddeCorp.TaskManager.API.Repositories;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<TaskRepository>();

// CORS for local dev - allows Vite dev server
builder.Services.AddCors(options => {
        options.AddPolicy("DevPolicy",
                          policy => policy.WithOrigins("http://localhost:5173")
                                          .AllowAnyMethod()
                                          .AllowAnyHeader()
        );
    }
);

WebApplication app = builder.Build();

app.UseCors("DevPolicy");

app.MapGet("/api/tasks", (TaskRepository repo) => repo.GetOpenTasks());

app.Run();
