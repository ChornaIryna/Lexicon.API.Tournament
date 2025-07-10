using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;
using Tournament.Shared.DTOs;

namespace Tournament.Tests.Helpers;
public static class TournamentData
{
    public static List<TournamentDto> GetTournamentDtos(int count)
    {
        var tournaments = new List<TournamentDto>();
        for (int i = 1; i <= count; i++)
        {
            tournaments.Add(new TournamentDto
            {
                Title = $"Tournament {i}",
                StartDate = DateTime.Now.AddDays(i),
            });
        }
        return tournaments;
    }

    public static TournamentDto GetTournamentDto(int id)
    {
        return new TournamentDto
        {
            Title = $"Single Tournament {id}",
            StartDate = DateTime.Now.AddDays(id),
        };
    }

    public static TournamentCreateDto GetTournamentCreateDto(string title = "New Tournament")
    {
        return new TournamentCreateDto
        {
            Title = title,
            StartDate = DateTime.Now.AddDays(10)
        };
    }

    public static TournamentEditDto GetTournamentEditDto(int id, string title = "Updated Tournament")
    {
        return new TournamentEditDto
        {
            Id = id,
            Title = title,
            StartDate = DateTime.Now.AddDays(20)
        };
    }

    public static JsonPatchDocument<TournamentEditDto> GetTournamentJsonPatchDocument(int id, string newTitle)
    {
        var patchDoc = new JsonPatchDocument<TournamentEditDto>(new List<Operation<TournamentEditDto>>
        {
            new Operation<TournamentEditDto>
            {
                op = "replace",
                path = "/Title",
                value = newTitle
            }
        }, new CamelCasePropertyNamesContractResolver()); // Important for correct patching
        return patchDoc;
    }

    public static QueryParameters GetQueryParameters(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? orderBy = null)
    {
        return new QueryParameters
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            OrderBy = orderBy
        };
    }

    public static List<GameDto> GetGameDtos(int tournamentId, int count)
    {
        var games = new List<GameDto>();
        for (int i = 1; i <= count; i++)
        {
            games.Add(new GameDto
            {
                Title = $"Game {i} for Tournament {tournamentId}",
                Time = DateTime.Now.AddHours(i)
            });
        }
        return games;
    }

    public static GameDto GetGameDto(int tournamentId, int gameId)
    {
        return new GameDto
        {
            Title = $"Single Game {gameId} for Tournament {tournamentId}",
            Time = DateTime.Now.AddHours(1)
        };
    }

    public static GameCreateDto GetGameCreateDto(int tournamentId, string title = "New Game")
    {
        return new GameCreateDto
        {
            Title = title,
            Time = DateTime.Now.AddHours(5)
        };
    }

    public static GameEditDto GetGameEditDto(int tournamentId, int gameId, string title = "Updated Game")
    {
        return new GameEditDto
        {
            Id = gameId,
            Title = title,
            Time = DateTime.Now.AddHours(10)
        };
    }

    public static JsonPatchDocument<GameEditDto> GetGameJsonPatchDocument(int gameId, string newTitle)
    {
        var patchDoc = new JsonPatchDocument<GameEditDto>(new List<Operation<GameEditDto>>
        {
            new Operation<GameEditDto>
            {
                op = "replace",
                path = "/Title",
                value = newTitle
            }
        }, new CamelCasePropertyNamesContractResolver());
        return patchDoc;
    }
}
