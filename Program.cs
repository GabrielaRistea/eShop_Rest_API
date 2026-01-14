
using Microsoft.EntityFrameworkCore;
using Proiect.Context;
using Proiect.Repositories.Interfaces;
using Proiect.Repositories;
using Proiect.Services.Interfaces;
using Proiect.Services;
using Microsoft.AspNetCore.Identity;
using Proiect.Models;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace Proiect
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            //builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "eShop API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token like: Bearer <token>"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
            });

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication()
                .AddBearerToken(IdentityConstants.BearerScheme);

            builder.Services.AddIdentityCore<User>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ShopContext>()
                .AddApiEndpoints();

            builder.Services.AddDbContext<ShopContext>(
            optionsBuilder =>
                optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("EShopDb"))
        );

            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
            builder.Services.AddScoped<IWishlistService, WishlistService>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                //app.MapOpenApi();
            }

            //// Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.MapOpenApi();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseDefaultFiles(); // cauta index.html
            app.UseStaticFiles();  // permite accesul la folderul wwwroot

            app.MapIdentityApi<User>();
            app.MapControllers();

            app.Run();
        }
    }
}
