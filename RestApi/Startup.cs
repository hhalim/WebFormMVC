using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
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

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IConfiguration>(Configuration);

            var serviceProvider = ConfigureAutofac(services); //Autofac
        }

        public IContainer ApplicationContainer { get; private set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,  IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Use CORS
            //Note: Doesn't work in IE 8/9 or Compatibility mode
            //app.UseCors(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); //Dangerous to allow any origin
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
        private AutofacServiceProvider ConfigureAutofac(IServiceCollection services)
        {
            //Autofac DI
            //http://docs.autofac.org/en/latest/integration/aspnetcore.html

            //Add ControllerActivator to autofac registration, used for Properties DI in controller, not just Constructor
            //https://github.com/autofac/Autofac/issues/713
            services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());

            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(Assembly.GetExecutingAssembly()));
            manager.FeatureProviders.Add(new ControllerFeatureProvider());
            var feature = new ControllerFeature();
            manager.PopulateFeature(feature);

            var builder = new ContainerBuilder();
            builder.RegisterTypes(feature.Controllers.Select(ti => ti.AsType()).ToArray()).PropertiesAutowired();

            //Register DataLayer
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

            //Register Business.Services business layer
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
            this.ApplicationContainer = container;

            //Register injectable container, to allow use in Factory model
            var builder2 = new ContainerBuilder();
            builder2.RegisterInstance<IContainer>(container);
            builder2.Update(container);

            return new AutofacServiceProvider(this.ApplicationContainer);
        }

    }
}
