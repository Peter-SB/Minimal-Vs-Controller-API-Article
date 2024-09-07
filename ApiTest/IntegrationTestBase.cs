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


    // --------- Song Tests ---------


    [Fact]
    public async Task Create_Song()
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

        // Verify
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


    // --------- Playlist Tests ---------


    [Fact]
    public async Task Create_Playlist()
    {
        // Arrange
        var newPlaylist = new Playlist { Id = 1, Name = "Test Playlist", Songs = new List<int> { 1, 2 } };

        // Act
        var response = await _client.PostAsJsonAsync("/playlists", newPlaylist);

        // Assert
        response.EnsureSuccessStatusCode();
        var createdPlaylist = await response.Content.ReadFromJsonAsync<Playlist>();
        Assert.Equal(newPlaylist.Name, createdPlaylist.Name);
        Assert.Equal(newPlaylist.Songs, createdPlaylist.Songs);
    }

    [Fact]
    public async Task Get_Playlist_ById()
    {
        // Arrange
        var newPlaylist = new Playlist { Id = 1, Name = "Test Playlist", Songs = new List<int> { 1, 2 } };
        var createResponse = await _client.PostAsJsonAsync("/playlists", newPlaylist);
        var createdPlaylist = await createResponse.Content.ReadFromJsonAsync<Playlist>();

        // Act
        var response = await _client.GetAsync($"/playlists/{createdPlaylist.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var retrievedPlaylist = await response.Content.ReadFromJsonAsync<Playlist>();
        Assert.Equal(createdPlaylist.Id, retrievedPlaylist.Id);
    }

    [Fact]
    public async Task Update_Playlist()
    {
        // Arrange
        var newPlaylist = new Playlist { Id = 1, Name = "Test Playlist", Songs = new List<int> { 1, 2 } };
        var createResponse = await _client.PostAsJsonAsync("/playlists", newPlaylist);
        var createdPlaylist = await createResponse.Content.ReadFromJsonAsync<Playlist>();

        var updatedPlaylist = new Playlist { Id = 1, Name = "Updated Playlist", Songs = new List<int> { 2, 3 } };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/playlists/{createdPlaylist.Id}", updatedPlaylist);

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        var updatedPlaylistResponse = await updateResponse.Content.ReadFromJsonAsync<Playlist>();
        Assert.Equal(updatedPlaylist.Name, updatedPlaylistResponse.Name);
        Assert.Equal(updatedPlaylist.Songs, updatedPlaylistResponse.Songs);
    }

    [Fact]
    public async Task Delete_Playlist()
    {
        // Arrange
        var newPlaylist = new Playlist { Id = 1, Name = "Test Playlist", Songs = new List<int> { 1, 2 } };
        var createResponse = await _client.PostAsJsonAsync("/playlists", newPlaylist);
        var createdPlaylist = await createResponse.Content.ReadFromJsonAsync<Playlist>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/playlists/{createdPlaylist.Id}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify
        var getResponse = await _client.GetAsync($"/playlists/{createdPlaylist.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Get_All_Playlists()
    {
        // Act
        var response = await _client.GetAsync("/playlists");

        // Assert
        response.EnsureSuccessStatusCode();
        var playlists = await response.Content.ReadFromJsonAsync<List<Playlist>>();
        Assert.NotNull(playlists);
    }
}



