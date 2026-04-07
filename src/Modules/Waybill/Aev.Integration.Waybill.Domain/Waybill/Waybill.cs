using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.Waybill.Domain.Waybill;

public class Waybill : AggregateRoot<Guid>
{
    public string WaybillNumber { get; private set; } = default!;
    public string LogisticsProvider { get; private set; } = default!;
    public string SenderAddress { get; private set; } = default!;
    public string ReceiverAddress { get; private set; } = default!;
    public WaybillStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Waybill() { }

    public static Waybill Create(string waybillNumber, string senderAddress, string receiverAddress)
    {
        var waybill = new Waybill
        {
            Id = Guid.NewGuid(),
            WaybillNumber = waybillNumber,
            LogisticsProvider = "Nhat Tin Logistics",
            SenderAddress = senderAddress,
            ReceiverAddress = receiverAddress,
            Status = WaybillStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        waybill.AddDomainEvent(new WaybillCreatedDomainEvent(waybill.Id, waybillNumber));
        return waybill;
    }

    public void Ship()
    {
        Status = WaybillStatus.Shipped;
        IncrementVersion();
    }

    public void Deliver()
    {
        Status = WaybillStatus.Delivered;
        IncrementVersion();
    }
}
