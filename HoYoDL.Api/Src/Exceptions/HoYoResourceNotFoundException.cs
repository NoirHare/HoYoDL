namespace HoYoDL.Api.Exceptions;

internal sealed class HoYoResourceNotFoundException(string resource, string id) : HoYoException($"Resource '{resource}' with ID '{id}' was not found.") {
    public string Resource { get; } = resource;
    public string Id { get; } = id;
}