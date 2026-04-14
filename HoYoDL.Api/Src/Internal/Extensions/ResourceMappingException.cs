using HoYoDL.Api.Internal.Dtos;
using HoYoDL.Api.Internal.Responses;
using HoYoDL.Api.Models;

namespace HoYoDL.Api.Internal.Extensions;

internal static class ResourceMappingException {
    extension(ResourceResponse response) {
        public Resource ToModel() => new() {
            Tag = response.Tag,
            Packages = response.Packages.ToModels(),
        };
    }
    extension(PackageDto dto) {
        public Package ToModel() => new() {
            Id = dto.Id,
            ManifestId = dto.Manifest.Id,
            ManifestDownloadPrefix = dto.ManifestDownload.Prefix,
            ChunkDownloadPrefix = dto.ChunkDownload.Prefix,
            CompressedSize = ulong.Parse(dto.Stats.CompressedSize),
            UncompressedSize = ulong.Parse(dto.Stats.UncompressedSize),
        };
    }
    extension(IReadOnlyList<PackageDto> dtos) {
        public IReadOnlyList<Package> ToModels() {
            Package[] results = new Package[dtos.Count];
            for (int i = 0; i < dtos.Count; i++) {
                results[i] = dtos[i].ToModel();
            }
            return results;
        }
    }
}