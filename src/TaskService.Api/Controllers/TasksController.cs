using CoreLib.DTOs;
using CoreLib.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TaskService.Api.Controllers;

[ApiController]
[Route("api/v1/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        var task = await _taskService.CreateTaskAsync(dto);
        return CreatedAtAction(nameof(GetTask), new { taskId = task.Id }, task);
    }

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetTask(int taskId)
    {
        var task = await _taskService.GetTaskByIdAsync(taskId);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] int? projectId, [FromQuery] int? assigneeId,
        [FromQuery] string? status, [FromQuery] string? tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var tasks = await _taskService.GetTasksAsync(projectId, assigneeId, status, tag, page, pageSize);
        return Ok(tasks);
    }

    [HttpPatch("{taskId}")]
    public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateTaskDto dto)
    {
        var task = await _taskService.UpdateTaskAsync(taskId, dto);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost("{taskId}/status")]
    public async Task<IActionResult> ChangeStatus(int taskId, [FromBody] ChangeStatusDto dto)
    {
        var task = await _taskService.ChangeStatusAsync(taskId, dto);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        var success = await _taskService.DeleteTaskAsync(taskId);
        if (!success) return NotFound();
        return NoContent();
    }
}
