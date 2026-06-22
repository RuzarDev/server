namespace Crm.Domain.Entities;

public class Note : IEntity
{
    public Guid ContactId { get; set; }
    public Contact Contact { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}