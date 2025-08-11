using Microsoft.EntityFrameworkCore;

namespace Dynamo.Data;

public class DynamoContext : DbContext
{
    public DbSet<EnergyMeasurements> EnergyMeasurements { get; set; }

    public DbSet<EnergyPredictions> EnergyPredictions { get; set; }

    public DbSet<Houses> Houses { get; set; }

    public DbSet<HouseAliases> HouseAliases { get; set; }

    public DynamoContext(DbContextOptions<DynamoContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        //builder.Entity<Blog>().Property(b => b.Url).IsRequired();

    }
    
}
