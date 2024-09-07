using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
