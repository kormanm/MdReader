using PolicyActionService.Models;

namespace PolicyActionService.Services;

public class PolicyService : IPolicyService
{
    private readonly List<Policy> _policies = new();
    private int _nextId = 1;

    public Task<IEnumerable<Policy>> GetAllPoliciesAsync()
    {
        return Task.FromResult<IEnumerable<Policy>>(_policies);
    }

    public Task<Policy?> GetPolicyByIdAsync(int id)
    {
        var policy = _policies.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(policy);
    }

    public Task<Policy> CreatePolicyAsync(Policy policy)
    {
        policy.Id = _nextId++;
        policy.CreatedAt = DateTime.UtcNow;
        _policies.Add(policy);
        return Task.FromResult(policy);
    }

    public Task<Policy?> UpdatePolicyAsync(int id, Policy policy)
    {
        var existingPolicy = _policies.FirstOrDefault(p => p.Id == id);
        if (existingPolicy == null)
            return Task.FromResult<Policy?>(null);

        existingPolicy.Name = policy.Name;
        existingPolicy.Description = policy.Description;
        existingPolicy.IsActive = policy.IsActive;
        existingPolicy.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult<Policy?>(existingPolicy);
    }

    public Task<bool> DeletePolicyAsync(int id)
    {
        var policy = _policies.FirstOrDefault(p => p.Id == id);
        if (policy == null)
            return Task.FromResult(false);

        _policies.Remove(policy);
        return Task.FromResult(true);
    }
}
