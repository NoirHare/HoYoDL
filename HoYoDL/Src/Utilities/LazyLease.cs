namespace HoYoDL.Utilities;

public sealed class LazyLease<T>(Func<T> factory, int count) where T : IDisposable {
    private readonly Lazy<T> _value = new(factory, LazyThreadSafetyMode.ExecutionAndPublication);
    private int _remaining = count;

    private bool _disposed = false;

    public Handle Acquire() {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return new Handle(this);
    }

    private void Release() {
        if (_disposed) return;

        if (Interlocked.Decrement(ref _remaining) == 0) {
            if (_disposed) return;

            _disposed = true;
            if (_value.IsValueCreated) {
                _value.Value.Dispose();
            }
        }
    }

    public sealed class Handle(LazyLease<T> lease) : IDisposable {
        private LazyLease<T>? _lease = lease;

        public T Value {
            get {
                LazyLease<T>? lease = _lease;
                ObjectDisposedException.ThrowIf(lease == null, this);

                return lease._value.Value;
            }
        }

        public void Dispose() => Interlocked.Exchange(ref _lease, null)?.Release();

        public static implicit operator T(Handle handle) => handle.Value;
    }
}
