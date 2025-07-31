namespace ValidationFlow.Domain;

public record SaveAudit(
    string EntityType,
    Guid EntityId,
    decimal MetricValue,
    bool Validated,
    DateTime Timestamp);
