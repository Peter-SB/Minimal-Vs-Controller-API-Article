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
