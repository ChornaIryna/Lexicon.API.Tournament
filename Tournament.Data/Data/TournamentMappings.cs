using AutoMapper;
using Tournament.Core.DTOs;
using Tournament.Core.Entities;

namespace Tournament.Data.Data;
public class TournamentMappings : Profile
{
    public TournamentMappings()
    {
        CreateMap<TournamentDto, TournamentDetails>()
            .ReverseMap();
        CreateMap<TournamentWithGamesDto, TournamentDetails>()
            .ForMember(target => target.Games, config => config.AllowNull())
            .ReverseMap();
        CreateMap<TournamentCreateDto, TournamentDetails>()
            .ForMember(target => target.Games, config => config.AllowNull())
            .ReverseMap();
        CreateMap<TournamentEditDto, TournamentDetails>()
            .ForMember(target => target.Games, config => config.AllowNull())
            .ReverseMap();

        CreateMap<GameDto, Game>()
            .ReverseMap();
        CreateMap<GameCreateDto, Game>()
            .ReverseMap();
        CreateMap<GameEditDto, Game>()
            .ReverseMap();
    }
}
