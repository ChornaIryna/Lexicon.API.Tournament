﻿using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class UoW(TournamentContext context) : IUoW
{
    public ITournamentRepository TournamentRepository => new TournamentRepository(context);

    public IGameRepository GameRepository => new GameRepository(context);

    public async Task CompleteAsync() => await context.SaveChangesAsync();
    public bool HasChanges() => context.ChangeTracker.HasChanges();
}
