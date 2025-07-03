using Service.Contracts.Interfaces;

namespace Tournament.Services.Implementations;
public class ServiceManager(Lazy<ITournamentService> tournamentService, Lazy<IGameService> gameService) : IServiceManager
{
    public ITournamentService TournamentService => tournamentService.Value;
    public IGameService GameService => gameService.Value;
}

