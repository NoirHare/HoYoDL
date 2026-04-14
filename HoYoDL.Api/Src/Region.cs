namespace HoYoDL.Api;

public class Region {
    private Region() { }

    public required string HypApiHost { get; init; }
    public required string TakumiApiHost { get; init; }

    public required string LauncherId { get; init; }

    public static readonly Region Global = new() {
        HypApiHost = "sg-hyp-api.hoyoverse.com",
        TakumiApiHost = "sg-public-api-static.hoyoverse.com",
        LauncherId = "VYTpXlbWo8",
    };

    public static readonly Region China = new() {
        HypApiHost = "hyp-api.mihoyo.com",
        TakumiApiHost = "api-takumi.mihoyo.com",
        LauncherId = "jGHBHlcOq1",
    };
}