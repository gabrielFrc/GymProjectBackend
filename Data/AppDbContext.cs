using GymProjectBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace GymProjectBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Location> Locations {get; set;}
}