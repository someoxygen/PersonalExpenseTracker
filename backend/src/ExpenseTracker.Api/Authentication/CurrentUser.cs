using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Common;
namespace ExpenseTracker.Api.Authentication;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id => Guid.TryParse(accessor.HttpContext?.User.FindFirst("sub")?.Value, out var id)
        ? id : throw new AppException(401, "Authentication required.");
}
