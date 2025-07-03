using AutoMapper;
using Service.Contracts.Interfaces;
using Tournament.Core.Repositories;

namespace Tournament.Services.Implementations;
public class GameService(IUoW unitOfWork, IMapper mapper) : IGameService
{
}
