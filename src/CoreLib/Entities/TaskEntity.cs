namespace CoreLib.Entities

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ProjectId { get; set; }
    public int? AssigneeId { get; set; }
    public int ReporterId { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}