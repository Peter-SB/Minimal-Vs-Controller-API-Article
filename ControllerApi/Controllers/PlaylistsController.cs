using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shared.Data;

namespace ControllerApi.Controllers;

[Route("/[controller]")]
[ApiController]
public class PlaylistsController : ControllerBase
{
    private readonly localDb _db;

    public PlaylistsController(localDb db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Playlist>>> GetAllPlaylists()
    {
        return await _db.Playlists.Include(p => p.Songs).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Playlist>> GetPlaylist(int id)
    {
        var playlist = await _db.Playlists.Include(p => p.Songs).FirstOrDefaultAsync(p => p.Id == id);

        if (playlist == null)
        {
            return NotFound();
        }

        return playlist;
    }

    [HttpPost]
    public async Task<ActionResult<Playlist>> SavePlaylist(Playlist playlist)
    {
        _db.Playlists.Add(playlist);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, playlist);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlaylist(int id, Playlist inputPlaylist)
    {
        var playlist = await _db.Playlists.Include(p => p.Songs).FirstOrDefaultAsync(p => p.Id == id);

        if (playlist == null)
        {
            return NotFound();
        }

        playlist.Name = inputPlaylist.Name;
        playlist.Songs = inputPlaylist.Songs;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> AddSongToPlaylist(int playlistId, int songId)
    {
        var playlist = await _db.Playlists.Include(p => p.Songs).FirstOrDefaultAsync(p => p.Id == playlistId);
        if (playlist == null) return NotFound();

        var song = await _db.Songs.FindAsync(songId);
        if (song == null) return NotFound();

        if (!playlist.Songs.Contains(songId))
        {
            playlist.Songs.Add(songId);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlaylist(int id)
    {
        var playlist = await _db.Playlists.FindAsync(id);
        if (playlist == null)
        {
            return NotFound();
        }

        _db.Playlists.Remove(playlist);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
