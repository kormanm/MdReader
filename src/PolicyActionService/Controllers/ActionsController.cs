using Microsoft.AspNetCore.Mvc;
using PolicyActionService.Models;
using PolicyActionService.Services;

namespace PolicyActionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActionsController : ControllerBase
{
    private readonly IActionService _actionService;
    private readonly ILogger<ActionsController> _logger;

    public ActionsController(IActionService actionService, ILogger<ActionsController> logger)
    {
        _actionService = actionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PolicyAction>>> GetAll()
    {
        _logger.LogInformation("Getting all actions");
        var actions = await _actionService.GetAllActionsAsync();
        return Ok(actions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PolicyAction>> GetById(int id)
    {
        _logger.LogInformation("Getting action with id {Id}", id);
        var action = await _actionService.GetActionByIdAsync(id);
        if (action == null)
        {
            _logger.LogWarning("Action with id {Id} not found", id);
            return NotFound();
        }
        return Ok(action);
    }

    [HttpGet("policy/{policyId}")]
    public async Task<ActionResult<IEnumerable<PolicyAction>>> GetByPolicyId(int policyId)
    {
        _logger.LogInformation("Getting actions for policy {PolicyId}", policyId);
        var actions = await _actionService.GetActionsByPolicyIdAsync(policyId);
        return Ok(actions);
    }

    [HttpPost("execute")]
    public async Task<ActionResult<PolicyAction>> Execute([FromBody] PolicyAction action)
    {
        _logger.LogInformation("Executing action type {ActionType} for policy {PolicyId}", action.ActionType, action.PolicyId);
        var executedAction = await _actionService.ExecuteActionAsync(action);
        return CreatedAtAction(nameof(GetById), new { id = executedAction.Id }, executedAction);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<PolicyAction>> UpdateStatus(int id, [FromBody] ActionStatusUpdate statusUpdate)
    {
        _logger.LogInformation("Updating status for action {Id} to {Status}", id, statusUpdate.Status);
        var updatedAction = await _actionService.UpdateActionStatusAsync(id, statusUpdate.Status, statusUpdate.Result);
        if (updatedAction == null)
        {
            _logger.LogWarning("Action with id {Id} not found for status update", id);
            return NotFound();
        }
        return Ok(updatedAction);
    }
}

public record ActionStatusUpdate(string Status, string? Result);
