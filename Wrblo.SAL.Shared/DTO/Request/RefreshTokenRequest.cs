using System.ComponentModel.DataAnnotations;

namespace Wrblo.SAL.Shared.DTO.Request;

public sealed class RefreshTokenRequest
{
    [Required]
    public required string RefreshToken { get; set; }
}