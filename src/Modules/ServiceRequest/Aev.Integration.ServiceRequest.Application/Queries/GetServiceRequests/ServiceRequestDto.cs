namespace Aev.Integration.ServiceRequest.Application.Queries.GetServiceRequests;

public record ServiceRequestDto(
    Guid Id,
    string CustomerCode,
    string CustomerType,
    string RequestType,
    string Description,
    string Status,
    DateTime CreatedAt);
