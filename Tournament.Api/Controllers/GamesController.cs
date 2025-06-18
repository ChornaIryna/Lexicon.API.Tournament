using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Data.Data;

namespace Tournament.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GamesController : ControllerBase
{
    private readonly TournamentContext _context;

    public GamesController(TournamentContext context)
    {
        _context = context;
    }

    // GET: api/Games
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Games>>> GetGames()
    {
        return await _context.Games.ToListAsync();
    }

    // GET: api/Games/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Games>> GetGames(int id)
    {
        var games = await _context.Games.FindAsync(id);

        if (games == null)
        {
            return NotFound();
        }

        return games;
    }

    // PUT: api/Games/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutGames(int id, Games games)
    {
        if (id != games.Id)
        {
            return BadRequest();
        }

        _context.Entry(games).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GamesExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Games
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Games>> PostGames(Games games)
    {
        _context.Games.Add(games);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetGames", new { id = games.Id }, games);
    }

    // DELETE: api/Games/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGames(int id)
    {
        var games = await _context.Games.FindAsync(id);
        if (games == null)
        {
            return NotFound();
        }

        _context.Games.Remove(games);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool GamesExists(int id)
    {
        return _context.Games.Any(e => e.Id == id);
    }
}
