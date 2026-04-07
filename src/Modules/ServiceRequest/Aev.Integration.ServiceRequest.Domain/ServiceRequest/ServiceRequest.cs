using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.ServiceRequest.Domain.ServiceRequest;

public class ServiceRequest : AggregateRoot<Guid>
{
    public string CustomerCode { get; private set; } = default!;
    public CustomerType CustomerType { get; private set; }
    public string RequestType { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public ServiceRequestStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ServiceRequest() { }

    public static ServiceRequest Create(string customerCode, CustomerType customerType, string requestType, string description)
    {
        var request = new ServiceRequest
        {
            Id = Guid.NewGuid(),
            CustomerCode = customerCode,
            CustomerType = customerType,
            RequestType = requestType,
            Description = description,
            Status = ServiceRequestStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        request.AddDomainEvent(new ServiceRequestCreatedDomainEvent(request.Id, customerCode, requestType));
        return request;
    }

    public void Complete()
    {
        Status = ServiceRequestStatus.Completed;
        IncrementVersion();
    }
}
