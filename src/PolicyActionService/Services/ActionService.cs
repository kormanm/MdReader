using PolicyActionService.Models;

namespace PolicyActionService.Services;

public class ActionService : IActionService
{
    private readonly List<PolicyAction> _actions = new();
    private int _nextId = 1;

    public Task<IEnumerable<PolicyAction>> GetAllActionsAsync()
    {
        return Task.FromResult<IEnumerable<PolicyAction>>(_actions);
    }

    public Task<PolicyAction?> GetActionByIdAsync(int id)
    {
        var action = _actions.FirstOrDefault(a => a.Id == id);
        return Task.FromResult(action);
    }

    public Task<IEnumerable<PolicyAction>> GetActionsByPolicyIdAsync(int policyId)
    {
        var actions = _actions.Where(a => a.PolicyId == policyId);
        return Task.FromResult<IEnumerable<PolicyAction>>(actions);
    }

    public Task<PolicyAction> ExecuteActionAsync(PolicyAction action)
    {
        action.Id = _nextId++;
        action.ExecutedAt = DateTime.UtcNow;
        action.Status = "Executed";
        action.Result = $"Action {action.ActionType} executed successfully";
        _actions.Add(action);
        return Task.FromResult(action);
    }

    public Task<PolicyAction?> UpdateActionStatusAsync(int id, string status, string? result)
    {
        var action = _actions.FirstOrDefault(a => a.Id == id);
        if (action == null)
            return Task.FromResult<PolicyAction?>(null);

        action.Status = status;
        action.Result = result;

        return Task.FromResult<PolicyAction?>(action);
    }
}
