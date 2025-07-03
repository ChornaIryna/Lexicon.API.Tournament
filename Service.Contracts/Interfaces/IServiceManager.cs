namespace Service.Contracts.Interfaces;
public interface IServiceManager
{
    ITournamentService TournamentService { get; }
    IGameService GameService { get; }
}
