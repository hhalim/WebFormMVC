using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RestApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IContainer ApplicationContainer { get; private set; }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //CORS
            var corsSettings = Configuration.GetSection("CORSSettings");
            var allowOrigins = Array.ConvertAll(corsSettings["AllowOrigin"].Split(','), p => p.Trim());
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", b => b.WithOrigins(allowOrigins)
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(15)) //Cache CORS preflight OPTIONS
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IConfiguration>(Configuration);

            var serviceProvider = ConfigureAutofac(services); //Autofac
            return serviceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,  IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Use CORS
            //Note: Doesn't work in IE 8/9 or Compatibility mode
            //app.UseCors(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); //Dangerous to allow any origin, okay for debugging
            app.UseCors("AllowSpecificOrigin");

            //Logging
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            // AutoFac DI - if you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(() => this.ApplicationContainer.Dispose());
        }

        //Autofac DI configuration and registrations
        private IServiceProvider ConfigureAutofac(IServiceCollection services)
        {
            //Autofac DI
            //http://docs.autofac.org/en/latest/integration/aspnetcore.html

            //REMOVED: properties injection not used in this solution, but optional.
            //Add ControllerActivator to autofac registration, used for Properties DI in controller, not just Constructor
            //https://github.com/autofac/Autofac/issues/713

            var builder = new ContainerBuilder();

            // Register DataLayer
            // NOTE: This Project has direct references to the DAL.MSSQL and DAL.Oracle, which is okay for Proof of Concept here.
            // The DAL.MSSQL.dll and DAL.Oracle.dll can be dropped into the /bin folder through a build script.
            // No code in this project should be "using DAL.MSSQL;" or "using DAL.Oracle" directly.
            // Instead, use Domain.Repository interfaces if need to use any repository infrastructure.
            var dalSettings = Configuration.GetSection("DalSettings");
            var assemblyName = dalSettings["Assembly"];
            var connectionString = dalSettings["ConnectionString"];
            var dalName = new AssemblyName(assemblyName);
            var dalAssembly = Assembly.Load(dalName);
            builder.RegisterAssemblyTypes(dalAssembly)
                .Where(t => t.IsClass && t.Name.EndsWith(dalSettings["ContextEndsWith"]))
                .AsImplementedInterfaces()
                .WithParameter("connectionString", connectionString);
            builder.RegisterAssemblyTypes(dalAssembly).Where(t => t.IsClass && t.Name.EndsWith(dalSettings["RepositoryEndsWith"])).AsImplementedInterfaces();

            // Register Business.Services business layer
            // NOTE: This Project has direct references to the Business.Services, which is okay for Proof of Concept here.
            // Another option is to have the Business.Services.dll can be dropped into the /bin folder in production using build script instead.
            // No code in this project should be "using Business.Services" directly.
            // Instead, use the Domain.Services interfaces to use the business services.
            var servicesSettings = Configuration.GetSection("ServicesSettings");
            var servicesList = Array.ConvertAll(servicesSettings["Assembly"].Split(';'), p => p.Trim());
            foreach (var name in servicesList)
            {
                var servicesName = new AssemblyName(name);
                var servicesAssembly = Assembly.Load(servicesName);
                builder.RegisterAssemblyTypes(servicesAssembly).Where(t => t.IsClass).PropertiesAutowired().AsImplementedInterfaces();
            }

            builder.Populate(services);
            var container = builder.Build();
            ApplicationContainer = container;

            return new AutofacServiceProvider(ApplicationContainer);
        }

    }
}
