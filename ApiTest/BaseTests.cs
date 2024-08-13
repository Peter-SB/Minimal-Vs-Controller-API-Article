using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Data;
using Xunit;

public class SongApiTests : IClassFixture<WebApplicationFactory<MinimalApi.Program>>, IDisposable
{
    private readonly WebApplicationFactory<MinimalApi.Program> _factory;
    private readonly IServiceScope _scope;
    private readonly localDb _dbContext;

    public SongApiTests(WebApplicationFactory<MinimalApi.Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the DB context with SQLite in-memory for testing
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<localDb>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<localDb>(options =>
                {
                    options.UseSqlite("DataSource=:memory:");
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context (localDb).
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<localDb>();

                    // Ensure the database is created.
                    db.Database.OpenConnection();
                    db.Database.EnsureCreated();
                }
            });
        });

        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<localDb>();
    }

    [Fact]
    public async Task GetAllSongs_ReturnsEmptyList_Initially()
    {
        // Act
        var response = await _factory.CreateClient().GetAsync("/songs");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        var songs = await response.Content.ReadAsStringAsync();
        Assert.Equal("[]", songs);
    }

    [Fact]
    public async Task SaveSong_AddsSongToDatabase()
    {
        // Arrange
        var client = _factory.CreateClient();
        var newSong = new Song { Id = 1, Name = "Test Song", Artist = "Test Artist" };

        // Act
        var postResponse = await client.PostAsJsonAsync("/songs", newSong);
        postResponse.EnsureSuccessStatusCode();

        // Assert
        var getResponse = await client.GetAsync("/songs");
        getResponse.EnsureSuccessStatusCode();
        var songs = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("Test Song", songs);
    }

    public void Dispose()
    {
        // Clear the database after each test
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _scope.Dispose();
    }
}
