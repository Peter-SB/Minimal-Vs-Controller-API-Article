using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shared.Data;


namespace ControllerApi.Controllers;

[Route("/[controller]")]
[ApiController]
public class SongsController : ControllerBase
{
    private readonly localDb _db;

    public SongsController(localDb db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Song>>> GetAllSongs()
    {
        return await _db.Songs.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Song>> GetSong(int id)
    {
        var song = await _db.Songs.FindAsync(id);

        if (song == null)
        {
            return NotFound();
        }

        return song;
    }

    [HttpPost]
    public async Task<ActionResult<Song>> SaveSong(Song song)
    {
        _db.Songs.Add(song);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSong), new { id = song.Id }, song);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSong(int id, Song inputSong)
    {
        var song = await _db.Songs.FindAsync(id);

        if (song == null)
        {
            return NotFound();
        }

        song.Name = inputSong.Name;
        song.Artist = inputSong.Artist;

        await _db.SaveChangesAsync();

        return Ok(song);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSong(int id)
    {
        var song = await _db.Songs.FindAsync(id);
        if (song == null)
        {
            return NotFound();
        }

        _db.Songs.Remove(song);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
