using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aev.Integration.ServiceRequest.Infrastructure.Persistence.Configurations;

public class ServiceRequestConfiguration : IEntityTypeConfiguration<Domain.ServiceRequest.ServiceRequest>
{
    public void Configure(EntityTypeBuilder<Domain.ServiceRequest.ServiceRequest> builder)
    {
        builder.ToTable("service_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CustomerCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CustomerType).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.RequestType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.Ignore(x => x.DomainEvents);
    }
}
