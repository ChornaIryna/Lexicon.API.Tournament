using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface ITournamentRepository
{
    Task<IEnumerable<TournamentDetails>> GetAllTournamentsAsync(bool includeGames);
    Task<TournamentDetails?> GetTournamentByIdAsync(int id);
    Task<bool> TournamentExists(int id);
    void Add(TournamentDetails tournamentDetails);
    void Update(TournamentDetails tournamentDetails);
    void Remove(TournamentDetails tournamentDetails);
}
