namespace CoreLib.DTOs;

public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; }  // вместо string можно Enum TaskPriority
}