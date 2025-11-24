using PolicyActionService.Models;

namespace PolicyActionService.Services;

public interface IPolicyService
{
    Task<IEnumerable<Policy>> GetAllPoliciesAsync();
    Task<Policy?> GetPolicyByIdAsync(int id);
    Task<Policy> CreatePolicyAsync(Policy policy);
    Task<Policy?> UpdatePolicyAsync(int id, Policy policy);
    Task<bool> DeletePolicyAsync(int id);
}
