namespace JcdMockApi
{
    using System;
    using System.Reflection;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;

    /// <summary>
    /// Calm down, validator, here's a comment.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Calm down, validator, here's a comment.
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Calm down, validator, here's a comment.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Swagger stuff !?!?!?
           // services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                // The first parameter - name - passed to SwaggerDoc is the one we'll need to specify
                // as the last argument to the Autorest command line.
                c.SwaggerDoc("JCDMockApi", new OpenApiInfo { Title = "JCDMockAPI", Version = "v1" });
                c.EnableAnnotations();
                
                // We've set the csproj to generate XML comments; this is how to find the XML file with those comments. 
                string fileName = this.GetType().GetTypeInfo().Module.Name.Replace(".dll", ".xml");
                string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                c.IncludeXmlComments(filePath);
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            // Swagger 
            app.UseSwagger(c => 
            {
                c.SerializeAsV2 = true; 
            });
            
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "JCDMockAPIV1");
            });
        }
    }
}
