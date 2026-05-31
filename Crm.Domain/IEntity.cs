namespace Crm.Domain;

public abstract class IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}