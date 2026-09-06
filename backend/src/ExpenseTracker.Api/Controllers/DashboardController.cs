using ExpenseTracker.Application.Features.Dashboard;
using ExpenseTracker.Application.Features.Budgets;
using Microsoft.AspNetCore.Mvc;
namespace ExpenseTracker.Api.Controllers;

[ApiController, Route("api/dashboard")]
public sealed class DashboardController(DashboardService service, BudgetService budgets) : ControllerBase
{
    [HttpGet("summary")] public async Task<ActionResult<DashboardSummary>> Summary(CancellationToken ct) => Ok(await service.Summary(ct));
    [HttpGet("monthly")] public async Task<ActionResult<List<MonthlyFinance>>> Monthly(CancellationToken ct, int months = 6) => Ok(await service.Monthly(months, ct));
    [HttpGet("category-expenses")] public async Task<ActionResult<List<CategoryExpense>>> Categories(CancellationToken ct) => Ok(await service.CategoryExpenses(ct));
    [HttpGet("recent-transactions")] public async Task<ActionResult<List<Application.Features.Transactions.TransactionResponse>>> Recent(CancellationToken ct) => Ok(await service.Recent(ct));
    [HttpGet("budgets")] public async Task<ActionResult<List<BudgetResponse>>> Budgets(CancellationToken ct) => Ok(await budgets.List(null, null, ct));
}
