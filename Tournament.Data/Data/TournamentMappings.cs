using AutoMapper;
using Tournament.Core.Entities;
using Tournament.Shared.DTOs;

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
            .ForMember(target => target.StartDate,
                        opt => opt.MapFrom((src, destination) =>
                                             src.StartDate == default
                                             ? DateTime.Now
                                             : src.StartDate))
            .ReverseMap();
        CreateMap<TournamentEditDto, TournamentDetails>()
            .ForMember(target => target.Games, config => config.AllowNull())
            .ForMember(target => target.StartDate,
                        opt => opt.MapFrom((src, destination) =>
                                             src.StartDate == default
                                             ? destination.StartDate
                                             : src.StartDate))
            .ReverseMap();

        CreateMap<GameDto, Game>()
            .ReverseMap();
        CreateMap<GameCreateDto, Game>()
            .ForMember(target => target.Time,
                        opt => opt.MapFrom((src, destination) =>
                                             src.Time == default
                                             ? DateTime.Now
                                             : src.Time))
            .ReverseMap();
        CreateMap<GameEditDto, Game>()
            .ForMember(target => target.Time,
                        opt => opt.MapFrom((src, destination) =>
                                             src.Time == default
                                             ? destination.Time
                                             : src.Time))
            .ReverseMap();
    }
}
