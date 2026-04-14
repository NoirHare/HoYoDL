using HoYoDL.Api.Internal.Dtos;
using HoYoDL.Api.Models;

namespace HoYoDL.Api.Internal.Extensions;

internal static class ManifestMappingException {
    extension(FileDto dto) {
        public HoYoFile ToModel() => new() {
            Path = dto.Path,
            Size = dto.Size,
            Chunks = dto.Chunks.ToModels(),
        };
    }
    extension(IReadOnlyList<FileDto> dtos) {
        public IReadOnlyList<HoYoFile> ToModels() {
            HoYoFile[] results = new HoYoFile[dtos.Count];
            for (int i = 0; i < dtos.Count; i++) {
                results[i] = dtos[i].ToModel();
            }
            return results;
        }
    }
    extension(ChunkDto dto) {
        public HoYoChunk ToModel() => new() {
            Id = dto.Id,
            Offset = dto.Offset,
            Md5 = dto.Md5,
            CompressedSize = dto.CompressedSize,
            UncompressedSize = dto.UncompressedSize,
        };
    }
    extension(IReadOnlyList<ChunkDto> dtos) {
        public IReadOnlyList<HoYoChunk> ToModels() {
            HoYoChunk[] results = new HoYoChunk[dtos.Count];
            for (int i = 0; i < dtos.Count; i++) {
                results[i] = dtos[i].ToModel();
            }
            return results;
        }
    }
}