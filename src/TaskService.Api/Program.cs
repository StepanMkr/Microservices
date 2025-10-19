using Microsoft.EntityFrameworkCore;
using TaskService.Infrastructure;
using TaskService.Infrastructure.Persistence;
using TaskService.Dal;
using CoreLib.Interfaces;
using TaskService.Logic.Services;
using TaskService.Application.Interfaces;
using TaskService.Application.Services;
using Serilog;
using CoreLib.TraceId;

var builder = WebApplication.CreateBuilder(args);

// Log
Log.Logger = new LoggerConfiguration()
    .GetConfiguration()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.TryAddTraceId();
builder.Services.AddLoggerServices();
builder.Services.AddHttpClient("default").AddHttpMessageHandler<TraceIdHttpMessageHandler>();

//TaskDbContext
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskDb")));

//ProjectDbContext
builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProjectDb")));

builder.Services.AddHttpRequestService();

builder.Services.AddHttpClient("project-catalog", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["PROJECT_CATALOG_BASEURL"]!);
    c.Timeout = TimeSpan.FromSeconds(5);
}).AddHttpMessageHandler<TraceIdHttpMessageHandler>();

builder.Services.AddScoped<ITaskService, TaskService.Logic.Services.TaskService>();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TraceIdMiddleware>();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();