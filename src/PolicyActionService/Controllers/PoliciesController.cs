using Microsoft.AspNetCore.Mvc;
using PolicyActionService.Models;
using PolicyActionService.Services;

namespace PolicyActionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;
    private readonly ILogger<PoliciesController> _logger;

    public PoliciesController(IPolicyService policyService, ILogger<PoliciesController> logger)
    {
        _policyService = policyService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Policy>>> GetAll()
    {
        _logger.LogInformation("Getting all policies");
        var policies = await _policyService.GetAllPoliciesAsync();
        return Ok(policies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Policy>> GetById(int id)
    {
        _logger.LogInformation("Getting policy with id {Id}", id);
        var policy = await _policyService.GetPolicyByIdAsync(id);
        if (policy == null)
        {
            _logger.LogWarning("Policy with id {Id} not found", id);
            return NotFound();
        }
        return Ok(policy);
    }

    [HttpPost]
    public async Task<ActionResult<Policy>> Create([FromBody] Policy policy)
    {
        _logger.LogInformation("Creating new policy: {PolicyName}", policy.Name);
        var createdPolicy = await _policyService.CreatePolicyAsync(policy);
        return CreatedAtAction(nameof(GetById), new { id = createdPolicy.Id }, createdPolicy);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Policy>> Update(int id, [FromBody] Policy policy)
    {
        _logger.LogInformation("Updating policy with id {Id}", id);
        var updatedPolicy = await _policyService.UpdatePolicyAsync(id, policy);
        if (updatedPolicy == null)
        {
            _logger.LogWarning("Policy with id {Id} not found for update", id);
            return NotFound();
        }
        return Ok(updatedPolicy);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting policy with id {Id}", id);
        var deleted = await _policyService.DeletePolicyAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Policy with id {Id} not found for deletion", id);
            return NotFound();
        }
        return NoContent();
    }
}
