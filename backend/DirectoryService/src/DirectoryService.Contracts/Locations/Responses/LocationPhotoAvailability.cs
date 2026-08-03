namespace DirectoryService.Contracts.Locations.Responses;

public enum LocationPhotoAvailability
{
    Available,
    TemporarilyUnavailable,
    Missing,
}

public static class LocationPhotoAvailabilityExtensions
{
    public static string ToContractValue(this LocationPhotoAvailability availability) =>
        availability switch
        {
            LocationPhotoAvailability.Available => "available",
            LocationPhotoAvailability.TemporarilyUnavailable => "temporarily_unavailable",
            LocationPhotoAvailability.Missing => "missing",
            _ => throw new ArgumentOutOfRangeException(nameof(availability), availability, null),
        };
}
