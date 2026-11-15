using System.ComponentModel.DataAnnotations;

namespace Wrblo.SAL.Shared.DTO.Request;

public sealed class RegisterRequest
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required, MinLength(8)]
    public required string Password { get; set; }

    [Required, MinLength(1)]
    public required string FirstName { get; set; }

    [Required, MinLength(1)]
    public required string LastName { get; set; }
}