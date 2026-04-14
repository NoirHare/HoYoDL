using System.Text;

namespace HoYoDL.Api.Internal.Utilities;

internal delegate UrlBuilder HostFn(string host);

internal static class Url {
    internal static UrlBuilder Https(string host) => new("https", host);
}

internal sealed class UrlBuilder(string scheme, string host) {
    private readonly string _scheme = scheme;
    private readonly string _host = host;
    private string _path = "/";
    private readonly List<KeyValuePair<string, string>> _queries = [];

    public UrlBuilder Path(string path) {
        _path = path.StartsWith('/') ? path : $"/{path}";
        return this;
    }

    public UrlBuilder Query(string key, string value) {
        _queries.Add(KeyValuePair.Create(key, value));
        return this;
    }

    public override string ToString() {
        StringBuilder builder = new();
        builder.Append(_scheme);
        builder.Append("://");
        builder.Append(_host);
        builder.Append(_path);
        if (_queries.Count == 0) return builder.ToString();

        for (int i = 0; i < _queries.Count; i++) {
            builder.Append(i == 0 ? '?' : '&');
            builder.Append(Uri.EscapeDataString(_queries[i].Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(_queries[i].Value));
        }

        return builder.ToString();
    }

    public Uri ToUri() => new(ToString());
}