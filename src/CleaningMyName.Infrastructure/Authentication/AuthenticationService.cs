using CleaningMyName.Application.Common.Interfaces;
using CleaningMyName.Application.Common.Models;
using CleaningMyName.Domain.Interfaces.Repositories;
using CleaningMyName.Infrastructure.Persistence;
using CleaningMyName.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CleaningMyName.Infrastructure.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtSettings _jwtSettings;
    private readonly ApplicationDbContext _dbContext;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        JwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtSettings,
        ApplicationDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtSettings = jwtSettings.Value;
        _dbContext = dbContext;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string email, string password)
    {
        try
        {
            var emailObj = Email.Create(email);
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(emailObj);

            if (user == null)
            {
                return new AuthenticationResult { Success = false, Message = "User not found." };
            }

            if (!user.IsActive)
            {
                return new AuthenticationResult { Success = false, Message = "User account is deactivated." };
            }

            if (!_passwordService.VerifyPassword(user.PasswordHash, password))
            {
                return new AuthenticationResult { Success = false, Message = "Invalid password." };
            }

            // Generate access token
            var token = _jwtTokenGenerator.GenerateToken(user);

            // Generate refresh token
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken(
                user.Id,
                refreshToken,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
            );

            // Save refresh token to database
            _dbContext.RefreshTokens.Add(refreshTokenEntity);
            await _dbContext.SaveChangesAsync();

            // Record login
            user.RecordLogin();
            await _unitOfWork.SaveChangesAsync();

            return new AuthenticationResult
            {
                Success = true,
                UserId = user.Id.ToString(),
                UserName = user.FullName,
                Token = token,
                RefreshToken = refreshToken,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResult { Success = false, Message = $"Authentication failed: {ex.Message}" };
        }
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Find the refresh token in the database
            var refreshTokenEntity = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (refreshTokenEntity == null)
            {
                return new AuthenticationResult { Success = false, Message = "Invalid refresh token." };
            }

            if (!refreshTokenEntity.IsActive)
            {
                return new AuthenticationResult { Success = false, Message = "Refresh token is expired, used, or revoked." };
            }

            var user = refreshTokenEntity.User;

            if (!user.IsActive)
            {
                return new AuthenticationResult { Success = false, Message = "User account is deactivated." };
            }

            // Mark the current refresh token as used
            refreshTokenEntity.MarkAsUsed();

            // Generate new tokens
            var newToken = _jwtTokenGenerator.GenerateToken(user);
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var newRefreshTokenEntity = new RefreshToken(
                user.Id,
                newRefreshToken,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
            );

            // Save changes
            _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
            await _dbContext.SaveChangesAsync();

            return new AuthenticationResult
            {
                Success = true,
                UserId = user.Id.ToString(),
                UserName = user.FullName,
                Token = newToken,
                RefreshToken = newRefreshToken,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResult { Success = false, Message = $"Token refresh failed: {ex.Message}" };
        }
    }
}
