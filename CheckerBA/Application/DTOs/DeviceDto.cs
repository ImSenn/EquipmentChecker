namespace CheckerBA.Application.DTOs
{
    public record DeviceDto(
        string DeviceId,
        string Name,
        string Type,
        double PowerRating,
        DateTime CreatedAt);

    public record CreateDeviceRequest(
        string DeviceId,
        string Name,
        string Type,
        double PowerRating);

    public record UpdateDeviceRequest(
        string Name,
        string Type,
        double PowerRating);

    public record CommandRequest(
        string Command,
        object? Value = null);
}
