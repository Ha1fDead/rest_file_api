using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace maplarge_restapicore
{
    public class Program
    {
        private static Dictionary<string, string> appConfig = new Dictionary<string, string>()
        {
            // configure where the root directory of the File Server is pointing at
            // Files above this directory will be inaccessible
            {"root_server_directory", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)},

            // Following params are for the PhysicalFileProvider settings "Exclude Options"
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.fileproviders.physical.exclusionfilters?view=aspnetcore-2.2
            // If true, hidden files will be affected and shown by the API
            {"show_hidden", "false"},
            // If true, files starting with a dot (".") will be shown
            {"show_dot", "false"},
            // If true, files set by the system will be shown
            {"show_system", "false"},
        };
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) => {
                    config.AddInMemoryCollection(appConfig);
                })
                .UseStartup<Startup>();
    }
}
