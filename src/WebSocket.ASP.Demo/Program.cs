using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Carbon.Match.Networking;
using System;

namespace Carbon.Match
{
    class Program
    {
        static void Main(string[] args)
        {
            new WebHostBuilder().UseKestrel().UseStartup<Startup>().Build().Run();
        }
    }
}

