using CleaningMyName.Application.Common.Interfaces;
using CleaningMyName.Domain.Interfaces.Repositories;
using CleaningMyName.Domain.ValueObjects;
using CleaningMyName.Domain.Entities;
using CleaningMyName.Application.Common.Models;
using MediatR;

namespace CleaningMyName.Application.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailObj = Email.Create(request.Email);
            var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(emailObj, cancellationToken);
            
            if (existingUser != null)
            {
                return Result.Failure<Guid>($"User with email {request.Email} already exists.");
            }

            var passwordHash = _passwordService.HashPassword(request.Password);

            var user = new User(
                request.FirstName,
                request.LastName,
                emailObj,
                passwordHash);

            if (request.Roles.Any())
            {
                foreach (var roleName in request.Roles)
                {
                    var role = await _unitOfWork.RoleRepository.GetByNameAsync(roleName, cancellationToken);
                    if (role != null)
                    {
                        user.AddRole(role);
                    }
                }
            }

            await _unitOfWork.UserRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(user.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>($"Failed to create user: {ex.Message}");
        }
    }
}
