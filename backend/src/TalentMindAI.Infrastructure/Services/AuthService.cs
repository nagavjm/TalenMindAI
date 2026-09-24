using Microsoft.EntityFrameworkCore;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Domain.Entities;
using TalentMindAI.Domain.Enums;
using TalentMindAI.Infrastructure.Data;

namespace TalentMindAI.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(AppDbContext dbContext, IJwtTokenGenerator tokenGenerator)
    {
        _dbContext = dbContext;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var exists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email, ct);
        if (exists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        var (token, expiresAt) = _tokenGenerator.GenerateToken(user);
        return new AuthResponse(token, expiresAt, user.FullName, user.Email, user.Role.ToString());
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var (token, expiresAt) = _tokenGenerator.GenerateToken(user);
        return new AuthResponse(token, expiresAt, user.FullName, user.Email, user.Role.ToString());
    }
}
