using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wrblo.SAL.Shared.Entities;

public interface IEntity
{
    long Id { get; set; }
}

public interface IAuditedEntity
{
    DateTime? CreatedOn { get; set; }
    DateTime? ModifiedOn { get; set; }

    string? CreatedBy { get; set; }
    string? ModifiedBy { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    string? DeletedBy { get; set; }
    DateTime? DeletedOn { get; set; }
}

public abstract class Entity : IEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
}

public abstract class AuditedEntity : Entity, IAuditedEntity
{
    public DateTime? CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }

    public string? CreatedBy { get; set; } = "System";
    public string? ModifiedBy { get; set; }
}

public abstract class BaseEntity : AuditedEntity, ISoftDelete
{
    protected BaseEntity()
    {
        var wcaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, wcaTimeZone);

        CreatedOn = now;
        ModifiedOn = now;
    }

    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedOn { get; set; }
}


