using MediatR;
using Identity.Application.DTOs;

namespace Identity.Application.Features.Users;

public record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role) : IRequest<UserDto>;

public record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Role) : IRequest<Unit>;

public record LoginCommand(
    string Email,
    string Password) : IRequest<AuthResponseDto>;

public record RefreshTokenCommand(
    string Token,
    string RefreshToken) : IRequest<AuthResponseDto>;