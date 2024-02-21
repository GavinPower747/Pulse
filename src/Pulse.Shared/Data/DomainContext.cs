using Microsoft.EntityFrameworkCore;

namespace Pulse.Shared.Data;

public class DomainContext : DbContext
{
    public DomainContext(DbContextOptions options)
        : base(options) { }
}
