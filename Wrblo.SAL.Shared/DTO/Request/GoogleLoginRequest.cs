using System.ComponentModel.DataAnnotations;

namespace Wrblo.SAL.Shared.DTO.Request;

public sealed class GoogleLoginRequest
{
    [Required]
    public required string IdToken { get; set; }
}