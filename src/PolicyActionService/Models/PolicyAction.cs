namespace PolicyActionService.Models;

public class PolicyAction
{
    public int Id { get; set; }
    public int PolicyId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ActionData { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Result { get; set; }
}
