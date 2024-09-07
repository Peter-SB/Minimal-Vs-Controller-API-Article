using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiTest;

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
