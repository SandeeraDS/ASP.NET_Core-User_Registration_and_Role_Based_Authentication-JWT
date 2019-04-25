using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Models;

namespace WebAPI
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

            //Inject AppSettings
            services.Configure<ApplicationSetting>(Configuration.GetSection("ApplicationSettings"));



            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //here pass the parameters for options in the dbContext - provider and connection string
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //this function call add common services from identity core in to this application
            services.AddDefaultIdentity<ApplicationUser>()
                .AddRoles<IdentityRole>() 
                .AddEntityFrameworkStores<ApplicationDbContext>(); // ad ef core implementations to identity core

            //customizing identity user validation in password
            services.Configure<IdentityOptions>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 4; // 1st user - 1234
                }
            );

            services.AddCors();

            //access from the appsettings.json using configuration
            var key = Encoding.UTF8.GetBytes(Configuration["ApplicationSettings:JWT_Secret"].ToString());
            //JWT authentication
            services.AddAuthentication(x =>
            {
                //set different type of authentication schema as jwt authentication
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => //configure jwt
            {
                x.RequireHttpsMetadata = false; //only for https. we false it here
                x.SaveToken = false; //after successful authentication token is save in the server or not
                //how we do the validate of the token once we receive from the client after successful authentication
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    //validate security key
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                    
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => { builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod(); });
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
