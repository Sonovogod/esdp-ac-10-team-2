namespace Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public Guid Oid { get; set; }

    public DateTime Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}