using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Inbox;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.EventType).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ReceivedOn).IsRequired();
        builder.HasIndex(x => x.ProcessedOn);
    }
}
