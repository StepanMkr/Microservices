namespace CoreLib.Entities;

public class TaskStatusLog
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int ChangedBy { get; set; }
    public TaskStatus OldStatus { get; set; }
    public TaskStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Comment { get; set; }
}