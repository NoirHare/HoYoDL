namespace HoYoDL.Api.Exceptions;

public class HoYoApiException(long retcode, string error) : HoYoException($"({retcode}){error}") {
    public HoYoApiException(string message) : this(0, message) { }
    public long Retcode { get; } = retcode;
    public string Error { get; } = error;
}