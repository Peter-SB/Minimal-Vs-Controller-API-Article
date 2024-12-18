using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Data;

namespace MinimalApi;

public class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<localDb>(opt => {
            opt.UseSqlite("Data Source=MinimalData.db");
        });

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minimal Api Demo", Version = "v1" });
        });

        var app = builder.Build();


        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<localDb>();
            dbContext.Database.OpenConnection();
            dbContext.Database.EnsureCreated();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
        }

        var songItems = app.MapGroup("/songs");
        var playlistItems = app.MapGroup("/playlists");

        songItems.MapGet("/", GetAllSongs);
        songItems.MapGet("/{id}", GetSong);
        songItems.MapPost("/", SaveSong);
        songItems.MapPut("/{id}", UpdateSong);
        songItems.MapDelete("/{id}", DeleteSong);

        playlistItems.MapGet("/", GetAllPlaylists);
        playlistItems.MapGet("/{id}", GetPlaylist);
        playlistItems.MapPost("/", SavePlaylist);
        playlistItems.MapPut("/{id}", UpdatePlaylist);
        playlistItems.MapDelete("/{id}", DeletePlaylist);

        app.Run();


        // --------- Song Functions ---------


        static async Task<IResult> GetAllSongs(localDb db)
        {
            return TypedResults.Ok(await db.Songs.ToArrayAsync());
        }


        static async Task<IResult> GetSong(int id, localDb db)
        {
            return await db.Songs.FindAsync(id)
                is Song song
                    ? TypedResults.Ok(song)
                    : TypedResults.NotFound();
        }

        static async Task<IResult> SaveSong(Song song, localDb db)
        {
            db.Songs.Add(song);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/songitems/{song.Id}", song);
        }

        static async Task<IResult> UpdateSong(int id, Song inputSong, localDb db)
        {
            var song = await db.Songs.FindAsync(id);

            if (song is null) return TypedResults.NotFound();

            song.Name = inputSong.Name;
            song.Artist = inputSong.Artist;

            await db.SaveChangesAsync();

            return TypedResults.Ok(song);
        }

        static async Task<IResult> DeleteSong(int id, localDb db)
        {
            if (await db.Songs.FindAsync(id) is Song song)
            {
                db.Songs.Remove(song);
                await db.SaveChangesAsync();
                return TypedResults.NoContent();
            }

            return TypedResults.Ok();
        }


        // --------- Playlist Functions ---------


        static async Task<IResult> GetAllPlaylists(localDb db)
        {
            return TypedResults.Ok(await db.Playlists.ToArrayAsync());
        }

        static async Task<IResult> GetPlaylist(int id, localDb db)
        {
            return await db.Playlists.FirstOrDefaultAsync(p => p.Id == id)
                is Playlist playlist
                    ? TypedResults.Ok(playlist)
                    : TypedResults.NotFound();
        }

        static async Task<IResult> SavePlaylist(Playlist playlist, localDb db)
        {
            db.Playlists.Add(playlist);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/playlists/{playlist.Id}", playlist);
        }

        static async Task<IResult> UpdatePlaylist(int id, Playlist inputPlaylist, localDb db)
        {
            var playlist = await db.Playlists.FirstOrDefaultAsync(p => p.Id == id);

            if (playlist is null) return TypedResults.NotFound();

            playlist.Name = inputPlaylist.Name;
            playlist.Songs = inputPlaylist.Songs;

            await db.SaveChangesAsync();

            return TypedResults.Ok(playlist);
        }

        static async Task<IResult> DeletePlaylist(int id, localDb db)
        {
            if (await db.Playlists.FindAsync(id) is Playlist playlist)
            {
                db.Playlists.Remove(playlist);
                await db.SaveChangesAsync();
                return TypedResults.NoContent();
            }

            return TypedResults.NotFound();
        }

    }
}