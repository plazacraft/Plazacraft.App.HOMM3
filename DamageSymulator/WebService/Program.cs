using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plazacraft.HOMM3.DamageSymulator.Core;

namespace Plazacraft.HOMM3.DamageSymulator.WebService
{
    public class Program
    {
        public static World World;
        public static void Main(string[] args)
        {
            string file = File.ReadAllText("Config/Definition.xml");
            World = new World(file);
            

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
