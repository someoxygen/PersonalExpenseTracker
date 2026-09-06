using ExpenseTracker.Application.Features.Transfers;
using Microsoft.AspNetCore.Mvc;
namespace ExpenseTracker.Api.Controllers;

[ApiController, Route("api/transfers")]
public sealed class TransfersController(TransferService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<ExpenseTracker.Application.Features.Transactions.PageResponse<TransferResponse>>> List(CancellationToken ct, int page = 1, int pageSize = 20) => Ok(await service.List(page, pageSize, ct));
    [HttpGet("{id:guid}")] public async Task<ActionResult<TransferResponse>> Get(Guid id, CancellationToken ct) => Ok(await service.Get(id, ct));
    [HttpPost, ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<TransferResponse>> Create(TransferRequest request, CancellationToken ct)
    {
        var result = await service.Create(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
