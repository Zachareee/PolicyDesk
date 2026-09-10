using Microsoft.AspNetCore.Mvc;
using PolicyDesk.Api.Models.Dtos;
using PolicyDesk.Api.Services;

namespace PolicyDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PoliciesController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedPolicyResult>> GetPolicies([FromQuery] PolicyListRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _policyService.GetPoliciesAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to load policies", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PolicyDetailDto>> GetPolicy(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _policyService.GetPolicyByIdAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to load policy", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public async Task<ActionResult<PolicyDetailDto>> CreatePolicy([FromBody] CreatePolicyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _policyService.CreatePolicyAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetPolicy), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to create policy", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PolicyDetailDto>> UpdatePolicy(Guid id, [FromBody] UpdatePolicyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _policyService.UpdatePolicyAsync(id, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to update policy", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePolicy(Guid id, [FromQuery] bool confirm = false, CancellationToken cancellationToken = default)
    {
        try
        {
            await _policyService.DeletePolicyAsync(id, confirm, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to delete policy", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("{id:guid}/actions/{actionName}")]
    public async Task<ActionResult<PolicyDetailDto>> ApplyLifecycleAction(Guid id, string actionName, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _policyService.ApplyLifecycleActionAsync(id, actionName, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to apply policy action", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
