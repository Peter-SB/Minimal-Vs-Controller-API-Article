using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Data;
using Xunit;


namespace ApiTest;

public class MinimalApiIntegrationTests : IntegrationTestBase<MinimalApi.Program>
{
    public MinimalApiIntegrationTests()
        : base(new WebApplicationFactory<MinimalApi.Program>())
    {
    }
}

public class ControllerBasedApiIntegrationTests : IntegrationTestBase<ControllerApi.Program>
{
    public ControllerBasedApiIntegrationTests()
        : base(new WebApplicationFactory<ControllerApi.Program>())
    {
    }
}
