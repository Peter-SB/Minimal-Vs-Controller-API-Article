using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ApiTest.ApiTestFactories;

namespace ApiTest;

public class ApiTestFactoryProvider
{
    public static IApiTestFactory CreateFactory(Type apiFactoryType, IApiTestFactory minimalApiFactory, IApiTestFactory controllerApiFactory)
    {
        if (apiFactoryType == typeof(MinimalApiTestFactory))
        {
            return minimalApiFactory;
        }
        else if (apiFactoryType == typeof(ControllerApiTestFactory))
        {
            return controllerApiFactory;
        }
        else
        {
            throw new ArgumentException("Invalid factory type", nameof(apiFactoryType));
        }
    }
}
