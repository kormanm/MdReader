using PolicyActionService.Models;

namespace PolicyActionService.Services;

public interface IActionService
{
    Task<IEnumerable<PolicyAction>> GetAllActionsAsync();
    Task<PolicyAction?> GetActionByIdAsync(int id);
    Task<IEnumerable<PolicyAction>> GetActionsByPolicyIdAsync(int policyId);
    Task<PolicyAction> ExecuteActionAsync(PolicyAction action);
    Task<PolicyAction?> UpdateActionStatusAsync(int id, string status, string? result);
}
