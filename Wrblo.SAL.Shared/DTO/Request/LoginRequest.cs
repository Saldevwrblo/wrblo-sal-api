using System.ComponentModel.DataAnnotations;

namespace Wrblo.SAL.Shared.DTO.Request;

public class LoginRequest
{
    [Required, EmailAddress]
    public required string Username { get; set; }
    [Required]
    public required string Password { get; set; }
    // if there are unique cases for validations, the ones that attributes cannot handle,
    // we can use custom validation attributes: FluentValidation or similar.
}