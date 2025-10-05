using DataProcessorService.Models;
using Microsoft.EntityFrameworkCore;

namespace DataProcessorService.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DeviceStateEntity> DeviceStates { get; set; }
}
