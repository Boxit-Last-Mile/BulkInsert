using Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework;

public class DatabaseContext : DbContext
{
    private readonly bool _creatingMigrations;
    
    public DbSet<ChildModel> Children {get; set;}
    public DbSet<ParentModel> Parents {get; set;}

    public DatabaseContext(DbContextOptions options) : base(options)
    {
        _creatingMigrations = false;
    }

    public DatabaseContext()
    {
        _creatingMigrations = true;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("Test");
        
        modelBuilder.Entity<ChildModel>().Property(x => x.Name).HasMaxLength(100).IsRequired();
        
        modelBuilder.Entity<ParentModel>().Property(x => x.Name).HasMaxLength(100).IsRequired();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        if (_creatingMigrations)
        {
            optionsBuilder.UseSqlServer();
        }
    }
}