using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Aev.Integration.Invoicing.Domain.Invoice;
using Microsoft.EntityFrameworkCore;

namespace Aev.Integration.Invoicing.Infrastructure.Persistence;

public sealed class InvoicingDbContext(DbContextOptions<InvoicingDbContext> options) : BaseDbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("invoicing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoicingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
