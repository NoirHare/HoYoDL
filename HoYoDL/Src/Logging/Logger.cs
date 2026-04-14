using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HoYoDL.Logging;

public class Logger : IDisposable {
    private readonly ChannelReader<Output> _reader;
    private readonly ChannelWriter<Output> _writer;
    private readonly Task _loop;

    private Output _pin = new(true, null, null);

    public Logger() {
        Channel<Output> channel = Channel.CreateUnbounded<Output>(new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });

        _reader = channel.Reader;
        _writer = channel.Writer;

        _loop = OutputLoopAsync();
    }

    public void Log(string format, params object[] args) {
        _writer.TryWrite(new Output(false, format, args));
    }

    public void SetPin(string format, params object[] args) {
        _writer.TryWrite(new Output(true, format, args));
    }

    public void ClearPin() {
        _writer.TryWrite(new Output(true, null, null));
    }

    private async Task OutputLoopAsync() {
        try {
            using Stream stdout = Console.OpenStandardOutput();
            using StreamWriter writer = new(stdout, Console.OutputEncoding) { AutoFlush = false };

            StringBuilder sb = new();

            while (await _reader.WaitToReadAsync()) {
                sb.Clear();

                if (_pin.Format != null) sb.Append("\r\x1b[2K");

                while (_reader.TryRead(out Output output)) {
                    if (output.IsPin) _pin = output;
                    else if (output.Args == null || output.Args.Length == 0) sb.AppendLine(output.Format);
                    else sb.AppendFormat(output.Format, output.Args).AppendLine();
                }

                if (_pin.Format != null) {
                    if (_pin.Args == null || _pin.Args.Length == 0) sb.Append(_pin.Format);
                    else sb.AppendFormat(_pin.Format, _pin.Args);
                }

                if (sb.Length > 0) {
                    await writer.WriteAsync(sb.ToString());
                    await writer.FlushAsync();
                }
            }
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    public void Dispose() {
        _writer.Complete();
        _loop.Wait();

        if (_pin.Format != null) Console.WriteLine();

        GC.SuppressFinalize(this);
    }

    private readonly struct Output(bool isPin, string? format, object?[]? args) {
        [MemberNotNullWhen(false, nameof(Format))]
        public bool IsPin { get; init; } = isPin;
        public readonly string? Format = format;
        public readonly object?[]? Args = args;
    }
}