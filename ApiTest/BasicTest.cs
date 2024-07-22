using ApiTest;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Data;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using static ApiTest.ApiTestFactories;
using static ApiTest.ApiTestFactoryProvider;

namespace ApiTest
{
    public class BasicTests : IClassFixture<WebApplicationFactory<MinimalApi.Program>>, IClassFixture<WebApplicationFactory<ControllerApi.Program>>
    {
        private readonly IApiTestFactory _minimalApiFactory;
        private readonly IApiTestFactory _controllerApiFactory;

        public BasicTests(WebApplicationFactory<MinimalApi.Program> minimalApiFactory, WebApplicationFactory<ControllerApi.Program> controllerApiFactory)
        {
            _minimalApiFactory = new MinimalApiTestFactory(minimalApiFactory);
            _controllerApiFactory = new ControllerApiTestFactory(controllerApiFactory);
        }

        public static IEnumerable<object[]> ApiFactories => new List<object[]>
        {
            new object[] { typeof(MinimalApiTestFactory) },
            new object[] { typeof(ControllerApiTestFactory) }
        };

        [Theory]
        [MemberData(nameof(ApiFactories))]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(Type apiFactoryType)
        {
            // Arrange
            var factory = CreateFactory(apiFactoryType, _minimalApiFactory, _controllerApiFactory);
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/songs/");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }

        [Theory]
        [MemberData(nameof(ApiFactories))]
        public async Task CreatePlaylist_ReturnsSuccessStatusCode(Type apiFactoryType)
        {
            var factory = CreateFactory(apiFactoryType, _minimalApiFactory, _controllerApiFactory);
            var client = factory.CreateClient();

            var newPlaylist = new Playlist { Id = 1, Name = "Playlist Name", Songs = [0, 1] };
            var response = await client.PostAsJsonAsync("/playlists/", newPlaylist);

            response.EnsureSuccessStatusCode();
            var createdPlaylist = await response.Content.ReadFromJsonAsync<Playlist>();

            Assert.NotNull(createdPlaylist);
            Assert.Equal(newPlaylist.Name, createdPlaylist.Name);
        }

        [Theory]
        [MemberData(nameof(ApiFactories))]
        public async Task DeletePlaylist_ReturnsSuccessStatusCode(Type apiFactoryType)
        {
            var factory = CreateFactory(apiFactoryType, _minimalApiFactory, _controllerApiFactory);
            var client = factory.CreateClient();

            var newPlaylist = new Playlist { Id = 1, Name = "Playlist Name", Songs = [0, 1] };
            var response = await client.PostAsJsonAsync("/playlists/", newPlaylist);

            response.EnsureSuccessStatusCode();
            var createdPlaylist = await response.Content.ReadFromJsonAsync<Playlist>();

            Assert.NotNull(createdPlaylist);
            Assert.Equal(newPlaylist.Name, createdPlaylist.Name);
        }
    }
}


