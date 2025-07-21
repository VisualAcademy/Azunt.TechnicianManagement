using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Azunt.TechnicianManagement;

public class TechnicianAppDbContextFactory
{
    private readonly IConfiguration? _configuration;

    public TechnicianAppDbContextFactory() { }

    public TechnicianAppDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TechnicianAppDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<TechnicianAppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new TechnicianAppDbContext(options);
    }

    public TechnicianAppDbContext CreateDbContext(DbContextOptions<TechnicianAppDbContext> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new TechnicianAppDbContext(options);
    }

    public TechnicianAppDbContext CreateDbContext()
    {
        if (_configuration == null)
        {
            throw new InvalidOperationException("Configuration is not provided.");
        }

        var defaultConnection = _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            throw new InvalidOperationException("DefaultConnection is not configured properly.");
        }

        return CreateDbContext(defaultConnection);
    }
}