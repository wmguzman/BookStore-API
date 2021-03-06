using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using BookStore_API.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using BookStore_API.Services;
using AutoMapper;
using BookStore_API.Mappings;
using BookStore_API.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BookStore_API
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(/*options => options.SignIn.RequireConfirmedAccount = true*/)//Lo del comentario requiere registro con confirmacion de email
                .AddRoles<IdentityRole>()//Esta l�nea se agreg� para que Identity funcione con Roles tambi�n
                .AddEntityFrameworkStores<ApplicationDbContext>();

            //Aqu� se configura CORS
            services.AddCors(o =>
            {
                o.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyMethod());
            });

            //Aqu� se configura el servicio Automapper para abstraer y conectar con las clases que representan la base de datos. Se relaciona con la carpeta Data/Mappings creada
            services.AddAutoMapper(typeof(Maps));

            //Aqu� se configura JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });

            //Aqu� se configura swashbuckler swager que se acaba de instalar 3 modulos usando nuget
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "Book Store API",
                    Version = "v1",
                    Description = "Prueba inicial de esta API"
                });
                var xmlfile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlpath = Path.Combine(AppContext.BaseDirectory, xmlfile);
                c.IncludeXmlComments(xmlpath);
            });

            //Aqu� se inicia el logger que se cre� con NLog
            services.AddSingleton<ILoggerService, LoggerService>();

            //Aqu� se instancian los repositorios de cada servicio que creamos para manejar y conectar con la base de datos.
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IBookRepository, BookRepository>();


            //Modificado para usar  Newtonsoft
            services.AddControllers().AddNewtonsoftJson(op =>
                op.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            //services.AddControllers()
            //services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //Aqu� se configura swashbuckler swager que se acaba de instalar 3 modulos usando nuget
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Book Store API");
                c.RoutePrefix = "";

            });

            app.UseHttpsRedirection();
            //app.UseStaticFiles();

            //Aqu� usamos el CORS que se configur� antes
            app.UseCors("CorsPolicy");

            //Aqu� se inicia los usuarios y roles ficticios para probar
            SeedData.Seed(userManager, roleManager).Wait();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
