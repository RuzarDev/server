namespace Crm.Domain.Entities;

public class Tag : IEntity
{
    public string Name { get; set; }
    public List<Contact> Contacts { get; set; }
}