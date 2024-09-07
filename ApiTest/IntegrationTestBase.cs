using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ApiTest;

public abstract class IntegrationTestBase
{
    protected readonly HttpClient _client;

    protected IntegrationTestBase(HttpClient client)
    {
        _client = client;
    }

    [Fact]
    public async Task Get_Playlists_Returns_Ok()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/playlists");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // 200-299 status code
    }

    [Fact]
    public async Task Get_Songs_Returns_Ok()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/songs");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // 200-299 status code
    }

    // Add more tests as needed
}

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



