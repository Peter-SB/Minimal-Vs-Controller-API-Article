using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTest;

public class ApiTestFactories
{
    public class MinimalApiTestFactory : IApiTestFactory
    {
        private readonly WebApplicationFactory<MinimalApi.Program> _factory;

        public MinimalApiTestFactory(WebApplicationFactory<MinimalApi.Program> factory)
        {
            _factory = factory;
        }

        public HttpClient CreateClient()
        {
            return _factory.CreateClient();
        }
    }

    public class ControllerApiTestFactory : IApiTestFactory
    {
        private readonly WebApplicationFactory<ControllerApi.Program> _factory;

        public ControllerApiTestFactory(WebApplicationFactory<ControllerApi.Program> factory)
        {
            _factory = factory;
        }

        public HttpClient CreateClient()
        {
            return _factory.CreateClient();
        }
    }
}
