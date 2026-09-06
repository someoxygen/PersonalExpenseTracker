using ExpenseTracker.Application.Features.Categories;
using Microsoft.AspNetCore.Mvc;
namespace ExpenseTracker.Api.Controllers;

[ApiController, Route("api/categories")]
public sealed class CategoriesController(CategoryService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<List<CategoryResponse>>> List(CancellationToken ct) => Ok(await service.List(ct));
    [HttpGet("{id:guid}")] public async Task<ActionResult<CategoryResponse>> Get(Guid id, CancellationToken ct) => Ok(await service.Get(id, ct));
    [HttpPost, ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request, CancellationToken ct)
    {
        var result = await service.Create(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CategoryRequest request, CancellationToken ct)
    { await service.Update(id, request, ct); return NoContent(); }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    { await service.Delete(id, ct); return NoContent(); }
}
