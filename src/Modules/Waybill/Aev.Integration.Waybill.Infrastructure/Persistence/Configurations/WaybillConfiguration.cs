using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aev.Integration.Waybill.Infrastructure.Persistence.Configurations;

public class WaybillConfiguration : IEntityTypeConfiguration<Domain.Waybill.Waybill>
{
    public void Configure(EntityTypeBuilder<Domain.Waybill.Waybill> builder)
    {
        builder.ToTable("waybills");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.WaybillNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LogisticsProvider).HasMaxLength(200);
        builder.Property(x => x.SenderAddress).HasMaxLength(500);
        builder.Property(x => x.ReceiverAddress).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.Ignore(x => x.DomainEvents);
    }
}
