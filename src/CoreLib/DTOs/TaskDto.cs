namespace CoreLib.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ProjectId { get; set; }
    public int? AssigneeId { get; set; }
    public int ReporterId { get; set; }
    public string Status { get; set; } = null!; // вместо string можно Enum TaskStatus
    public string Priority { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}