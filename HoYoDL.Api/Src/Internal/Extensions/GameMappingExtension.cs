using HoYoDL.Api.Internal.Dtos;
using HoYoDL.Api.Models;

namespace HoYoDL.Api.Internal.Extensions;

internal static class GameMappingExtension {
    extension(GameDto dto) {
        public Game ToModel() => new() {
            Id = dto.Id,
            Name = dto.Display.Name,
        };
    }
    extension(IReadOnlyList<GameDto> dtos) {
        public IReadOnlyList<Game> ToModels() {
            Game[] results = new Game[dtos.Count];
            for (int i = 0; i < dtos.Count; i++) {
                results[i] = dtos[i].ToModel();
            }
            return results;
        }
    }
}