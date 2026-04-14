using System;
using System.Diagnostics;
using System.Threading;

namespace HoYoDL.Utilities;

internal sealed class ProgressTracker(long total) {
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private readonly long _total = total;

    private long _completed;

    private long _latestCompleted;
    private double _latestSeconds;

    public void Report(long amount) => Interlocked.Add(ref _completed, amount);

    public Snapshot GetSnapshot() {
        double now = _stopwatch.Elapsed.TotalSeconds;
        long completed = Interlocked.Read(ref _completed);

        double windowDuration = now - _latestSeconds;
        long windowAmount = completed - _latestCompleted;

        double speed = windowDuration > 0 ? windowAmount / windowDuration : 0;
        int percentage = _total > 0 ? (int)(completed * 10000 / _total) : 0;
        TimeSpan eta = speed > 0 ? TimeSpan.FromSeconds((_total - completed) / speed) : TimeSpan.MaxValue;

        _latestSeconds = now;
        _latestCompleted = completed;

        return new Snapshot {
            Percentage = percentage,
            Speed = speed,
            Eta = eta,
        };
    }

    public readonly struct Snapshot {
        public required int Percentage { get; init; }
        public required double Speed { get; init; }
        public required TimeSpan Eta { get; init; }
    }
}