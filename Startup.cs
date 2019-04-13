using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace rest_file_api
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            if (!Directory.Exists(config["root_server_directory"]))
            {
                Console.WriteLine("Configuration for root server public path invalid -- ensure path is valid and re-run program");
                Environment.Exit(1);
            }

            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var exclusions = ExclusionFilters.None;
            if (!Boolean.Parse(_config["show_hidden"]))
            {
                exclusions |= ExclusionFilters.Hidden;
            }
            if (!Boolean.Parse(_config["show_dot"]))
            {
                exclusions |= ExclusionFilters.DotPrefixed;
            }
            if (!Boolean.Parse(_config["show_system"]))
            {
                exclusions |= ExclusionFilters.System;
            }

            services.AddResponseCompression();
            services.AddMvc();
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(
                _config["root_server_directory"],
                exclusions
            ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();
            app.UseMvc();
            app.UseStatusCodePagesWithReExecute("/");
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
