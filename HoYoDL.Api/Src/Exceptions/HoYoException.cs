namespace HoYoDL.Api.Exceptions;

public class HoYoException : Exception {
    public HoYoException() { }
    public HoYoException(string? message) : base(message) { }
    public HoYoException(string? message, Exception? innerException) : base(message, innerException) { }
}