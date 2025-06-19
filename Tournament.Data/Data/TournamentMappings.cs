using AutoMapper;
using Tournament.Core.DTOs;
using Tournament.Core.Entities;

namespace Tournament.Data.Data;
public class TournamentMappings : Profile
{
    public TournamentMappings()
    {
        CreateMap<TournamentDto, TournamentDetails>()
            .ForMember(target => target.Games, config => config.AllowNull())
            .ReverseMap();
        CreateMap<GameDto, Game>()
            .ReverseMap();
        CreateMap<EditTournamentDto, TournamentDetails>()
            .ForMember(target => target.Games, config => config.Ignore())
            .ReverseMap();
    }
}
