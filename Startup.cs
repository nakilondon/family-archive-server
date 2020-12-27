using System.IO;
using LightInject;
using family_archive_server.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using family_archive_server.DependencyInjection;
using family_archive_server.RepositoriesDb;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace family_archive_server
{
    public class Startup
    {
        public IWebHostEnvironment HostingEnvironment { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureContainer(IServiceContainer container)
        {
            container.RegisterFrom<MapperConfigurationRoot>();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IFamilyRepository, FamilyRepository>();
            services.AddSingleton<IImagesRepository, ImagesRepository>();
            services.AddSingleton<IImagesDbRepository, ImagesDbRepository>();
            services.AddSingleton<IPersonRepository, PersonRepository>();

            var pathToKey = Path.Combine(Directory.GetCurrentDirectory(), "firebase_admin_sdk.json");

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(pathToKey)
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var firebaseProjectName = Configuration["FirebaseProjectName"];
                    options.Authority = "https://securetoken.google.com/" + firebaseProjectName;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "https://securetoken.google.com/" + firebaseProjectName,
                        ValidateAudience = true,
                        ValidAudience = firebaseProjectName,
                        ValidateLifetime = true
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    //public class Startup
    //{
    //    public IWebHostEnvironment HostingEnvironment { get; set; }

    //    public Startup(IConfiguration configuration)
    //    {
    //        Configuration = configuration;
    //    }

    //    public void ConfigureContainer(IServiceContainer container)
    //    {
    //        container.RegisterFrom<MapperConfigurationRoot>();
    //    }

    //    public IConfiguration Configuration { get; }

    //    // This method gets called by the runtime. Use this method to add services to the container.
    //    public void ConfigureServices(IServiceCollection services)
    //    {
    //        services.AddControllers();
    //        services.AddSingleton<IFamilyRepository, FamilyRepository>();
    //        services.AddSingleton<IImagesRepository, ImagesRepository>();
    //        services.AddSingleton<IPersonRepository, PersonRepository>();

    //        services.AddControllers();

    //        var pathToKey = Path.Combine(Directory.GetCurrentDirectory(), "firebase_admin_sdk.json");

    //        //   if (HostingEnvironment.IsEnvironment("local"))
    //        //       pathToKey = Path.Combine(Directory.GetCurrentDirectory(), "keys", "firebase_admin_sdk.local.json");

    //        //  FirebaseApp.Create(new AppOptions
    //        //  {
    //        //      Credential = GoogleCredential.FromFile(pathToKey)
    //        //  });

    //        //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    //        //    .AddJwtBearer(options =>
    //        //    {
    //        //        var firebaseProjectName = Configuration["FirebaseProjectName"];
    //        //        options.Authority = "https://securetoken.google.com/" + firebaseProjectName;
    //        //        options.TokenValidationParameters = new TokenValidationParameters
    //        //        {
    //        //            ValidateIssuer = true,
    //        //            ValidIssuer = "https://securetoken.google.com/" + firebaseProjectName,
    //        //            ValidateAudience = true,
    //        //            ValidAudience = firebaseProjectName,
    //        //            ValidateLifetime = true
    //        //        };
    //        //    });

    //    }

    //    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    //    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    //    {
    //        if (env.IsDevelopment())
    //        {
    //            app.UseDeveloperExceptionPage();
    //        }

    //        app.UseStaticFiles();
    //        app.UseHttpsRedirection();

    //        app.UseRouting();

    //        //app.UseAuthentication();
    //        //app.UseAuthorization();

    //        app.UseEndpoints(endpoints =>
    //        {
    //            endpoints.MapControllers();
    //        });
    //    }
   // }
}
