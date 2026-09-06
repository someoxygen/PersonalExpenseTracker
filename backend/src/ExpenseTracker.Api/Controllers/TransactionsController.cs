using ExpenseTracker.Application.Features.Transactions;
using Microsoft.AspNetCore.Mvc;
namespace ExpenseTracker.Api.Controllers;

[ApiController, Route("api/transactions")]
public sealed class TransactionsController(TransactionService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<ExpenseTracker.Application.Features.Transactions.PageResponse<TransactionResponse>>> List([FromQuery] TransactionQuery query, CancellationToken ct) => Ok(await service.List(query, ct));
    [HttpGet("{id:guid}")] public async Task<ActionResult<TransactionResponse>> Get(Guid id, CancellationToken ct) => Ok(await service.Get(id, ct));
    [HttpPost, ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<TransactionResponse>> Create(TransactionRequest request, CancellationToken ct)
    {
        var result = await service.Create(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TransactionRequest request, CancellationToken ct)
    { await service.Update(id, request, ct); return NoContent(); }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    { await service.Delete(id, ct); return NoContent(); }
}
