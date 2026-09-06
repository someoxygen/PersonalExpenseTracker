using ExpenseTracker.Application.Features.Accounts;
using Microsoft.AspNetCore.Mvc;
namespace ExpenseTracker.Api.Controllers;

[ApiController, Route("api/accounts")]
public sealed class AccountsController(AccountService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<List<AccountResponse>>> List(CancellationToken ct) => Ok(await service.List(ct));
    [HttpGet("{id:guid}")] public async Task<ActionResult<AccountResponse>> Get(Guid id, CancellationToken ct) => Ok(await service.Get(id, ct));
    [HttpPost, ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<AccountResponse>> Create(AccountRequest request, CancellationToken ct)
    {
        var result = await service.Create(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AccountRequest request, CancellationToken ct)
    { await service.Update(id, request, ct); return NoContent(); }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    { await service.Delete(id, ct); return NoContent(); }
}
