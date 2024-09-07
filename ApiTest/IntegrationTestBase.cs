using Shared.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

public abstract class IntegrationTestBase
{
    protected readonly HttpClient _client;

    protected IntegrationTestBase(HttpClient client)
    {
        _client = client;
    }


    [Fact]
    public async Task Create_Song_Returns_Created()
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
    public async Task Get_Song_ById()
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

    [Fact]
    public async Task Update_Song()
    {
        // Arrange
        var newSong = new Song { Id = 1, Name = "Test Song", Artist = "Test Artist" };
        var createResponse = await _client.PostAsJsonAsync("/songs", newSong);
        var createdSong = await createResponse.Content.ReadFromJsonAsync<Song>();

        var updatedSong = new Song { Id = 1, Name = "Test Song New", Artist = "Test Artist New" };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/songs/{createdSong.Id}", updatedSong);

        // Assert
        updateResponse.EnsureSuccessStatusCode(); 
        var updatedSongResponse = await updateResponse.Content.ReadFromJsonAsync<Song>();
        Assert.Equal(updatedSong.Name, updatedSongResponse.Name);
    }

    [Fact]
    public async Task Delete_Song()
    {
        // Arrange
        var newSong = new Song { Id = 1, Name = "Test Song", Artist = "Test Artist" };
        var createResponse = await _client.PostAsJsonAsync("/songs", newSong);
        var createdSong = await createResponse.Content.ReadFromJsonAsync<Song>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/songs/{createdSong.Id}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify that the song is deleted
        var getResponse = await _client.GetAsync($"/songs/{createdSong.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Get_All_Songs()
    {
        // Act
        var response = await _client.GetAsync("/songs");

        // Assert
        response.EnsureSuccessStatusCode();
        var songs = await response.Content.ReadFromJsonAsync<List<Song>>();
        Assert.NotNull(songs);
    }
}



