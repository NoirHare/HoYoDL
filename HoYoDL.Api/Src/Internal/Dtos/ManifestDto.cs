using LightProto;

namespace HoYoDL.Api.Internal.Dtos;

[ProtoContract]
internal sealed partial class ManifestDto {
    [ProtoMember(1)]
    public required IReadOnlyList<FileDto> Files { get; init; }
}

[ProtoContract]
internal sealed partial class FileDto {
    [ProtoMember(1)]
    public required string Path { get; init; }

    [ProtoMember(2)]
    public required IReadOnlyList<ChunkDto> Chunks { get; init; }

    [ProtoMember(4)]
    public required ulong Size { get; init; }
}

[ProtoContract]
internal sealed partial class ChunkDto {
    [ProtoMember(1)]
    public required string Id { get; init; }

    [ProtoMember(2)]
    public required string Md5 { get; init; }

    [ProtoMember(3)]
    public required ulong Offset { get; init; }

    [ProtoMember(4)]
    public required ulong CompressedSize { get; init; }

    [ProtoMember(5)]
    public required ulong UncompressedSize { get; init; }
}