namespace HoYoDL.Api.Exceptions;

public class HoYoSerializationException : HoYoException {
    public HoYoSerializationException() { }
    public HoYoSerializationException(string? message) : base(message) { }
    public HoYoSerializationException(string? message, Exception? innerException) : base(message, innerException) { }
}