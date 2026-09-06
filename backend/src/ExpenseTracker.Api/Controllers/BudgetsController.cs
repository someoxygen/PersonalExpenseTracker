using ExpenseTracker.Application.Features.Budgets;
using Microsoft.AspNetCore.Mvc;
namespace ExpenseTracker.Api.Controllers;

[ApiController, Route("api/budgets")]
public sealed class BudgetsController(BudgetService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<List<BudgetResponse>>> List(CancellationToken ct, int? month = null, int? year = null) => Ok(await service.List(month, year, ct));
    [HttpGet("{id:guid}")] public async Task<ActionResult<BudgetResponse>> Get(Guid id, CancellationToken ct) => Ok(await service.Get(id, ct));
    [HttpPost, ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<BudgetResponse>> Create(BudgetRequest request, CancellationToken ct)
    {
        var result = await service.Create(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, BudgetRequest request, CancellationToken ct)
    { await service.Update(id, request, ct); return NoContent(); }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    { await service.Delete(id, ct); return NoContent(); }
}
