# Comparing Minimal and Controller-Based APIs in ASP.NET

When building APIs in ASP.NET Core, there are two primary approaches: **Minimal APIs** and **Controller-Based APIs**. Each has its own advantages, and choosing the right one depends on the specific project needs. Minimal APIs are ideal for lightweight, small-scale applications where simplicity and speed are important. On the other hand, Controller-based APIs provide more controll and separation of concerns, which is potentially needed for larger applications with more complex requirements.

In this article we will walk through designing a simple CRUD RESTful API, using first the simpler approach, minimal API, then watching how as we expand the functionality of the API, a controller base design could be preferable.

The code for this demo can be found here: https://github.com/Peter-SB/Minimal-Vs-Controller-API-Article

---

---

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

## Upgrading To Controller Base API

Now that we've demonstrated a simple Minimal API, let's rewrite the same API using a controller-based approach.

## Upgrading to a Controller-Based API

While Minimal APIs are great for small projects with straightforward requirements, as your application grows in complexity we benefit from a more structured approach with more controll. This is where Controller-Based APIs come in.

Controller-based APIs follow the clasical MVC (Model-View-Controller) pattern, giving you a clear separation of concerns by isolating the logic that handles HTTP requests (called the controllers) from the rest of your application. This approach is especially useful when you're dealing with larger applications, as it makes your codebase easier to organize and maintain. With this controllers-based approach, you also get built-in support for features like routing, validation, and model binding, which can make development more efficient as your API expands.

Now that we’ve demonstrated a Minimal API, let's see how switching to a controller-based design compares with the more structured approach.

### Program.cs

As we move the endpoint code to separate controller classes, we now have a much smaller `Program.cs` file. We set up our `WebApplication` in a similar way, but with a few key changes.

First change, this `builder.Services.AddControllers();` is what registers the support for the controllers to the app.

The other important difference is the `app.MapControllers();` which does the mapping from the controller routes to the right endpoints

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Same as our Minimal Api example
builder.Services.AddDbContext<localDb>(opt => {
    opt.UseSqlite("Data Source=:memory:");
}, ServiceLifetime.Singleton);

var app = builder.Build();

// Middleware redirects HTTP requests to HTTPS
app.UseHttpsRedirection();

//  Middleware adds authorization support based on user permissions
app.UseAuthorization();

app.MapControllers();

app.Run();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<localDb>();
    dbContext.Database.OpenConnection();
    dbContext.Database.EnsureCreated();
}
```

### Controllers

**SongsController.cs**

Similar to before, this time we have our endpoints in its own class.

Now `[Route("/[controller]")]` attribute sets the URL prefixes. The `[controller]` is a placeholder and will be replaced with `songs` since the controller name is `SongsController` .

The `[ApiController]` attribute marks the class as an API controller, enabling features like automatic model validation and binding errors.

In the class constructor localDb is taken as a parameter provided by dependancy injection.

Lastly, notice how the function attributes like `[HttpGet]`, `[HttpPost]`, etc… specify the HTTP methods the controller actions should respond to.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

```

**PlaylistController.cs**

Similar to our songs controller but with playlist functionality.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        return await _db.Playlists.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Playlist>> GetPlaylist(int id)
    {
        var playlist = await _db.Playlists.FirstOrDefaultAsync(p => p.Id == id);

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
        var playlist = await _db.Playlists.FirstOrDefaultAsync(p => p.Id == id);

        if (playlist == null)
        {
            return NotFound();
        }

        playlist.Name = inputPlaylist.Name;
        playlist.Songs = inputPlaylist.Songs;

        await _db.SaveChangesAsync();

        return Ok(playlist);
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

```

With a bit more setup, the controller-based approach offers better separation of concerns, making it easier to write and maintain APIs, especially as they grow in complexity.

### Pros:

- **Neater Code Organization:** Controllers group related endpoints together, making the code easier to navigate, particularly in larger applications.
- **Scalability:** Controller-based APIs handle larger projects with numerous endpoints more effectively, providing a structured approach that’s ideal for enterprise-level systems.
- **Built-In Routing and Features:** Unlike Minimal APIs, controllers come with built-in routing, validation, and model binding, reducing the need for custom configurations and making the code more maintainable as the project grows.

### Cons:

- **More Boilerplate Code:** The controller-based approach requires additional setup, such as defining controller classes and attributes, leading to more boilerplate.
- **Overhead for Small Projects:** For small APIs, the extra layers and setup may be unnecessary when a Minimal API could handle the same tasks with less code.

# Testing

## Manual Testing With Swagger

Swagger offers a user-friendly UI for documenting and interacting with your API, making it useful for quick manual testing.

You can add Swagger by modifying the `Program.cs` file as follows:

```jsx
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Controller Api Demo", Version = "v1" });
});

var app = builder.Build();  // This will already be in Program.cs

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

With this setup, Swagger will generate a UI at `/swagger` where you can interact with the API, making it much easier to manually test the endpoints and see live documentation. This is especially useful for early stages of development.

## Integration vs. Unit Testing:

A quick note on the difference between integration testing and unit testing.

- **Integration Tests**: These tests verify that different parts of the application work together as expected. They check the interaction between components, such as databases, services, and the HTTP request pipeline.
- **Unit Tests**: These focus on individual components or methods, ensuring they work in isolation from the rest of the system. They usually focus on testing single functions with mocked services.

## Integration Test

No code is complete without some good unit or integration tests to quickly and repeatably ensure that your API works as expected and you’ve not introduced any bugs.

Since we have two APIs that should functionally work the same, we can write a single set of integration tests to test both. We'll write these tests in a base test class, and implement specific test classes for each API. This structure allows us to avoid duplicating test logic. The base class will contain the core test logic, and each API will pass its own `HttpClient` to the base class for execution. By doing this, we ensure that both the Minimal and Controller-based APIs go through the same tests, confirming that they behave identically.

We’ll use `WebApplicationFactory` from `Microsoft.AspNetCore.Mvc.Testing` to create a test server and generate `HttpClient` instances. The `WebApplicationFactory` class mimics how the application is run in production, setting up a real test host, middleware, and dependency injection. Each specific API test class (Minimal and Controller-based) will pass its own `WebApplicationFactory` into the base class, allowing the same set of tests to be run against different implementations.

```jsx
using Shared.Data;
using System.Net.Http.Json;
using Xunit;

public abstract class IntegrationTestBase
{
    protected readonly HttpClient _client;

    protected IntegrationTestBase(HttpClient client)
    {
        _client = client;
    }

    [Fact]
		[Trait("Category", "Song Endpoint Tests")]
		public async Task Songs_Create_Song()
		{
		    // Arrange
		    var newSong = new Song { Id = 1, Name = "Test Song", Artist = "Test Artist" };

		    // Act
		    var response = await _client.PostAsJsonAsync("/songs", newSong);

		    // Assert
		    response.EnsureSuccessStatusCode();
		    var createdSong = await response.Content.ReadFromJsonAsync<Song>();
		    Assert.Equal(newSong.Name, createdSong.Name);
		}

		[Fact]
		[Trait("Category", "Song Endpoint Tests")]
		public async Task Songs_Get_Song_ById()
		{
		    // Arrange
		    var newSong = new Song { Id = 1, Name = "Test Song", Artist = "Test Artist" };
		    var createResponse = await _client.PostAsJsonAsync("/songs", newSong);
		    var createdSong = await createResponse.Content.ReadFromJsonAsync<Song>();

		    // Act
		    var response = await _client.GetAsync($"/songs/{createdSong.Id}");

		    // Assert
		    response.EnsureSuccessStatusCode();
		    var retrievedSong = await response.Content.ReadFromJsonAsync<Song>();
		    Assert.Equal(createdSong.Id, retrievedSong.Id);
		}

    // Implement more tests here ....
    // See Github code for examples.

}
```

Now that we have a base class for integration testing, we can create separate test classes for the Minimal API and the Controller-based API:

```jsx
public class MinimalApiIntegrationTests : IntegrationTestBase
{
    public MinimalApiIntegrationTests()
        : base(new WebApplicationFactory<MinimalApi.Program>().CreateClient())
    {
    }
}

public class ControllerBasedApiIntegrationTests : IntegrationTestBase
{
    public ControllerBasedApiIntegrationTests()
        : base(new WebApplicationFactory<ControllerApi.Program>().CreateClient())
    {
    }
}

```

# Quick Conclusion

We've now explored two different approaches for building APIs in [ASP.NET](http://asp.net/) Core. Minimal APIs as demonstrated can be more useful for smaller applications or microservices where speed and simplicity are needed. However, as we've seen, as our program grew the lack of structure and separation of concerns can lead to clutter in `Program.cs`, and managing all the endpoints in one file can become challenging.

We’ve also seen how Controller-Based APIs are more structured and I’ve explained how their built-in support for more advanced features can make them a better choice for larger applications that may need to scale over time.

# Lessons I Learned

It is always valuable to make time to reflect on what has been learned and practiced. Here are some of the other takeaways I have from this article:

- **Always Plan Longer for Testing** - I’ve been a software engineer long enough to know this, that testing always takes longer than you expect, and here was no difference. Testing is not an afterthought and good automated tests are invaluable. Approaches such as Test Driven Design (TDD) put this front and center. While I would prefer this approach, sometimes it’s not very feasible. I was still learning while I wrote this article so it was difficult to follow a TDD but I should always remember to factor in good time for it testing and writing automated tests.
- **Singletons!!** - The amount of time I spent while writing this article just battling an SQLite database that didn't work was exhausting. Finally, I tracked down that the database was not connected while running my requests, and then only after that did I realize that my database was being disposed off. `ServiceLifetime.Singleton` stopped it being binned and fixed all my problems. This brings me onto my last point
- **Debugging and Docs Practice:** A large part of being a software developer is debugging—both in using the tools in your IDE to debug your code and in researching and solving problems. This article gave me great practice in breaking down problems, testing different solutions, and pushing through challenges. Additionally, reading documentation is also a core skill that always benefits from practice. Researching and reading docs, other articles, and forum posts for this article was good practice ability to find reliable sources and quickly locate the information I need.

# References:

1. Choose between controller-based APIs and minimal APIs - Microsoft: [https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis?view=aspnetcore-8.0](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis?view=aspnetcore-8.0)
2. Controllers vs Minimal APIs - Lumythys: [https://www.reddit.com/r/dotnet/comments/17t27cv/comment/k8tzgot/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button](https://www.reddit.com/r/dotnet/comments/17t27cv/comment/k8tzgot/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button)
3. Minimal APIs quick reference - Microsoft: [https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-8.0](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-8.0)
4. Tutorial: Create a minimal API with [ASP.NET](http://asp.net/) Core - Microsoft: [https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-8.0&tabs=visual-studio](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-8.0&tabs=visual-studio)
5. Tutorial: Create a web API with [ASP.NET](http://asp.net/) Core - Microsoft: [https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-8.0&tabs=visual-studio](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-8.0&tabs=visual-studio)
