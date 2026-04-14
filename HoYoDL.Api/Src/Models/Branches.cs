namespace HoYoDL.Api.Models;

public sealed class Branches {
    public required Branch Main { get; init; }
    public required Branch? PreDownload { get; init; }
}

public sealed class Branch {
    public required string Id { get; init; }
    public required string PackageId { get; init; }
    public required string Password { get; init; }
}