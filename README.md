# Comparing Minimal and Controller-Based APIs in ASP.NET

When building APIs in ASP.NET Core, there are two primary approaches: **Minimal APIs** and **Controller-Based APIs**. Each has its own advantages, and choosing the right one depends on the specific project needs. Minimal APIs are ideal for lightweight, small-scale applications where simplicity and speed are important. On the other hand, Controller-based APIs provide a more technical control and separation of concerns, which is beneficial for larger applications with more complex requirements.

In this article we will walk through designing a simple CRUD RESTful API, using first the simpler approach, minimal API, then watching how as we expand the functionality of the API, a controller base design could be preferable.

The code for this demo can be found here: https://github.com/Peter-SB/Minimal-Vs-Controller-API-Article

# Minimal API

Minimal APIs were introduced in ASP.NET Core 6.0 as a way to build HTTP APIs with less boilerplate code. They’re perfect for small, simple applications or microservices, where you don't need the normal overhead of MVC (Model-View-Controller) patterns.

The main purpose of Minimal APIs is to simplify development by allowing you to write API endpoints quickly and without the need for controllers. It focuses on keeping the setup minimal, allowing to do everything directly in the `Program.cs` file. While this can reduce code complexity in small apps, the structure might not scale well as we will see shortly.

Let’s dive into our example.

## Simple CRUD API

We will start our example by building a simple CRUD API to store songs, and then later playlists, in an inmemory SQLite database. We are going with an SQLlite in-memory database since it's lightweight, serverless, and perfect for our small-scale demonstrative purposes.

### Song Data Structure

This entity class will represent a schema for our Song object.

```csharp
public class Song
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Artist { get; set; }
}
```

(Note: `required` modifier was made available beginning with C# 11)

### In Memory SQLite Database

We will extend the `DbContext` class from the Entity Framework Core library as our Object Relational Mapping (ORM) framework.

```csharp
using Microsoft.EntityFrameworkCore;

public class localDb : DbContext
{
    public localDb(DbContextOptions<localDb> options): base(options) { }

    public DbSet<Song> Songs => Set<Song>();
    public DbSet<Playlist> Playlists => Set<Playlist>(); // Used later in the article demo
}
```

### Program.cs

In the first section of our Program.cs file, we set up the web app using WebApplication.CreateBuilder to initialize a new instance of the WebApplicationBuilder class with pre-configured defaults. We also add the in-memory SQLite database.

The `builder.Services` gives access to the `IServiceCollection`, which is used to register services for dependency injection.

```jsx
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
```

Here is where we set up our SQLite database.

This adds our `localDb` context to the dependency injection container and sets it up to use it in-memory.

```jsx
builder.Services.AddDbContext <
  localDb >
  ((opt) => {
    opt.UseSqlite("Data Source=:memory:");
  },
  ServiceLifetime.Singleton); // Needed to keep the in memory database from being disposed
```

The `ServiceLifetime.Singleton` is very important, it ensures that the database context lives for the entire lifetime of the application. Without this, the in-memory database would get disposed of after each request, meaning you'd lose all data between requests. Believe me, I found this out the hard way.

`builder.Build()` finalizes the `WebApplication` configurations and makes it ready to handle requests.

```csharp
var app = builder.Build();
```

We now need to add our endpoints. We can quickly declare an endpoint using a lambda function like this:

```csharp
app.MapGet("/songs", async (localDb db) =>
    await db.Songs.ToListAsync());
```

But for ease of readability and maintainability, we can group our endpoints by URL prefixes and pass named functions, like so:

```csharp
var songItems = app.MapGroup("/songs");

songItems.MapGet("/", GetAllSongs);
songItems.MapGet("/{id}", GetSong);
songItems.MapPost("/", SaveSong);
songItems.MapPut("/{id}", UpdateSong);
songItems.MapDelete("/{id}", DeleteSong);
```

By grouping endpoints like this, we make your code cleaner and easier to maintain, especially as your project grows. While Minimal APIs allow you to define routes inline, organizing them with `MapGroup` helps us keep the structure logical and scalable.

Finally, we start the web API and tell it to start listening for incoming HTTP requests. After starting the API we need to initialize our SQLite database and then ensure its been created.

```csharp
app.Run();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<localDb>();
    dbContext.Database.OpenConnection();
    dbContext.Database.EnsureCreated();  // Creates the schema in the in-memory database
}
```

Here are our named CRUD function code for our endpoints:

```csharp
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
```

## Extending Our Minimal CRUD API’s Functionality

Now, for demonstrative purposes, let's extend the functionality of our API. Now, we would like to add playlists and allow songs to be added to playlists. One of the benefits of using a Minimal APIs approach is that it’s easy to tack on additional functionality without much effort.

### Data Structure

Let's start by adding the playlist data structure for our playlist entity and endpoints. This class will represent our Playlist object

```csharp
public class Playlist
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required List<int> Songs { get; set; } = new List<int>();
}
```

The line of code for the database should already be in the localDb class

### Program.cs

Just like we did with songs, we map our playlist routes for clean organization

```csharp
var playlistItems = app.MapGroup("/playlists");

playlistItems.MapGet("/", GetAllPlaylists);
playlistItems.MapGet("/{id}", GetPlaylist);
playlistItems.MapPost("/", SavePlaylist);
playlistItems.MapPut("/{id}", UpdatePlaylist);
playlistItems.MapPost("/{playlistId}/songs/{songId}", AddSongToPlaylist);
playlistItems.MapDelete("/{id}", DeletePlaylist);
```

Here are our function definitions for our new playlist CRUD endpoints

```csharp
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
```

We have now quickly set up a CRUD API for adding songs to an SQLite database and adding playlists of songs too. While the setup was very quick and we didn't need much code to get started, as the functionality of our program grew our Program.cs file also grew and is now looking quite large. A potential version control (Git) nightmare. If you wanted to add more features or refactor this code, having all your logic in one file would make maintenance more challenging. This approach works well when your project is small, but as you scale, it can become more difficult to manage.

### Pros

- **Faster setup:** With Minimal APIs, you can get your application up and running much faster compared to the more rigid structure of Controller-based APIs. You can skip the boilerplate code and can define routes and logic in a few lines.
- **Lightweight for small apps:** For applications where performance and minimal overhead are essential, such as microservices, Minimal APIs are a great fit. You have access to a lot of the same features available in Controller-based APIs, but without the additional setup.
- **Readable for simple use cases:** When dealing with just a few endpoints, having everything defined in one place can make the code easier to follow at first glance.

### Cons

- **Difficulty in scaling and less structure:** As your application grows and you add more functionality, having all routes, logic, and dependencies handled in one file becomes unmanageable. Refactoring becomes harder, and keeping track of various endpoints and services can lead to a "spaghetti code" situation.
- **Harder Dependency Injection:** Minimal APIs don’t handle dependency injection as seamlessly as Controller-based APIs. While you can inject services into endpoint handlers, it gets tricky when you need more complex dependencies. You might need to access the service provider manually or implement custom middleware, which can introduce unnecessary complexity.
- **Limited middleware capabilities:** Minimal APIs support basic middleware, but for more advanced scenarios, like custom authentication, authorization, or complex request pipelines, Controller-based APIs might be a better fit.

# Controller Based API
