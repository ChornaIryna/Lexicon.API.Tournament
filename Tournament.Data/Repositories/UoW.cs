using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class UoW(TournamentContext context, Lazy<ITournamentRepository> tournamentRepository, Lazy<IGameRepository> gameRepository) : IUoW
{
    public ITournamentRepository TournamentRepository => tournamentRepository.Value;
    public IGameRepository GameRepository => gameRepository.Value;

    public async Task CompleteAsync() => await context.SaveChangesAsync();
    public bool HasChanges() => context.ChangeTracker.HasChanges();
}
