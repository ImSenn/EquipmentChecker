namespace CheckerWPF.Models
{
    public record DeviceDto(
        string DeviceId,
        string Name,
        string Type,
        double PowerRating,
        DateTime CreatedAt);

    public record TelemetryDto(
        string DeviceId,
        DateTime Timestamp,
        MetricsDto Metrics,
        StatusDto Status,
        SystemDto System);

    public record MetricsDto(
        double Temperature,
        double Vibration,
        double EstimatedPower);

    public record StatusDto(
        string RunState,
        string PowerState);

    public record SystemDto(
        double UpTime,
        double WifiRssi,
        double FreeHeap);

    public record AlertDto(
        string DeviceId,
        string Type,
        string Severity,
        double Value,
        string Message,
        DateTime Timestamp);

    public record EnergyDto(
        string DeviceId,
        DateTime Date,
        double EnergyKWh,
        double EstimatedCost);

    public record EventDto(
        string DeviceId,
        DateTime Timestamp,
        string Type,
        string Severity,
        double Value,
        string Message);

    public record LoginRequest(string Username, string Password);

    public record LoginResponse(
        string Token,
        string Username,
        string Role,
        DateTime ExpiresAt);

    public record CommandRequest(string Command, object? Value = null);
}
