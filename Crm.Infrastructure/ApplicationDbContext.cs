using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Crm.Infrastucture;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }
}