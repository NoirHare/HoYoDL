using HoYoDL.Api.Internal.Dtos;
using HoYoDL.Api.Models;

namespace HoYoDL.Api.Internal.Extensions;

internal static class BranchesMappingException {
    extension(BranchesDto dto) {
        public Branches ToModel() => new() {
            Main = dto.Main.ToModel(),
            PreDownload = dto.PreDownload?.ToModel(),
        };
    }
    extension(BranchDto dto) {
        public Branch ToModel() => new() {
            Id = dto.Id,
            PackageId = dto.PackageId,
            Password = dto.Password,
        };
    }
}