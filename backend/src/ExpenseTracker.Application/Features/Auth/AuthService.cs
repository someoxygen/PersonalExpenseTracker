using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace ExpenseTracker.Application.Features.Auth;

public sealed class AuthService(IAppDbContext db, IPasswordService passwords, ITokenService tokens,
    ICurrentUser currentUser, IValidator<RegisterRequest> registerValidator, IValidator<LoginRequest> loginValidator)
{
    public async Task<AuthResponse> Register(RegisterRequest request, CancellationToken ct)
    {
        await registerValidator.ValidateAndThrowAsync(request, ct);
        var email = User.NormalizeEmail(request.Email);
        if (await db.Users.AnyAsync(x => x.Email == email, ct))
            throw AppException.Conflict("An account with this email already exists.");
        var user = User.Create(request.FirstName, request.LastName, email, passwords.Hash(request.Password));
        db.Users.Add(user);
        db.Categories.AddRange(Category.Defaults(user.Id));
        await db.SaveChangesAsync(ct);
        return Authenticate(user);
    }
    public async Task<AuthResponse> Login(LoginRequest request, CancellationToken ct)
    {
        await loginValidator.ValidateAndThrowAsync(request, ct);
        var email = User.NormalizeEmail(request.Email);
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Email == email, ct);
        var valid = passwords.Verify(request.Password, user?.PasswordHash);
        if (user is null || !valid) throw new AppException(401, "Invalid email or password.");
        return Authenticate(user);
    }
    public async Task<UserResponse> Me(CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == currentUser.Id, ct)
            ?? throw new AppException(401, "Authentication required.");
        return Map(user);
    }
    private AuthResponse Authenticate(User user)
    {
        var (token, expiresAt) = tokens.Create(user.Id);
        return new(token, expiresAt, Map(user));
    }
    private static UserResponse Map(User user) => new(user.Id, user.FirstName, user.LastName, user.Email, user.Currency);
}
