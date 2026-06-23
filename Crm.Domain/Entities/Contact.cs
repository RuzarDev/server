namespace Crm.Domain.Entities;

public class Contact : IEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsDeleted { get; set; }
    public List<Note> Notes { get; set; }
    public List<Tag> Tags { get; set; }
    
}