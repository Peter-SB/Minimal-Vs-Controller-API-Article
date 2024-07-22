using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTest;

public interface IApiTestFactory
{
    HttpClient CreateClient();
}
