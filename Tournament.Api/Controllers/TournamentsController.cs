using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Data.Data;

namespace Tournament.Api.Controllers;
[AllowAnonymous] // Allow anonymous access to the API for demonstration purposes; adjust as needed for security.
[Route("api/[controller]")]
[ApiController]
public class TournamentsController : ControllerBase
{
    private readonly TournamentContext _context;

    public TournamentsController(TournamentContext context)
    {
        _context = context;
    }

    // GET: api/Tournaments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentDetails>>> GetTournamentDetails()
    {
        return await _context.TournamentDetails.Include(t => t.Games).ToListAsync();
    }

    // GET: api/Tournaments/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TournamentDetails>> GetTournamentDetails(int id)
    {
        var tournamentDetails = await _context.TournamentDetails.FindAsync(id);

        if (tournamentDetails == null)
        {
            return NotFound();
        }

        return tournamentDetails;
    }

    // PUT: api/Tournaments/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTournamentDetails(int id, TournamentDetails tournamentDetails)
    {
        if (id != tournamentDetails.Id)
        {
            return BadRequest();
        }

        _context.Entry(tournamentDetails).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TournamentDetailsExists(id))
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

    // POST: api/Tournaments
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TournamentDetails>> PostTournamentDetails(TournamentDetails tournamentDetails)
    {
        _context.TournamentDetails.Add(tournamentDetails);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTournamentDetails", new { id = tournamentDetails.Id }, tournamentDetails);
    }

    // DELETE: api/Tournaments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTournamentDetails(int id)
    {
        var tournamentDetails = await _context.TournamentDetails.FindAsync(id);
        if (tournamentDetails == null)
        {
            return NotFound();
        }

        _context.TournamentDetails.Remove(tournamentDetails);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TournamentDetailsExists(int id)
    {
        return _context.TournamentDetails.Any(e => e.Id == id);
    }
}
