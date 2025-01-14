using AutoMapper;
using Hiper.Academia.AspNetCore.Database.Context;
using Hiper.Academia.AspNetCore.Repositories.IoC;
using Hiper.Academia.AspNetCore.Services.IoC;
using Hiper.Academia.AspNetCore.Web.Extensions;
using Hiper.Academia.AspNetCore.Web.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Hosting;

namespace Hiper.Academia.AspNetCore.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseTimeCheck();
            app.UseRouting();

            app.UseMvc()
                .UseApiVersioning();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<IHiperAcademiaContext, HiperAcademiaContext>(opt => opt.UseInMemoryDatabase(databaseName: "teste"));
            MigrateDatabase(services);

            services
                .AddMvc(m => { m.EnableEndpointRouting = false; })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddApiVersioning(s =>
            {
                s.DefaultApiVersion = new ApiVersion(1, 0);
                s.ReportApiVersions = true;
                s.AssumeDefaultVersionWhenUnspecified = true;
            });
            services.AddRazorPages();

            IoCServices.Register(services);
            IoCRepositories.Register(services);

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new HiperAcademiaProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        private void MigrateDatabase(IServiceCollection services)
        {
            using var serviceScope = services.BuildServiceProvider().CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<IHiperAcademiaContext>();

            //if (context.AllMigrationsApplied()) return;

            //context.Database.Migrate();
            context.Seed();
        }
    }
}