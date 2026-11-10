using Microsoft.AspNetCore.Identity;

namespace Wrblo.SAL.Shared.Entities;

public class ApplicationUser : IdentityUser, IAuditedEntity, ISoftDelete
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
    public string? GoogleId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? RefreshToken { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? Token { get; set; }
    // Soft-delete
    public bool IsDeleted { get; set; } = false;
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    // Audit
    public DateTime? CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? CreatedBy { get; set; } = "System";
    public string? ModifiedBy { get; set; }
}
