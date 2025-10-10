using Microsoft.EntityFrameworkCore;
using TaskService.Infrastructure;
using TaskService.Infrastructure.Persistence;
using TaskService.Dal;
using CoreLib.Interfaces;
using TaskService.Logic.Services;
using TaskService.Application.Interfaces;
using TaskService.Application.Services;

var builder = WebApplication.CreateBuilder(args);

//TaskDbContext
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskDb")));

//ProjectDbContext
builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProjectDb")));

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

app.UseHttpsRedirection();

app.MapControllers();

app.Run();