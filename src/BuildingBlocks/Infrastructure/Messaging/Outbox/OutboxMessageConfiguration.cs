using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Outbox;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.EventType).HasMaxLength(500).IsRequired();
        builder.Property(x => x.EventPayload).IsRequired();
        builder.Property(x => x.OccurredOn).IsRequired();
        builder.HasIndex(x => x.ProcessedOn);
    }
}
